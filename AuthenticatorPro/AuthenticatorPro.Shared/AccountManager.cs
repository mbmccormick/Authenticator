using System;
using System.Collections.Generic;
using System.Text;
using Windows.Security.Credentials;
using Windows.Storage;

namespace AuthenticatorPro
{
    public class AccountManager
    {
        public static void DeserializeAccounts()
        {
            if (App.RoamAccountSecrets)
            {
                DeserializeRoamedAccounts();
            }
            else
            {
                App.Accounts.Clear();

                foreach (var value in ApplicationData.Current.LocalSettings.Values)
                {
                    Account a = new Account();
                    a.Name = value.Key;
                    a.SecretKey = value.Value as string;

                    App.Accounts.Add(a);
                }
            }
        }

        public static void SerializeAccounts()
        {
            if (App.RoamAccountSecrets)
            {
                SerializeRoamedAccounts();
            }
            else
            {
                ClearLocalAccounts();

                foreach (var a in App.Accounts)
                {
                    ApplicationData.Current.LocalSettings.Values[a.Name] = a.SecretKey;
                }
            }
        }

        public static void ClearLocalAccounts()
        {
            ApplicationData.Current.LocalSettings.Values.Clear();
        }

        public static void DeserializeRoamedAccounts()
        {
            PasswordVault vault = new PasswordVault();

            App.Accounts.Clear();

            foreach (var credential in vault.RetrieveAll())
            {
                Account a = new Account();
                a.Name = credential.UserName;
                a.SecretKey = credential.Password;

                App.Accounts.Add(a);
            }
        }

        public static void SerializeRoamedAccounts()
        {
            PasswordVault vault = new PasswordVault();

            ClearRoamedAccounts();

            foreach (var a in App.Accounts)
            {
                PasswordCredential credential = new PasswordCredential();
                credential.Resource = "Authenticator Pro";
                credential.UserName = a.Name;
                credential.Password = a.SecretKey;

                vault.Add(credential);
            }
        }

        public static void ClearRoamedAccounts()
        {
            PasswordVault vault = new PasswordVault();

            foreach (var credential in vault.RetrieveAll())
            {
                vault.Remove(credential);
            }
        }
    }
}
