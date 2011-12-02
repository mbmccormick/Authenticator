using System;
using System.Collections.Generic;

namespace GAuthenticator
{
    public class AccountDB
    {
        public List<Account> Accounts { get; set; }

        public AccountDB()
        {
            Accounts = new List<Account>();
        }
    }
}