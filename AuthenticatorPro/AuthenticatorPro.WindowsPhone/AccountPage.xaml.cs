using AuthenticatorPro.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Security.Credentials;
using Windows.UI.Popups;

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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Account item = new Account();
            item.Name = this.txtName.Text;
            item.SecretKey = this.txtSecretKey.Text;

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
