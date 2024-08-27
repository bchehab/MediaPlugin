using Android.App;

[assembly: UsesPermission("android.permission.CAMERA")]

[assembly: UsesFeature("android.hardware.camera", Required = false)]
[assembly: UsesFeature("android.hardware.camera.autofocus", Required = false)]