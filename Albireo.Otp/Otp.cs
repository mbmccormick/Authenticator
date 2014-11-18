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
    using Windows.Security.Cryptography;

    internal static class Otp
    {
        internal const int DefaultDigits = 6;

        internal static int GetCode(
            HashAlgorithm algorithm,
            string secret,
            long counter,
            int digits)
        {
            //MacAlgorithmProvider algorithmProvider = MacAlgorithmProvider.OpenAlgorithm(algorithm.ToAlgorithmName());

            //var keyMaterial = CryptographicBuffer.ConvertStringToBinary(secret, BinaryStringEncoding.Utf8);

            //var hash = algorithmProvider.CreateHash(keyMaterial);
            //hash.Append(CounterToBytes(counter).AsBuffer());

            //var hmac = hash.GetValueAndReset().ToArray().Select(b => Convert.ToInt32(b)).ToArray();

            MacAlgorithmProvider algorithmProvider = MacAlgorithmProvider.OpenAlgorithm(algorithm.ToAlgorithmName());

            var keyMaterial = CryptographicBuffer.ConvertStringToBinary(secret, BinaryStringEncoding.Utf8);
            var key = algorithmProvider.CreateKey(keyMaterial);

            var hash = CryptographicEngine.Sign(key, CounterToBytes(counter).AsBuffer());

            byte[] hashArray = new byte[hash.Length];
            CryptographicBuffer.CopyToByteArray(hash, out hashArray);

            var hmac = hashArray.Select(b => Convert.ToInt32(b)).ToArray();
            
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
