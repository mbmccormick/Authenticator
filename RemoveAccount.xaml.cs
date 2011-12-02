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
    public partial class RemoveAccount : PhoneApplicationPage
    {
        private App a;
        public RemoveAccount()
        {
            InitializeComponent();
            a = (App)Application.Current;
            foreach (Account acct in a.WorkingDB.Accounts)
            {
                lstAccounts.Items.Add(acct);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            if (lstAccounts.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Are you sure you wish you remove the selected accounts?", "Remove?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    foreach (Account acct in lstAccounts.SelectedItems)
                    {
                        a.WorkingDB.Accounts.Remove(acct);
                    }
                    NavigationService.GoBack();
                }
            }
        }
    }
}