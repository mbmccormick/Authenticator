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
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        private App a;
        public MainPage()
        {
            InitializeComponent();
            a = (App)Application.Current;
            GPinGenerator.intervalLength = 30;
            GPinGenerator.pinCodeLength = 6;
            pbTimeLeft.Minimum = 0;
            pbTimeLeft.Maximum = GPinGenerator.intervalLength;
        }

        private void btnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddAccount.xaml", UriKind.Relative));
        }

        private void UpdateStackPanel()
        {
            //clear current data in stack panel
            SPAccounts.Children.Clear();
            //for every account
            int i = 0;
            foreach (Account acct in a.WorkingDB.Accounts)
            {
                //retrieve account information
                string tempName = acct.AccountName;
                //generate new stack panel
                StackPanel tempSP = new StackPanel();
                StackPanel tempSP2 = new StackPanel();
                tempSP.Orientation = System.Windows.Controls.Orientation.Vertical;
                tempSP.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                tempSP.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                tempSP.Margin = new Thickness(0, 0, 0, 20);
                TextBlock tempTBName = new TextBlock();
                tempTBName.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                tempTBName.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                tempTBName.FontSize = 24;
                tempTBName.Text = tempName;
                TextBlock tempTBPin = new TextBlock();
                tempTBPin.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                tempTBPin.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                tempTBPin.FontSize = 32;
                tempTBPin.Text = acct.CalculatePin();

                if (GPinGenerator.numberSecondsLeft() <= 5)
                {
                    tempTBPin.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    tempTBPin.Foreground = new SolidColorBrush(Color.FromArgb(255, 55, 145, 200));
                }
                tempSP.Children.Add(tempTBName);
                tempSP.Children.Add(tempTBPin);
                //put new stackpanel in master stackpanel
                SPAccounts.Children.Add(tempSP);
                i++;

            }
        }

        public void StartTimer()
        {
            System.Windows.Threading.DispatcherTimer myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100); // 100 Milliseconds
            myDispatcherTimer.Tick += new EventHandler(Each_Tick);
            myDispatcherTimer.Start();
        }

        // A variable to count with.
        // Fires every 100 miliseconds while the DispatcherTimer is active.
        public void Each_Tick(object o, EventArgs sender)
        {
            UpdateStackPanel();
            UpdateProgressBar();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            StartTimer();
        }

        private void btnRemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/RemoveAccount.xaml", UriKind.Relative));
        }

        private void UpdateProgressBar()
        {
            if (a.WorkingDB.Accounts.Count > 0)
            {
                pbTimeLeft.Visibility = System.Windows.Visibility.Visible;
            }

            else
            {
                pbTimeLeft.Visibility = System.Windows.Visibility.Collapsed;
            }
            pbTimeLeft.Value = GPinGenerator.numberSecondsLeft();
            if (GPinGenerator.numberSecondsLeft() <= 5)
            {
                pbTimeLeft.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                pbTimeLeft.Foreground = new SolidColorBrush(Color.FromArgb(255, 55, 145, 200));
            }
        }
    }
}