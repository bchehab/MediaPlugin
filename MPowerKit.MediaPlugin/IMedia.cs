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

    Task<bool> Initialize();

    /// <summary>
    /// Picks a photo from the default gallery
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <param name="request">Pick Photos Request</param>
    /// <returns>Media file</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled by user.</exception>
    Task<MediaFile?> PickPhotoAsync(PickRequest? request = null, CancellationToken token = default);

    /// <summary>
    /// Picks multiple photos from the default gallery
    /// </summary>
    /// <param name="request">Pick Photos Request</param>
    /// <param name="multiOptions">Options for picking multiple photos</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Media files</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled by user.</exception>
    Task<List<MediaFile>?> PickPhotosAsync(PickRequest? request = null, MultiPickerOptions? multiOptions = null, CancellationToken token = default);

    /// <summary>
    /// Takes a photo
    /// </summary>
    /// <param name="request">Camera Photo Request</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Media file of photo</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled by user.</exception>
    Task<MediaFile?> TakePhotoAsync(CaptureRequest? request = null, CancellationToken token = default);

    /// <summary>
    /// Picks a video from the default gallery
    /// </summary>
    /// <param name="request">Pick Video Request</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Media file of video or null if canceled</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled by user.</exception>
    Task<MediaFile?> PickVideoAsync(VideoPickRequest? request = null, CancellationToken token = default);

    /// <summary>
    /// Takes a video
    /// </summary>
    /// <param name="request">Camera Video Request</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Media file of new video</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled by user.</exception>
    Task<MediaFile?> TakeVideoAsync(CaptureRequest? request = null, CancellationToken token = default);
}