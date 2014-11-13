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

namespace AuthenticatorPro
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.togRoamAccountSecrets.IsOn = App.RoamAccountSecrets;
            this.togAutomaticTimeCorrection.IsOn = App.AutomaticTimeCorrection;

            this.txtDrift.Text = "Your device is currently behind by " + App.NtpTimeOffset.ToString(@"hh\:mm\:ss") + " (hh:mm:ss).";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.togRoamAccountSecrets != null)
                App.RoamAccountSecrets = this.togRoamAccountSecrets.IsOn;

            if (this.togAutomaticTimeCorrection != null)
                App.AutomaticTimeCorrection = this.togAutomaticTimeCorrection.IsOn;

            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
