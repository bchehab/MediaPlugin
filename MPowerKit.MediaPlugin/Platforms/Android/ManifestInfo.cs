using Android.App;
using Android.Content;

using Uri = Android.Net.Uri;

[assembly: UsesPermission("android.permission.CAMERA")]
[assembly: UsesPermission("android.permission.WRITE_EXTERNAL_STORAGE")]
[assembly: UsesPermission("android.permission.READ_EXTERNAL_STORAGE")]

[assembly: UsesFeature("android.hardware.camera", Required = false)]
[assembly: UsesFeature("android.hardware.camera.autofocus", Required = false)]

namespace MPowerKit.MediaPlugin;

[ContentProvider(["${applicationId}" + Authority], Name = "MPowerKit.MediaPlugin.MediaFileProvider", Exported = false, GrantUriPermissions = true)]
[MetaData("android.support.FILE_PROVIDER_PATHS", Resource = "@xml/file_provider_paths")]
public class MediaFileProvider : FileProvider
{
    internal const string Authority = ".fileprovider";

    public static Uri GetUriForFile(Context context, Java.IO.File file)
        => GetUriForFile(context, context.PackageName! + Authority, file)!;
}