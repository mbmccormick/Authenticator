using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using WP7_Barcode_Library;

namespace Authenticator
{
    public partial class AddPage : PhoneApplicationPage
    {
        private Authenticator.App _application = null;
        bool newPageInstance = false;

        public AddPage()
        {
            InitializeComponent();

            _application = (App)Application.Current;
            newPageInstance = true;
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            string tempName = txtAccountName.Text;
            string tempKey = txtSecretKey.Text;
            if (tempName != "" && tempKey != "")
            {
                AddToAccountDB(tempName, tempKey);
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void btnScanBarcode_Click(object sender, RoutedEventArgs e)
        {
            WP7BarcodeManager.ScanBarcode(ScanBarcode_Completed);
        }

        public void ScanBarcode_Completed(BarcodeCaptureResult e)
        {
            // otpauth://totp/sample@gmail.com?secret=samplesample
            if (e.State == WP7_Barcode_Library.CaptureState.Success)
            {
                string str = e.BarcodeText;
                str = str.Replace("otpauth://totp/", "");
                string[] splitString = str.Split(Convert.ToChar("?"));
                splitString[1] = splitString[1].Replace("secret=", "");

                AddToAccountDB(splitString[0], splitString[1]);
            }
            else
            {
                MessageBox.Show("The barcode for your account could not be read. Please try again.", "Error", MessageBoxButton.OK);
            }
        }

        private void AddToAccountDB(string Name, string Key)
        {
            Account a = new Account();
            a.AccountName = Name;
            a.SecretKey = Key;

            CodeGenerator cg = new CodeGenerator(6, 30);
            string code = cg.computePin(a.SecretKey);

            if (code == null || code.Length != 6)
            {
                MessageBox.Show("The Secret Key you provided could not be validated. Please check your input and try again.", "Error", MessageBoxButton.OK);
                return;
            }
            else
            {
                _application.Database.Add(a);
            }

            NavigationService.GoBack();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (State.ContainsKey("txtAccountName") && newPageInstance == true)
            {
                txtAccountName.Text = (string)State["txtAccountName"];
                txtSecretKey.Text = (string)State["txtSecretKey"];
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            newPageInstance = false;
            State["txtAccountName"] = txtAccountName.Text;
            State["txtSecretKey"] = txtSecretKey.Text;

            base.OnNavigatedFrom(e);
        }
    }
}