using System.Globalization;
using System.Text.RegularExpressions;

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Media;
using Android.Provider;

using Environment = Android.OS.Environment;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

namespace MPowerKit.MediaPlugin;

[ContentProvider(["${applicationId}" + Authority], Name = "MPowerKit.MediaPlugin.MediaFileProvider", Exported = false, GrantUriPermissions = true)]
[MetaData("android.support.FILE_PROVIDER_PATHS", Resource = "@xml/file_provider_paths")]
public class MediaFileProvider : FileProvider
{
    internal const string Authority = ".fileprovider";

    internal static Uri GetUriForFile(Context context, Java.IO.File file)
        => GetUriForFile(context, context.PackageName! + Authority, file)!;
}

public class State
{
    public int RequestId { get; set; }
    public Uri? Path { get; set; }
    public TaskCompletionSource<List<MediaFile>?> Tcs { get; set; }
    public bool IsPhoto => Type == "image/*";
    public string Action { get; set; }
    public string Type { get; set; }
}

/// <summary>
/// Implementation for Feature
/// </summary>
public partial class MediaImplementation : IMedia
{
    protected const string IllegalCharacters = "[|\\?*<\":>/']";

    [GeneratedRegex(IllegalCharacters)]
    private static partial Regex IllegalCharactersRegex();

    protected Context Context { get; set; }

    protected int RequestId { get; set; }

    protected IList<string> RequestedPermissions;

    protected State? State;

    protected bool IsPicking => State is not null;

    /// <inheritdoc/>
    public virtual bool IsCameraAvailable { get; }
    /// <inheritdoc/>
    public virtual bool IsTakePhotoSupported => true;

    /// <inheritdoc/>
    public virtual bool IsPickPhotoSupported => true;

    /// <inheritdoc/>
    public virtual bool IsTakeVideoSupported => true;
    /// <inheritdoc/>
    public virtual bool IsPickVideoSupported => true;

    /// <summary>
    /// Implementation
    /// </summary>
    public MediaImplementation()
    {
        Context = Android.App.Application.Context;
        IsCameraAvailable = Context.PackageManager!.HasSystemFeature(PackageManager.FeatureCamera)
            || Context.PackageManager.HasSystemFeature(PackageManager.FeatureCameraFront);
    }

    protected virtual int GetRequestId()
    {
        var id = RequestId;
        if (RequestId == int.MaxValue) RequestId = 0;
        else RequestId++;

        return id;
    }

    public async Task<MediaFile?> PickPhotoAsync(PickRequest? request = null, CancellationToken token = default)
    {
        request ??= new PickRequest();

        var medias = await TakeMediasAsync(true, request, null, token);

        var media = medias?.FirstOrDefault();

        return media;
    }

    public async Task<List<MediaFile>?> PickPhotosAsync(PickRequest? request = null, MultiPickerOptions? multiOptions = null, CancellationToken token = default)
    {
        request ??= new PickRequest();

        multiOptions ??= new MultiPickerOptions();

        var medias = await TakeMediasAsync(true, request, multiOptions, token);

        return medias;
    }

    public async Task<MediaFile?> TakePhotoAsync(CaptureRequest? request = null, CancellationToken token = default)
    {
        if (!IsCameraAvailable)
            throw new NotSupportedException("Camera is not available on this device");

        if (!await RequestCameraPermissions())
        {
            throw new MediaPermissionException(nameof(Permissions.Camera));
        }

        request ??= new CaptureRequest();

        VerifyCaptureRequest(request);

        var medias = await TakeMediasAsync(true, request, null, token);

        var media = medias?.FirstOrDefault();

        return media;
    }

    public async Task<MediaFile?> PickVideoAsync(VideoPickRequest? request = null, CancellationToken token = default)
    {
        request ??= new VideoPickRequest();

        var medias = await TakeMediasAsync(false, request, null, token);

        return medias?.FirstOrDefault();
    }

    public async Task<MediaFile?> TakeVideoAsync(CaptureRequest? request = null, CancellationToken token = default)
    {
        if (!IsCameraAvailable)
            throw new NotSupportedException("Camera is not available on this device");

        if (!(await RequestCameraPermissions()))
        {
            throw new MediaPermissionException(nameof(Permissions.Camera));
        }

        request ??= new CaptureRequest();

        VerifyCaptureRequest(request);

        var medias = await TakeMediasAsync(false, request, null, token);

        return medias?.FirstOrDefault();
    }

    protected virtual async Task<List<MediaFile>?> TakeMediasAsync(
        bool isPhoto,
        MediaRequest request,
        MultiPickerOptions? pickerOptions = null,
        CancellationToken token = default)
    {
        if (IsPicking) throw new InvalidOperationException("Only one picking operation can be produced at a time");

        var id = GetRequestId();

        var tcs = new TaskCompletionSource<List<MediaFile>?>(id);
        State = new State()
        {
            Action = request is PickRequest ? Intent.ActionPick : (isPhoto ? MediaStore.ActionImageCapture : MediaStore.ActionVideoCapture),
            RequestId = id,
            Tcs = tcs,
            Type = isPhoto ? "image/*" : "video/*"
        };

        try
        {
            if (token.IsCancellationRequested)
            {
                tcs.SetCanceled(token);
                return await tcs.Task;
            }

            var intent = CreateMediaIntent(State, request, pickerOptions);

            Platform.CurrentActivity!.StartActivityForResult(intent, id);

            token.Register(() =>
            {
                Platform.CurrentActivity!.FinishActivity(id);
            });

            return await tcs.Task;
        }
        finally
        {
            State = null;
        }
    }

    protected virtual Intent CreateMediaIntent(State state, MediaRequest? request, MultiPickerOptions? multiOptions = null)
    {
        var pickIntent = new Intent(state.Action);

        try
        {
            if (state.Action == Intent.ActionPick)
            {
                if (multiOptions is not null)
                    pickIntent.PutExtra(Intent.ExtraAllowMultiple, true);

                pickIntent.SetType(state.Type);

                return pickIntent;
            }

            if (!state.IsPhoto)
            {
                var isPixel = false;
                try
                {
                    var name = Settings.System.GetString(Android.App.Application.Context.ContentResolver, "device_name")!;
                    isPixel = name.Contains("pixel", StringComparison.OrdinalIgnoreCase);
                }
                catch { }

                if (request is VideoPickRequest videoRequest)
                {
                    if ((int)videoRequest.DesiredLength.TotalSeconds != 0
                        && !isPixel)
                    {
                        pickIntent.PutExtra(MediaStore.ExtraDurationLimit, (int)videoRequest.DesiredLength.TotalSeconds);
                    }

                    if (videoRequest.DesiredSize != 0)
                    {
                        pickIntent.PutExtra(MediaStore.ExtraSizeLimit, videoRequest.DesiredSize);
                    }

                    pickIntent.PutExtra(MediaStore.ExtraVideoQuality, GetVideoQuality(videoRequest.DesiredQuality));
                }
            }

            var captureRequest = request as CaptureRequest;

            if (captureRequest?.DefaultCamera is CameraDevice.Front)
            {
                pickIntent.UseFrontCamera();
            }

            var path = GetOutputMediaFile(Context, captureRequest?.DesiredDirectory, captureRequest?.DesiredName, state.IsPhoto);

            Touch(path);

            state.Path = path;

            if (path.Scheme != "file")
            {
                pickIntent.PutExtra(MediaStore.ExtraOutput, path);
                return pickIntent;
            }

            try
            {
                var photoURI = MediaFileProvider.GetUriForFile(Context, new Java.IO.File(path.Path!));

                GrantUriPermissionsForIntent(pickIntent, photoURI);
                pickIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
                pickIntent.AddFlags(ActivityFlags.GrantWriteUriPermission);
                pickIntent.PutExtra(MediaStore.ExtraOutput, photoURI);

                return pickIntent;
            }
            catch (Java.Lang.IllegalArgumentException iae)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to get file location, check and set manifest with file provider. Exception: {iae}");

                throw new ArgumentException("Unable to get file location. This most likely means that the file provider information is not set in your Android Manifest file. Please check documentation on how to set this up in your project.", iae);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to get file location, check and set manifest with file provider. Exception: {ex}");

                throw new ArgumentException("Unable to get file location. This most likely means that the file provider information is not set in your Android Manifest file. Please check documentation on how to set this up in your project.", ex);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (State is null || State.RequestId != requestCode) return;

        var path = State.Path;

        if (resultCode == Result.Canceled)
        {
            State.Tcs.SetCanceled();

            DeleteOutputFile(path);
            return;
        }

        var isPhoto = State.IsPhoto;

        try
        {
            List<MediaFile> files;

            if (data?.ClipData is not null)
            {
                var clipData = data.ClipData;
                List<MediaFile> mediaFiles = [];
                for (var i = 0; i < clipData.ItemCount; i++)
                {
                    var item = clipData.GetItemAt(i);
                    files = GetMediaFile(Context, path, requestCode, State.Action, isPhoto, item!.Uri);

                    // TODO: This can be done better.
                    mediaFiles.AddRange(files);
                }

                State.Tcs.SetResult(mediaFiles);

                return;
            }

            files = GetMediaFile(Context, path, requestCode, State.Action, isPhoto, data?.Data);

            State.Tcs.SetResult(files);

            return;
        }
        catch (Exception ex)
        {
            State.Tcs.SetException(ex);
        }
    }

    protected virtual void GrantUriPermissionsForIntent(Intent intent, Uri uri)
    {
        var resInfoList = Context.PackageManager!.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
        foreach (var resolveInfo in resInfoList)
        {
            var packageName = resolveInfo.ActivityInfo!.PackageName;
            Context.GrantUriPermission(packageName, uri, ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantReadUriPermission);
        }
    }

    protected virtual void Touch(Uri path)
    {
        if (path.Scheme != "file") return;

        var newPath = GetLocalPath(path);
        try
        {
            var stream = File.Create(newPath);
            stream.Close();
            stream.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unable to create path: " + newPath + " " + ex.Message + "This means you have illegal characters");
            throw;
        }
    }

    protected virtual List<MediaFile> GetMediaFile(
        Context context,
        Uri? uriPath,
        int requestCode,
        string action,
        bool isPhoto,
        Uri? data)
    {
        (string?, string?, bool) path;

        string? originalPath = null;

        if (action != Intent.ActionPick)
        {
            originalPath = uriPath!.Path;

            // Not all camera apps respect EXTRA_OUTPUT, some will instead
            // return a content or file uri from data.
            if (data is null || data.Path == originalPath)
            {
                path = (uriPath.Path, Path.GetFileName(uriPath.Path), false);
            }
            else
            {
                originalPath = data.ToString();
                var currentPath = uriPath.Path;
                var originalFilename = Path.GetFileName(currentPath);

                path = TryMoveFile(context, data, uriPath, isPhoto)
                    ? (currentPath, originalFilename, false)
                    : (null, null, false);
            }
        }
        else if (data is not null)
        {
            originalPath = data.ToString();
            uriPath = data;
            path = GetFileForUri(context, uriPath, isPhoto);
        }
        else path = (null, null, false);

        var resultPath = path.Item1;
        var originalFileName = path.Item2;

        if (!string.IsNullOrWhiteSpace(resultPath) && File.Exists(resultPath))
        {
            var mf = new MediaFile(resultPath, () =>
            {
                return File.OpenRead(resultPath);
            }, albumPath: originalPath, originalFilename: originalFileName);

            return [mf];
        }

        throw new MediaFileNotFoundException(originalPath);
    }

    protected virtual bool TryMoveFile(Context context, Uri url, Uri path, bool isPhoto)
    {
        var moveTo = GetLocalPath(path);

        var filePath = GetFileForUri(context, url, isPhoto);

        if (string.IsNullOrWhiteSpace(filePath.Item1)) return false;

        try
        {
            if (url.Scheme == "content") context.ContentResolver!.Delete(url, null, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unable to delete content resolver file: " + ex.Message);
        }

        try
        {
            File.Delete(moveTo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unable to delete normal file: " + ex.Message);
        }

        try
        {
            File.Move(filePath.Item1, moveTo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unable to move files: " + ex.Message);
        }

        return true;
    }

    protected virtual (string?, string?, bool) GetFileForUri(Context context, Uri uri, bool isPhoto)
    {
        if (uri.Scheme == "file")
        {
            var path = new System.Uri(uri.ToString()!).LocalPath;
            var originalFilename = System.IO.Path.GetFileName(path);
            return (path, originalFilename, false);
        }
        else if (uri.Scheme == "content")
        {
            ICursor? cursor = null;
            try
            {
                string[]? proj = null;
                if (OperatingSystem.IsAndroidVersionAtLeast(22))
                    proj = [MediaStore.IMediaColumns.Data];

                cursor = context.ContentResolver!.Query(uri, proj, null, null, null);
                if (cursor is null || !cursor.MoveToNext()) return (null, null, false);
                else
                {
                    var column = cursor.GetColumnIndex(MediaStore.IMediaColumns.Data);
                    string? contentPath = null;

                    if (column != -1) contentPath = cursor.GetString(column);

                    string? originalFilename = null;

                    // If they don't follow the "rules", try to copy the file locally
                    if (string.IsNullOrWhiteSpace(contentPath) || !contentPath.StartsWith("file", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string? fileName = null;
                        try
                        {
                            fileName = System.IO.Path.GetFileName(contentPath)!;
                            originalFilename = fileName;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Unable to get file path name, using new unique " + ex);
                        }

                        var outputPath = GetOutputMediaFile(context, "temp", fileName!, isPhoto);

                        try
                        {
                            using var input = context.ContentResolver.OpenInputStream(uri);
                            using var output = File.Create(outputPath.Path!);

                            input!.CopyTo(output);

                            contentPath = outputPath.Path;
                        }
                        catch (Java.IO.FileNotFoundException fnfEx)
                        {
                            // If there's no data associated with the uri, we don't know
                            // how to open this. contentPath will be null which will trigger
                            // MediaFileNotFoundException.
                            System.Diagnostics.Debug.WriteLine("Unable to save picked file from disk " + fnfEx);
                        }
                    }
                    else
                    {
                        originalFilename = Path.GetFileName(contentPath);
                    }

                    return (contentPath, originalFilename, false);
                }
            }
            finally
            {
                if (cursor != null)
                {
                    cursor.Close();
                    cursor.Dispose();
                }
            }
        }

        return (null, null, false);
    }

    /// <summary>
    /// Try go get output file
    /// </summary>
    /// <param name="context"></param>
    /// <param name="subdir"></param>
    /// <param name="name"></param>
    /// <param name="isPhoto"></param>
    /// <returns></returns>
    public static Uri GetOutputMediaFile(Context context, string? subdir, string? name, bool isPhoto)
    {
        subdir = string.IsNullOrWhiteSpace(subdir) ? string.Empty : subdir;

        Uri uri;

        if (string.IsNullOrWhiteSpace(name))
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            name = isPhoto ? $"IMG_{timestamp}.jpg" : $"VID_{timestamp}.mp4";
        }

        var mediaType = isPhoto ? Environment.DirectoryPictures : Environment.DirectoryMovies;
        var directory = context.GetExternalFilesDir(mediaType);

        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            using var mediaStorageDir = new Java.IO.File(directory, subdir);

            mediaStorageDir.Mkdirs();

            // Ensure this media doesn't show up in gallery apps
            using var nomedia = new Java.IO.File(mediaStorageDir, ".nomedia");
            nomedia.CreateNewFile();

            uri = Uri.FromFile(new Java.IO.File(GetUniquePath(mediaStorageDir.Path, name, isPhoto)))!;
        }
        else
        {
            using var mediaStorageDir = new Java.IO.File(directory, subdir);

            if (!mediaStorageDir.Exists())
            {
                if (!mediaStorageDir.Mkdirs())
                    throw new IOException("Couldn't create directory, have you added the WRITE_EXTERNAL_STORAGE permission?");

                // Ensure this media doesn't show up in gallery apps
                using var nomedia = new Java.IO.File(mediaStorageDir, ".nomedia");
                nomedia.CreateNewFile();
            }

            uri = Uri.FromFile(new Java.IO.File(GetUniquePath(mediaStorageDir.Path, name, isPhoto)))!;
        }

        return uri;
    }

    protected static string GetUniquePath(string folder, string name, bool isPhoto)
    {
        var ext = Path.GetExtension(name);
        if (string.IsNullOrWhiteSpace(ext)) ext = isPhoto ? ".jpg" : ".mp4";

        name = Path.GetFileNameWithoutExtension(name);

        var nname = name + ext;
        var i = 1;
        while (File.Exists(Path.Combine(folder, nname)))
            nname = name + "_" + i++ + ext;

        return Path.Combine(folder, nname);
    }

    protected virtual void DeleteOutputFile(Uri? path)
    {
        if (path?.Scheme != "file") return;

        try
        {
            var localPath = GetLocalPath(path);

            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unable to delete file: " + ex.Message);
        }
    }

    protected virtual bool IsValidExif(ExifInterface exif)
    {
        //if null, then not falid
        if (exif is null) return false;

        try
        {
            //if has thumb, but is <= 0, then not valid
            if (exif.HasThumbnail && (exif.GetThumbnail()?.Length ?? 0) <= 0)
                return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Unable to get thumbnail exif: " + ex);
            return false;
        }

        return true;
    }

    protected virtual async Task<bool> RequestCameraPermissions()
    {
        var checkCamera = HasPermissionInManifest(Manifest.Permission.Camera);

        var hasCameraPermission = PermissionStatus.Granted;
        if (checkCamera)
        {
            hasCameraPermission = await Permissions.CheckStatusAsync<Permissions.Camera>();
        }

        if (hasCameraPermission is PermissionStatus.Granted) return true;

        if (Permissions.ShouldShowRationale<Permissions.Camera>())
        {

        }

        hasCameraPermission = await Permissions.RequestAsync<Permissions.Camera>();

        if (hasCameraPermission is not PermissionStatus.Granted)
        {
            System.Diagnostics.Debug.WriteLine("Camera permission Denied.");
            return false;
        }

        return true;
    }

    protected virtual bool HasPermissionInManifest(string permission)
    {
        try
        {
            if (RequestedPermissions is not null)
                return RequestedPermissions.Any(r => r.Equals(permission, StringComparison.InvariantCultureIgnoreCase));

            if (Context is null)
            {
                System.Diagnostics.Debug.WriteLine("Unable to detect current Activity or App Context. Please ensure Xamarin.Essentials is installed and initialized.");
                return false;
            }

            var info = Context.PackageManager!.GetPackageInfo(Context.PackageName!, PackageInfoFlags.Permissions);

            if (info is null)
            {
                System.Diagnostics.Debug.WriteLine("Unable to get Package info, will not be able to determine permissions to request.");
                return false;
            }

            RequestedPermissions = info.RequestedPermissions!;

            if (RequestedPermissions is null)
            {
                System.Diagnostics.Debug.WriteLine("There are no requested permissions, please check to ensure you have marked permissions you want to request.");
                return false;
            }

            return RequestedPermissions.Any(r => r.Equals(permission, StringComparison.InvariantCultureIgnoreCase));
        }
        catch (Exception ex)
        {
            Console.Write("Unable to check manifest for permission: " + ex);
        }
        return false;
    }

    protected virtual void VerifyCaptureRequest(CaptureRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (Path.IsPathRooted(request.DesiredDirectory))
            throw new ArgumentException("options.Directory must be a relative path", nameof(request));

        if (!string.IsNullOrWhiteSpace(request.DesiredName))
            request.DesiredName = IllegalCharactersRegex().Replace(request.DesiredName, string.Empty).Replace(@"\", string.Empty);

        if (!string.IsNullOrWhiteSpace(request.DesiredDirectory))
            request.DesiredDirectory = IllegalCharactersRegex().Replace(request.DesiredDirectory, string.Empty).Replace(@"\", string.Empty);
    }

    protected virtual int GetVideoQuality(VideoQuality videoQuality)
    {
        return videoQuality switch
        {
            >= VideoQuality.Medium => 1,
            _ => 0,
        };
    }

    protected virtual string GetLocalPath(Uri uri) => new System.Uri(uri.ToString()!).LocalPath;
}