# This is the .NET 9 version of the Media Plugin by MPowerKit/MediaPlugin: https://github.com/MPowerKit/MediaPlugin

# MPowerKit.MediaPlugin

Simple .NET MAUI cross platform plugin to take photos and video or pick them from a gallery.

Ported from [James Montemagno](https://github.com/jamesmontemagno)'s [MediaPlugin](https://github.com/jamesmontemagno/MediaPlugin) to the latest .NET, cut off resize functionality.

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/alexdobrynin)

### Setup
* Available on NuGet: [![NuGet](https://img.shields.io/nuget/v/MPowerKit.MediaPlugin.svg?label=NuGet)](https://www.nuget.org/packages/MPowerKit.MediaPlugin/)
* Please see the additional setup for each platforms permissions.

**Platform Support**

|Platform|Min Version|
|-|:-:|
|.NET|8.0|
|iOS|14.2|
|MacCatalyst|14.2|
|Android|24|
|Windows|17763|

### API Usage

Call `Media.Current` from any project or PCL to gain access to APIs.

Before taking photos or videos you should check to see if a camera exists and if photos and videos are supported on the device. There are five properties that you can check:

```csharp

/// <summary>
/// Only for Windows. Initialize all camera components, must be called before checking properties below
/// </summary>
/// <returns>If success</returns>
Task<bool> Initialize();

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
```

### Photos
```csharp
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
```

### Videos
```csharp
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
```

## Set up

To be able to use this library you need to initialize in in you `MauiProgram.cs` file as next:

```csharp

builder.
    .UseMauiApp<App>()
    .UseMpowerKitMediaPlugin(registerInterface: false); // This will register the IMedia interface for you if set to true

```

### Android 

By default, the library adds `android.hardware.camera` and `android.hardware.camera.autofocus` to your apps manifest as optional features. It is your responsbility to check whether your device supports the hardware before using it. If instead you'd like [Google Play to filter out devices](http://developer.android.com/guide/topics/manifest/uses-feature-element.html#permissions-features) without the required hardware, add the following to your AssemblyInfo.cs file in your Android project:

```
[assembly: UsesFeature("android.hardware.camera", Required = true)]
[assembly: UsesFeature("android.hardware.camera.autofocus", Required = true)]
```

Other additional setup is not required.

### iOS / MacCatalyst

Your app is required to have next keys in your Info.plist

```xml
<!--Used for taking video / photo-->
<key>NSCameraUsageDescription</key>
<string>This app needs access to the camera to take photos.</string>

<!--Used for taking video-->
<key>NSMicrophoneUsageDescription</key>
<string>This app needs access to microphone.</string>

<!--Used for picking video / photo from the gallery-->
<key>NSPhotoLibraryUsageDescription</key>
<string>This app needs access to photos.</string>

<key>NSPhotoLibraryAddUsageDescription</key>
<string>This app needs access to the photo gallery.</string>
```

If you want the dialogs to be translated you must support the specific languages in your app. Read the [iOS Localization Guide](https://developer.xamarin.com/guides/ios/advanced_topics/localization_and_internationalization/)

### Windows

Add this to your Package.appxmanifest if you want to take photo / video:

```xml
<Capabilities>
    <DeviceCapability Name="webcam"/>

    <!--Used for taking video-->
    <DeviceCapability Name="microphone"/>
</Capabilities>
```

### Permissions
By default, the Media Plugin will attempt to request multiple permissions, but each platform handles this a bit differently, such as iOS which will only pop up permissions once.

## Usage
Via a Xamarin.Forms project with a Button and Image to take a photo:

```csharp
takePhoto.Clicked += async (sender, args) =>
{
    // only for Windwos
    await Media.Current.Initialize();
    
    if (!Media.Current.IsCameraAvailable || !Media.Current.IsTakePhotoSupported)
    {
        DisplayAlert("No Camera", ":( No camera available.", "OK");
        return;
    }

    try
    {
        var file = await Media.Current.TakePhotoAsync(new CaptureRequest
        {
            DesiredDirectory = "Sample",
            DesiredName = "test.jpg"
        });

        image.Source = file.Path; 
    }
    catch (OperationCancelledException oce)
    { 
        // user cancelled
    }
};
```

To see more examples open up the Sample rpoject form this repo.

## Photo & Video Settings

### Directories and File Names

These settings are available only for taking photos and videos over `CaptureRequest` object. These properties are optional. Any illegal characters will be removed and if the name of the file is a duplicate then a number will be appended to the end. The default implementation is to specify a unique time code to each value. 

### Default Camera 
By default when you take a photo or video the default system camera will be selected. Simply set the `DefaultCamera` on `CaptureRequest`. This option does not guarantee that the actual camera will be selected because each platform is different. It seems to work extremely well on iOS, but not so much on Android. Your mileage may vary.

```csharp
var file = await Media.Current.TakePhotoAsync(new CaptureRequest
{
    DefaultCamera = CameraDevice.Front
});
```