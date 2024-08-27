//using UIKit;

//namespace MPowerKit.MediaPlugin;

//public class MediaPickerPopoverDelegate : UIPopoverControllerDelegate
//{
//    protected readonly MediaPickerDelegate PickerDelegate;
//    protected readonly UINavigationController Picker;

//    public MediaPickerPopoverDelegate(MediaPickerDelegate pickerDelegate, UINavigationController picker)
//    {
//        PickerDelegate = pickerDelegate;
//        Picker = picker;
//    }

//    public override bool ShouldDismiss(UIPopoverController popoverController) => true;

//    public override void DidDismiss(UIPopoverController popoverController) => PickerDelegate.Canceled(Picker);
//}