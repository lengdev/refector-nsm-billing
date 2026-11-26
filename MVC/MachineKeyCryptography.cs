
using System;
using System.Text;
using System.Web.Security;

namespace NSMSOLUTION.Library
{
    /// <summary>
    /// Protects and unprotects cookie values using ASP.NET MachineKey.
    /// </summary>
    internal static class MachineKeyCryptography
    {
        private static string[] GetPurpose(CookieProtection protection)
        {
            return new[]
            {
                "NSMSOLUTION",
                "CookieProtection",
                protection.ToString()
            };
        }

        public static string Encode(string text)
        {
            return Encode(text, CookieProtection.All);
        }

        public static string Encode(string text, CookieProtection protection)
        {
            if (text == null)
                return null;

            if (protection == CookieProtection.None)
                return text;

            if (string.IsNullOrEmpty(text))
                return text;

            try
            {
                byte[] input = Encoding.UTF8.GetBytes(text);

                byte[] protectedBytes =
                    MachineKey.Protect(input, GetPurpose(protection));

                if (protectedBytes == null || protectedBytes.Length == 0)
                    throw new CookieSecureException(
                        "MachineKey.Protect returned empty data.");

                return Convert.ToBase64String(protectedBytes);
            }
            catch (CookieSecureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CookieSecureException(
                    "Unable to protect cookie data.", ex);
            }
        }

        public static string Decode(string text)
        {
            return Decode(text, CookieProtection.All);
        }

        public static string Decode(
            string text,
            CookieProtection protection)
        {
            if (text == null)
                return null;

            if (protection == CookieProtection.None)
                return text;

            if (string.IsNullOrEmpty(text))
                return text;

            byte[] inputBytes;

            try
            {
                inputBytes = Convert.FromBase64String(text);
            }
            catch (FormatException ex)
            {
                throw new CookieSecureException(
                    "Cookie data is not valid Base64.", ex);
            }

            if (inputBytes.Length == 0)
            {
                throw new CookieSecureException(
                    "Cookie data is empty.");
            }

            Exception currentError = null;

            // 1. Try the current MachineKey.Protect format.
            try
            {
                byte[] decodedBytes =
                    MachineKey.Unprotect(
                        inputBytes,
                        GetPurpose(protection));

                if (decodedBytes != null && decodedBytes.Length > 0)
                    return Encoding.UTF8.GetString(decodedBytes);
            }
            catch (Exception ex)
            {
                currentError = ex;
            }

            // 2. Try the legacy MachineKey.Encode format.
            try
            {
#pragma warning disable CS0618
                byte[] legacyBytes =
                    MachineKey.Decode(text, Map(protection));
#pragma warning restore CS0618

                if (legacyBytes != null && legacyBytes.Length > 0)
                    return Encoding.UTF8.GetString(legacyBytes);
            }
            catch (Exception ex)
            {
                if (currentError == null)
                    currentError = ex;
            }

            throw new CookieSecureException(
                "Cookie could not be decrypted. The cookie may have been " +
                "created with a different machineKey, protection mode, " +
                "or purpose string. The cookie may also be corrupted.",
                currentError);
        }

        private static MachineKeyProtection Map(
            CookieProtection protection)
        {
            switch (protection)
            {
                case CookieProtection.Encryption:
                    return MachineKeyProtection.Encryption;

                case CookieProtection.Validation:
                    return MachineKeyProtection.Validation;

                default:
                    return MachineKeyProtection.All;
            }
        }
    }

    public class CookieSecureException : Exception
    {
        public CookieSecureException(string message)
            : base(message)
        {
        }

        public CookieSecureException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
