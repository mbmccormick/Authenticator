using System;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;

namespace AuthenticatorPro
{
    public sealed partial class ScanPage : Page
    {
        MediaCapture captureManager;
        BarcodeReader reader;

        public ScanPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            reader = new BarcodeReader();

            PreviewCameraFeed();
        }

        private async void PreviewCameraFeed()
        {
            MediaCapture captureManager = new MediaCapture();
            await captureManager.InitializeAsync();

            captureManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
            captureManager.FocusChanged += captureManager_FocusChanged;

            cptCameraFeed.Source = captureManager;

            await captureManager.StartPreviewAsync();

            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 0, 3, 0);
            dt.Tick += DispatchTimer_Tick;

            dt.Start();
        }

        private async void DispatchTimer_Tick(object sender, object e)
        {
            try
            {
                await captureManager.VideoDeviceController.FocusControl.FocusAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private async void captureManager_FocusChanged(MediaCapture sender, MediaCaptureFocusChangedEventArgs args)
        {
            if (args.FocusState == Windows.Media.Devices.MediaCaptureFocusState.Focused)
            {
                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
                InMemoryRandomAccessStream imageStream = new InMemoryRandomAccessStream();

                await sender.CapturePhotoToStreamAsync(imageProperties, imageStream);

                await imageStream.FlushAsync();

                WriteableBitmap image = new WriteableBitmap(300, 400);

                imageStream.Seek(0);
                image.SetSource(imageStream);

                var result = reader.Decode(image);

                if (result != null)
                {
                    App.BarcodeScannerResult = result.Text;

                    if (Frame.CanGoBack)
                        Frame.GoBack();
                }
            }
        }
    }
}
