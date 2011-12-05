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
using System.Collections;
using System.ComponentModel;

namespace Authenticator
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Authenticator.App _application = null;
        private ProgressIndicator _progressIndicator = null;

        ApplicationBarIconButton add; 
        ApplicationBarIconButton select;
        ApplicationBarIconButton delete;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            _application = (Authenticator.App)Application.Current;

            add = new ApplicationBarIconButton();
            add.IconUri = new Uri("/Resources/add.png", UriKind.RelativeOrAbsolute);
            add.Text = "add";
            add.Click += add_Click; 
            
            select = new ApplicationBarIconButton();
            select.IconUri = new Uri("/Resources/select.png", UriKind.RelativeOrAbsolute);
            select.Text = "select";
            select.Click += select_Click;

            delete = new ApplicationBarIconButton();
            delete.IconUri = new Uri("/Resources/delete.png", UriKind.RelativeOrAbsolute);
            delete.Text = "delete";
            delete.Click += delete_Click;

            this.lstAccounts.ItemsSource = _application.Database;

            CodeGenerator.intervalLength = 30;
            CodeGenerator.pinCodeLength = 6;
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

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (this.lstAccounts.IsSelectionEnabled)
            {
                this.lstAccounts.IsSelectionEnabled = false;
                e.Cancel = true;
            }
        }

        private void add_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddPage.xaml", UriKind.Relative));
        }

        private void select_Click(object sender, EventArgs e)
        {
            this.lstAccounts.IsSelectionEnabled = true;
        }

        private void delete_Click(object sender, EventArgs e)
        {
            if (this.lstAccounts.SelectedItems.Count == 1)
            {
                if (MessageBox.Show("Are you sure you want to delete the selected account?", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    while (this.lstAccounts.SelectedItems.Count > 0)
                    {
                        _application.Database.Remove((Account)this.lstAccounts.SelectedItems[0]);
                    }
                }
            }
            else if (this.lstAccounts.SelectedItems.Count > 1)
            {
                if (MessageBox.Show("Are you sure you want to delete the selected accounts?", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    while (this.lstAccounts.SelectedItems.Count > 0)
                    {
                        _application.Database.Remove((Account)this.lstAccounts.SelectedItems[0]);
                    }
                }
            }
        }

        private void StartTimer()
        {
            System.Windows.Threading.DispatcherTimer myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();

            myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            myDispatcherTimer.Tick += new EventHandler(Timer_Tick);
            myDispatcherTimer.Start();
        }

        private void Timer_Tick(object o, EventArgs sender)
        {
            foreach (Account a in _application.Database)
            {
                CodeGenerator cg = new CodeGenerator(6, 30);
                string code = cg.computePin(a.SecretKey);

                a.Code = code;
            }

            if (_application.Database.Count > 0)
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

        private void lstAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MultiselectList target = (MultiselectList)sender;
            ApplicationBarIconButton i = (ApplicationBarIconButton)ApplicationBar.Buttons[0];

            if (target.IsSelectionEnabled)
            {
                if (target.SelectedItems.Count > 0)
                {
                    i.IsEnabled = true;
                }
                else
                {
                    i.IsEnabled = false;
                }
            }
            else
            {
                i.IsEnabled = true;
            }
        }

        private void lstAccounts_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            while (ApplicationBar.Buttons.Count > 0)
            {
                ApplicationBar.Buttons.RemoveAt(0);
            }

            while (ApplicationBar.MenuItems.Count > 0)
            {
                ApplicationBar.MenuItems.RemoveAt(0);
            }

            if ((bool)e.NewValue)
            {
                ApplicationBar.Buttons.Add(delete);
                ApplicationBarIconButton i = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                i.IsEnabled = false;
            }
            else
            {
                ApplicationBar.Buttons.Add(add);
                ApplicationBar.Buttons.Add(select);
            }
        }

        private void ItemContent_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Account item = ((FrameworkElement)sender).DataContext as Account;
            if (this.lstAccounts.IsSelectionEnabled)
            {
                MultiselectItem container = this.lstAccounts.ItemContainerGenerator.ContainerFromItem(item) as MultiselectItem;
                if (container != null)
                {
                    container.IsSelected = !container.IsSelected;
                }
            }
        }
    }
}