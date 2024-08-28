using MPowerKit.MediaPlugin;

namespace Sample;

public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();

        //img.Source = "/Users/mac/Library/Developer/CoreSimulator/Devices/E0439A11-AB9A-4D30-A0BF-2008CCAABF18/data/Containers/Shared/AppGroup/8C2E15C4-28A8-4D9F-A19B-B1812D237B90/File Provider Storage/photospicker/uuid=CC95F08C-88C3-4012-9D6D-64A413D254B3&library=1&type=1&mode=2&loc=true&cap=true.jpeg";
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            var file = await Media.Current.TakePhotoAsync(new CaptureRequest()
            {
                DefaultCamera = CameraDevice.Front,
            });

            BindableLayout.SetItemsSource(stack, new List<MediaFile>() { file });
        }
        catch (OperationCanceledException oce)
        {

        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            var file = await Media.Current.TakeVideoAsync(new CaptureRequest()
            {
                DefaultCamera = CameraDevice.Front,
            });
        }
        catch (OperationCanceledException oce)
        {

        }
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        try
        {
            var file = await Media.Current.PickPhotoAsync();

            BindableLayout.SetItemsSource(stack, new List<MediaFile>() { file });
        }
        catch (OperationCanceledException oce)
        {

        }
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        try
        {
            var files = await Media.Current.PickPhotosAsync();

            BindableLayout.SetItemsSource(stack, files);
        }
        catch (OperationCanceledException oce)
        {

        }
    }

    private async void Button_Clicked_4(object sender, EventArgs e)
    {
        try
        {
            var files = await Media.Current.PickVideoAsync();
        }
        catch (OperationCanceledException oce)
        {

        }
    }
}