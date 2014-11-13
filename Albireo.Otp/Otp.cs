namespace Albireo.Otp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Security.Cryptography.Core;

    using Albireo.Base32;

    internal static class Otp
    {
        internal const int DefaultDigits = 6;

        internal static int GetCode(
            HashAlgorithm algorithm,
            string secret,
            long counter,
            int digits)
        {
            //Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(HashAlgorithm), algorithm));
            //Contract.Requires<ArgumentOutOfRangeException>(algorithm != HashAlgorithm.Unknown);
            //Contract.Requires<ArgumentNullException>(secret != null);
            //Contract.Requires<ArgumentOutOfRangeException>(counter >= 0);
            //Contract.Requires<ArgumentOutOfRangeException>(digits > 0);
            //Contract.Ensures(Contract.Result<int>() > 0);
            //Contract.Ensures(Contract.Result<int>() < Math.Pow(10, digits));

            MacAlgorithmProvider algorithmProvider = MacAlgorithmProvider.OpenAlgorithm(algorithm.ToAlgorithmName());

            byte[] bytes = new byte[secret.Length * sizeof(char)];
            System.Buffer.BlockCopy(secret.ToCharArray(), 0, bytes, 0, bytes.Length);

            var hash = algorithmProvider.CreateHash(bytes.AsBuffer());
            hash.Append(CounterToBytes(counter).AsBuffer());

            var hmac = hash.GetValueAndReset().ToArray().Select(b => Convert.ToInt32(b)).ToArray();

            var offset = hmac[19] & 0xF;

            var code =
                (hmac[offset + 0] & 0x7F) << 24
                | (hmac[offset + 1] & 0xFF) << 16
                | (hmac[offset + 2] & 0xFF) << 8
                | (hmac[offset + 3] & 0xFF);

            return code % (int)Math.Pow(10, digits);
        }

        internal static string GetKeyUri(
            OtpType type,
            string issuer,
            string account,
            byte[] secret,
            HashAlgorithm algorithm,
            int digits,
            long counter,
            int period)
        {
            //Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(OtpType), type));
            //Contract.Requires<ArgumentOutOfRangeException>(type != OtpType.Unknown);
            //Contract.Requires<ArgumentNullException>(issuer != null);
            //Contract.Requires<ArgumentOutOfRangeException>(!string.IsNullOrWhiteSpace(issuer));
            //Contract.Requires<ArgumentNullException>(account != null);
            //Contract.Requires<ArgumentOutOfRangeException>(!string.IsNullOrWhiteSpace(account));
            //Contract.Requires<ArgumentNullException>(secret != null);
            //Contract.Requires<ArgumentException>(secret.Length > 0);
            //Contract.Requires<ArgumentOutOfRangeException>(Enum.IsDefined(typeof(HashAlgorithm), algorithm));
            //Contract.Requires<ArgumentOutOfRangeException>(algorithm != HashAlgorithm.Unknown);
            //Contract.Requires<ArgumentOutOfRangeException>(digits > 0);
            //Contract.Requires<ArgumentOutOfRangeException>(counter >= 0);
            //Contract.Requires<ArgumentOutOfRangeException>(period > 0);
            //Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return
                string.Format(
                    CultureInfo.InvariantCulture,
                    "otpauth://{0}/{1}:{2}?secret={3}&issuer={4}&algorithm={5}&digits={6}&counter={7}&period={8}",
                    type.ToKeyUriValue(),
                    WebUtility.UrlEncode(issuer),
                    WebUtility.UrlEncode(account),
                    Base32.Encode(secret),
                    WebUtility.UrlEncode(issuer),
                    algorithm.ToKeyUriValue(),
                    digits,
                    counter,
                    period);
        }

        private static byte[] CounterToBytes(long counter)
        {
            //Contract.Requires<ArgumentOutOfRangeException>(counter >= 0);
            //Contract.Ensures(Contract.Result<byte[]>() != null);

            var result = new List<byte>();

            while (counter != 0)
            {
                result.Add((byte)(counter & 0xFF));
                counter >>= 8;
            }

            for (int i = 0, j = 8 - result.Count; i < j; i++)
            {
                result.Add(0);
            }

            result.Reverse();

            return result.ToArray();
        }
    }
}
