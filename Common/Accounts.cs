using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Authenticator
{
    public class Accounts : ObservableCollection<Account>
    {
        public Accounts()
            : base()
        {
        }
    }
}