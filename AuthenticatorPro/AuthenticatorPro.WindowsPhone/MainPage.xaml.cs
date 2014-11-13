using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Credentials;

namespace AuthenticatorPro
{
    public sealed partial class MainPage : Page
    {
        public static ObservableCollection<Account> Accounts { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
                InitializeApplication();

            LoadData();
        }

        private void InitializeApplication()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("RoamAccountSecrets"))
            {
                var value = ApplicationData.Current.RoamingSettings.Values["RoamAccountSecrets"] as bool?;

                if (value.HasValue)
                    App.RoamAccountSecrets = value.Value;
                else
                    App.RoamAccountSecrets = true;
            }

            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("AutomaticTimeCorrection"))
            {
                var value = ApplicationData.Current.RoamingSettings.Values["AutomaticTimeCorrection"] as bool?;

                if (value.HasValue)
                    App.AutomaticTimeCorrection = value.Value;
                else
                    App.AutomaticTimeCorrection = true;
            }

            App.Accounts = new List<Account>();

            App.NtpTimeOffset = new TimeSpan(0, 0, 0, 0, 0);

            if (App.AutomaticTimeCorrection)
                NtpClient.SynchronizeDeviceTime();

            AccountManager.DeserializeAccounts();
        }

        private void LoadData()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 0, 0, 5);
            dt.Tick += DispatchTimer_Tick;

            dt.Start();

            Accounts = new ObservableCollection<Account>();

            foreach (Account a in App.Accounts)
            {
                Accounts.Add(a);
            }

            if (Accounts.Count > 0)
            {
                this.prgStatusBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.txtEmpty.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                this.prgStatusBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.txtEmpty.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void DispatchTimer_Tick(object sender, object e)
        {
            this.prgStatusBar.Value = CodeGenerator.TimeElapsed() * 100.0;

            foreach (Account a in Accounts)
            {
                a.RefreshCode();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AccountPage));
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
