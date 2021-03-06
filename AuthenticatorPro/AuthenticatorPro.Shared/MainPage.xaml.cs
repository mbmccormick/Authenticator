﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace AuthenticatorPro
{
    public sealed partial class MainPage : Page
    {
        public static ObservableCollection<Account> Accounts { get; set; }

        DispatcherTimer dispatcherTimer;

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
            else
            {
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
            else
            {
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
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
                dispatcherTimer.Tick += DispatcherTimer_Tick;

                dispatcherTimer.Start();
            }

            if (Accounts == null)
                Accounts = new ObservableCollection<Account>();
            else
                Accounts.Clear();

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

        private void DispatcherTimer_Tick(object sender, object e)
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

        private void btnFeedback_Click(object sender, RoutedEventArgs e)
        {
            FeedbackHelper.Feedback();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            // TODO: handle this
        }

        private void StackPanel_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }

        private void btnCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Account item = ((FrameworkElement)sender).DataContext as Account;

            // TODO: implement clipboard functionality
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Account item = ((FrameworkElement)sender).DataContext as Account;

            MessageDialog dialog = new MessageDialog("Are you sure you want to delete your " + item.Name + " account?", "Delete Account");
            dialog.Commands.Add(new UICommand("yes"));
            dialog.Commands.Add(new UICommand("no"));

            var result = await dialog.ShowAsync();

            if (result.Label == "yes")
            {
                App.Accounts.Remove(item);

                LoadData();
            }
        }
    }
}
