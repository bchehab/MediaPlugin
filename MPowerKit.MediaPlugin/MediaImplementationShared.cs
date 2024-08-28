namespace MPowerKit.MediaPlugin;

public class MediaImplementationShared : IMedia
{
    public bool IsCameraAvailable { get; } = false;
    public bool IsTakePhotoSupported { get; } = false;
    public bool IsPickPhotoSupported { get; } = false;
    public bool IsTakeVideoSupported { get; } = false;
    public bool IsPickVideoSupported { get; } = false;

    public Task<bool> Initialize()
    {
        throw new NotImplementedException();
    }

    public Task<MediaFile?> PickPhotoAsync(PickRequest? request = null, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<MediaFile>?> PickPhotosAsync(PickRequest? request = null, MultiPickerOptions? multiOptions = null, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<MediaFile?> PickVideoAsync(VideoPickRequest? request = null, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<MediaFile?> TakePhotoAsync(CaptureRequest? request = null, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<MediaFile?> TakeVideoAsync(CaptureRequest? request = null, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}