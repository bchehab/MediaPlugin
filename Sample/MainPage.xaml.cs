using MPowerKit.MediaPlugin;

namespace Sample;

public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
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