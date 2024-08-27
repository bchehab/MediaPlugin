namespace MPowerKit.MediaPlugin;

/// <summary>
/// Interface for Media
/// </summary>
public interface IMedia
{
    /// <summary>
    /// Gets if a camera is available on the device
    /// </summary>
    bool IsCameraAvailable { get; }

    /// <summary>
    /// Gets if ability to take photos supported on the device
    /// </summary>
    bool IsTakePhotoSupported { get; }

    /// <summary>
    /// Gets if the ability to pick photo is supported on the device
    /// </summary>
    bool IsPickPhotoSupported { get; }

    /// <summary>
    /// Gets if ability to take video is supported on the device
    /// </summary>
    bool IsTakeVideoSupported { get; }

    /// <summary>
    /// Gets if the ability to pick a video is supported on the device
    /// </summary>
    bool IsPickVideoSupported { get; }

    /// <summary>
    /// Picks a photo from the default gallery
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns>Media file or null if canceled</returns>
    Task<MediaFile?> PickPhotoAsync(PickRequest? request = null, CancellationToken token = default);

    /// <summary>
    /// Picks a photo from the default gallery
    /// </summary>
    /// <returns>Media file or null if canceled</returns>
    Task<List<MediaFile>?> PickPhotosAsync(PickRequest? request = null, MultiPickerOptions? multiOptions = null, CancellationToken token = default);

    /// <summary>
    /// Take a photo async with specified options
    /// </summary>
    /// <param name="request">Camera Photo Request</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Media file of photo or null if canceled</returns>
    Task<MediaFile?> TakePhotoAsync(CaptureRequest? request = null, CancellationToken token = default);

    /// <summary>
    /// Picks a video from the default gallery
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns>Media file of video or null if canceled</returns>
    Task<MediaFile?> PickVideoAsync(VideoPickRequest? request = null, CancellationToken token = default);

    /// <summary>
    /// Take a video with specified options
    /// </summary>
    /// <param name="request">Camera Video Request</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Media file of new video or null if canceled</returns>
    Task<MediaFile?> TakeVideoAsync(CaptureRequest? request = null, CancellationToken token = default);

#if ANDROID
    void OnActivityResult(int requestCode, Android.App.Result resultCode, Android.Content.Intent? data);
#endif
}