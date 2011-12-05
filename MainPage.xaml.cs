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

using Microsoft.Phone.Shell;

namespace Authenticator
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Authenticator.App _application = null;
        private ProgressIndicator _progressIndicator = null;
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            _application = (Authenticator.App)Application.Current;

            this.lstAccounts.ItemsSource = _application.Database.AccountList;

            CodeGenerator.intervalLength = 30;
            CodeGenerator.pinCodeLength = 6;
        }

        private void btnAddAccount_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AccountAddPage.xaml", UriKind.Relative));
        }

        public void StartTimer()
        {
            System.Windows.Threading.DispatcherTimer myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();

            myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            myDispatcherTimer.Tick += new EventHandler(Each_Tick);
            myDispatcherTimer.Start();
        }

        public void Each_Tick(object o, EventArgs sender)
        {
            foreach (Account a in _application.Database.AccountList)
            {
                CodeGenerator cg = new CodeGenerator(6, 30);
                string code = cg.computePin(a.SecretKey);

                a.Code = code;
            }

            if (_application.Database.AccountList.Count > 0)
            {
                _progressIndicator.IsVisible = true;
            }
            else
            {
                _progressIndicator.IsVisible = false;
            }

            _progressIndicator.IsIndeterminate = false;
            _progressIndicator.Value = CodeGenerator.numberSecondsLeft() / 30.0;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_progressIndicator == null)
            {
                _progressIndicator = new ProgressIndicator();
                SystemTray.SetProgressIndicator(this, _progressIndicator);
            }
            
            StartTimer();
        }

        private void btnRemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/RemoveAccount.xaml", UriKind.Relative));
        }
    }
}