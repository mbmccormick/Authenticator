using System;
using System.Collections.Generic;

namespace Authenticator
{
    public class Accounts
    {
        public List<Account> AccountList { get; set; }

        public Accounts()
        {
            AccountList = new List<Account>();
        }
    }
}