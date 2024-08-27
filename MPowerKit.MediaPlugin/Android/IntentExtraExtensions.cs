using Android.Content;

namespace MPowerKit.MediaPlugin;

public static class IntentExtraExtensions
{
    public static void UseFrontCamera(this Intent intent)
    {
        intent.PutExtra("com.google.assistant.extra.USE_FRONT_CAMERA", true);
        intent.PutExtra("android.intent.extra.USE_FRONT_CAMERA", true);
        intent.PutExtra("android.intent.extras.LENS_FACING_FRONT", 1);
        intent.PutExtra("android.intent.extras.CAMERA_FACING", 1);

        // Extras for displaying the front camera on Samsung
        intent.PutExtra("camerafacing", "front");
        intent.PutExtra("previous_mode", "Selfie");

        if (Android.App.Application.Context?.PackageName?.Contains("honor", StringComparison.OrdinalIgnoreCase) is true)
        {
            // Extras for displaying the front camera on Honor
            intent.PutExtra("default_camera", "1");
            intent.PutExtra("default_mode", "com.hihonor.camera2.mode.photo.PhotoMode");
        }
        else
        {
            // Extras for displaying the front camera on Huawei
            intent.PutExtra("default_camera", "1");
            intent.PutExtra("default_mode", "com.huawei.camera2.mode.photo.PhotoMode");
        }
    }

    public static void UseBackCamera(this Intent intent)
    {

    }
}