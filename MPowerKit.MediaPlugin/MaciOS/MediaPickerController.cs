//using Foundation;

//using UIKit;

//namespace MPowerKit.MediaPlugin;

///// <summary>
///// Media Picker Controller
///// </summary>
//public sealed class MediaPickerController : UIImagePickerController
//{
//    private bool _disposed;

//    internal MediaPickerController(MediaPickerDelegate mpDelegate) =>
//        base.Delegate = mpDelegate;

//    /// <summary>
//    /// Deleage
//    /// </summary>
//    public override NSObject? Delegate
//    {
//        get => base.Delegate;
//        set
//        {
//            if (value is null)
//                base.Delegate = value;
//            else throw new NotSupportedException();
//        }
//    }

//    /// <summary>
//    /// Gets result of picker
//    /// </summary>
//    /// <returns></returns>

//    public Task<List<MediaFile>> GetResultAsync() =>
//        ((MediaPickerDelegate)Delegate).Task;

//    protected override void Dispose(bool disposing)
//    {
//        base.Dispose(disposing);
//        if (disposing && !_disposed)
//        {
//            _disposed = true;
//            InvokeOnMainThread(() =>
//            {
//                try
//                {
//                    Delegate?.Dispose();
//                    Delegate = null;
//                }
//                catch
//                {

//                }
//            });
//        }
//    }
//}