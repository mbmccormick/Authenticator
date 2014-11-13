using System;
using Albireo.Otp;

namespace AuthenticatorPro
{
    public static class CodeGenerator
    {
        public static string ComputeCode(string secret)
        {
            DateTime currentTime = DateTime.UtcNow.AddTicks(App.NtpTimeOffset.Ticks);
            int code = Totp.GetCode(HashAlgorithm.Sha1, secret, currentTime);

            return code.ToString();
        }

        public static double TimeElapsed()
        {
            DateTime currentTime = DateTime.UtcNow.AddTicks(App.NtpTimeOffset.Ticks);
            TimeSpan timeSpan = currentTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            double timeElapsed = timeSpan.TotalMilliseconds % (30.0 * 1000);

            return timeElapsed / (30.0 * 1000);
        }
    }
}
