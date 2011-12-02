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

namespace GAuthenticator
{
    public partial class AddAccount : PhoneApplicationPage
    {
        bool newPageInstance = false;
        private App a;
        public AddAccount()
        {
            InitializeComponent();
            a = (App)Application.Current;
            newPageInstance = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void btnScanBarcode_Click(object sender, RoutedEventArgs e)
        {
            WP7_Barcode_Library.WP7BarcodeManager.ScanBarcode(BarcodeScanned);
        }

        private void BarcodeScanned(WP7_Barcode_Library.BarcodeCaptureResult e)
        {
            //otpauth://totp/sample@gmail.com?secret=samplesample
            if (e.State == WP7_Barcode_Library.CaptureState.Success)
            {
                string str = e.BarcodeText;
                str = str.Replace("otpauth://totp/", "");
                string[] splitString = str.Split(Convert.ToChar("?"));
                splitString[1] = splitString[1].Replace("secret=", "");
                AddToAccountDB(splitString[0], splitString[1]);
                NavigationService.GoBack();
            }
        }

        private void AddToAccountDB(string Name, string Key)
        {
            Account newAccount = new Account();
            newAccount.AccountName = Name;
            newAccount.SecretKey = Key;
            a.WorkingDB.Accounts.Add(newAccount);
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            string tempName = txtAccountName.Text;
            string tempKey = txtSecretKey.Text;
            if (tempName != "" && tempKey != "")
            {
                AddToAccountDB(tempName, tempKey);
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