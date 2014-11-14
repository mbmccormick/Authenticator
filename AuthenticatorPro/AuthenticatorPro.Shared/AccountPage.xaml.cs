using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AuthenticatorPro
{
    public sealed partial class AccountPage : Page
    {
        public AccountPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.BarcodeScannerResult != null)
            {
                string encoding = App.BarcodeScannerResult;

                try
                {
                    encoding = encoding.Replace("otpauth://totp/", "");
                    string[] splitString = encoding.Split(Convert.ToChar("?"));
                    splitString[1] = splitString[1].Replace("secret=", "");

                    this.txtName.Text = splitString[0];
                    this.txtSecretKey.Text = splitString[1];
                }
                catch (Exception ex)
                {
                    MessageDialog dialog = new MessageDialog("Something went wrong while decoding the QR code. Please try scanning again.", "Scanning Failed");
                    dialog.ShowAsync();
                }

                App.BarcodeScannerResult = null;
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Account item = new Account();
            item.Name = this.txtName.Text;
            item.SecretKey = this.txtSecretKey.Text;

            if (item.Name.Length == 0)
            {
                MessageDialog dialog = new MessageDialog("You need to provide a name for this account. Please try again.", "Validation Error");
                await dialog.ShowAsync();

                return;
            }

            item.RefreshCode();

            if (item.SecretKey.Length == 0 ||
                item.Code == null ||
                item.Code.Length != 6)
            {
                MessageDialog dialog = new MessageDialog("The secret key you provided could not be validated. Please try again.", "Validation Error");
                await dialog.ShowAsync();

                return;
            }

            App.Accounts.Add(item);

            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ScanPage));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
