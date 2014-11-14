using System;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;

namespace AuthenticatorPro
{
    public sealed partial class ScanPage : Page
    {
        DispatcherTimer dispatcherTimer;

        MediaCapture captureManager;
        BarcodeReader reader;
        Result scanResult;

        public ScanPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            captureManager = new MediaCapture();
            reader = new BarcodeReader();

            PreviewCameraFeed();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            dispatcherTimer.Stop();

            await captureManager.StopPreviewAsync();
        }

        private async void PreviewCameraFeed()
        {
            try
            {
                var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                if (cameras.Count < 1)
                {
                    MessageDialog dialog = new MessageDialog("You don't have any cameras attached to this device to use for scanning.", "Camera Not Found");
                    await dialog.ShowAsync();

                    if (Frame.CanGoBack)
                        Frame.GoBack();
                }

                MediaCaptureInitializationSettings settings = null;
                DeviceInformation camera = null;

                foreach (var item in cameras)
                {
                    if (item.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back)
                    {
                        camera = item;
                        break;
                    }
                }

                if (camera == null)
                    camera = cameras[0];

                settings = new MediaCaptureInitializationSettings { VideoDeviceId = camera.Id };

                await captureManager.InitializeAsync(settings);

                this.cptCameraFeed.Source = captureManager;

                captureManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);

                await captureManager.StartPreviewAsync();

                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 3, 0);
                dispatcherTimer.Tick += DispatcherTimer_Tick;

                dispatcherTimer.Start();
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new MessageDialog("Something went wrong while decoding the QR code. Please try scanning again.", "Scanning Failed");
                dialog.ShowAsync();
            }
        }

        private async void DispatcherTimer_Tick(object sender, object e)
        {
            try
            {
                dispatcherTimer.Stop();

                await captureManager.VideoDeviceController.FocusControl.FocusAsync();

                var photoStorageFile = await KnownFolders.PicturesLibrary.CreateFileAsync("scan.jpg", CreationCollisionOption.GenerateUniqueName);
                await captureManager.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), photoStorageFile);

                var stream = await photoStorageFile.OpenReadAsync();

                var writeableBmp = new WriteableBitmap(1, 1);
                writeableBmp.SetSource(stream);

                writeableBmp = new WriteableBitmap(writeableBmp.PixelWidth, writeableBmp.PixelHeight);

                stream.Seek(0);
                writeableBmp.SetSource(stream);

                scanResult = ScanBitmap(writeableBmp);

                await photoStorageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                if (scanResult != null)
                {
                    App.BarcodeScannerResult = scanResult.Text;

                    if (Frame.CanGoBack)
                        Frame.GoBack();
                }

                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                dispatcherTimer.Start();
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new MessageDialog("Something went wrong while decoding the QR code. Please try scanning again.", "Scanning Failed");
                dialog.ShowAsync();
            }
        }

        private Result ScanBitmap(WriteableBitmap writeableBmp)
        {
            var barcodeReader = new BarcodeReader
            {
                TryHarder = true,
                AutoRotate = true
            };

            var result = barcodeReader.Decode(writeableBmp);

            return result;
        }
    }
}
