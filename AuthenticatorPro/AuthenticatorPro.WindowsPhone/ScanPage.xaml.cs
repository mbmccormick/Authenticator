using System;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
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

        private async void PreviewCameraFeed()
        {
            try
            {
                var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                if (cameras.Count < 1)
                {
                    // TODO: handle this
                }

                MediaCaptureInitializationSettings settings;

                if (cameras.Count == 1)
                {
                    settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameras[0].Id }; // 0 => front, 1 => back
                }
                else
                {
                    settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameras[1].Id }; // 0 => front, 1 => back
                }

                await captureManager.InitializeAsync(settings);

                this.cptCameraFeed.Source = captureManager;

                await captureManager.StartPreviewAsync();

                while (scanResult == null)
                {
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
                }

                await captureManager.StopPreviewAsync();

                App.BarcodeScannerResult = scanResult.Text;

                if (Frame.CanGoBack)
                    Frame.GoBack();
            }
            catch (Exception ex)
            {
                // TODO: handle this
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

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            await captureManager.StopPreviewAsync();
        }
    }
}
