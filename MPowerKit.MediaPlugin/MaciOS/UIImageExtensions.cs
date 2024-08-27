using CoreGraphics;

using CoreImage;

using UIKit;

namespace MPowerKit.MediaPlugin;

/// <summary>
/// Static mathods for UIImage
/// </summary>
public static class UIImageExtensions
{
    /// <summary>
    /// Resize image maintain aspect ratio
    /// </summary>
    /// <param name="imageSource"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static UIImage ResizeImageWithAspectRatio(this UIImage imageSource, float scale)
    {
        if (scale > 1.0f) return imageSource;

        using var c = CIContext.Create();
        var sourceImage = CIImage.FromCGImage(imageSource.CGImage!);
        var orientation = imageSource.Orientation;
        imageSource?.Dispose();

        CILanczosScaleTransform transform = new()
        {
            Scale = scale,
            InputImage = sourceImage,
            AspectRatio = 1.0f
        };

        var output = transform.OutputImage;
        using var cgi = c.CreateCGImage(output!, output!.Extent);
        transform?.Dispose();
        output?.Dispose();
        sourceImage?.Dispose();

        return UIImage.FromImage(cgi!, 1.0f, orientation);
    }

    /// <summary>
    /// Resize image to maximum size
    /// keeping the aspect ratio
    /// </summary>
    public static UIImage ResizeImageWithAspectRatio(this UIImage sourceImage, float maxWidth, float maxHeight)
    {
        var sourceSize = sourceImage.Size;
        var maxResizeFactor = Math.Max(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
        if (maxResizeFactor > 1) return sourceImage;

        var width = maxResizeFactor * sourceSize.Width;
        var height = maxResizeFactor * sourceSize.Height;

        return sourceImage.ResizeImage(width, height);
    }

    /// <summary>
    /// Resize image, but ignore the aspect ratio
    /// </summary>
    /// <param name="sourceImage"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static UIImage ResizeImage(this UIImage sourceImage, double width, double height)
    {
        CGSize size = new(width, height);

        return new UIGraphicsImageRenderer(size).CreateImage((ctx) => sourceImage.Draw(new CGRect(CGPoint.Empty, size)));
    }

    /// <summary>
    /// Crop image to specitic size and at specific coordinates
    /// </summary>
    /// <param name="sourceImage"></param>
    /// <param name="crop_x"></param>
    /// <param name="crop_y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static UIImage CropImage(this UIImage sourceImage, int crop_x, int crop_y, int width, int height)
    {
        var imgSize = sourceImage.Size;
        CGSize size = new(width, height);

        return new UIGraphicsImageRenderer(size).CreateImage((ctx) =>
        {
            ctx.ClipToRect(new(CGPoint.Empty, size));

            sourceImage.Draw(new CGRect(-crop_x, -crop_y, imgSize.Width, imgSize.Height));
        });
    }
}