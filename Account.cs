using System;

namespace GAuthenticator
{
    public class Account
    {
        public string AccountName { get; set; }
        public string SecretKey { get; set; }

        public string CalculatePin()
        {
            GPinGenerator pg = new GPinGenerator(6, 30);
            string mPIN = pg.computePin(SecretKey);
            return mPIN;
        }
    }
}
