using System;
using System.Text;
using System.Web.Security;

namespace NSMSOLUTION.Library
{
    /// <summary>
    /// The machine key cryptography.
    /// </summary>
    internal static class MachineKeyCryptography
    {
        #region Methods

        /// <summary>
        /// Decodes a string and returns null if the string is tampered
        /// </summary>
        /// <param name="text">
        /// String to decode
        /// </param>
        /// <returns>
        /// The decoded string or throws <see cref="CookieSecureException"/> if tampered with
        /// </returns>
        public static string Decode(string text)
        {
            return Decode(text, CookieProtection.All);
        }

        /// <summary>
        /// Decodes a string
        /// </summary>
        /// <param name="text">
        /// String to decode
        /// </param>
        /// <param name="cookieProtection">
        /// The method in which the string is protected
        /// </param>
        /// <returns>
        /// The decoded string or throws <see cref="CookieSecureException"/> if tampered with
        /// </returns>
        public static string Decode(string text, CookieProtection cookieProtection)
        {
            if (string.IsNullOrEmpty(text) || cookieProtection == CookieProtection.None)
            {
                return text;
            }

            byte[] buf;

            var purposes = GetPurposes(cookieProtection);

            try
            {
                buf = MachineKey.Unprotect(Convert.FromBase64String(text), purposes);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); //CookieSecureException(Resources.CookieError1, ex.InnerException);
            }

            if (buf == null || buf.Length == 0)
            {
                //throw new ex.Message;//CookieSecureException(Resources.CookieError1);
                return string.Empty;
            }

            return Encoding.UTF8.GetString(buf, 0, buf.Length);
        }

        /// <summary>
        /// Encodes a string and protects it from tampering
        /// </summary>
        /// <param name="text">
        /// String to encode
        /// </param>
        /// <returns>
        /// Encoded string
        /// </returns>
        public static string Encode(string text)
        {
            return Encode(text, CookieProtection.All);
        }

        /// <summary>
        /// Encodes a string
        /// </summary>
        /// <param name="text">
        /// String to encode
        /// </param>
        /// <param name="cookieProtection">
        /// The method in which the string is protected
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string Encode(string text, CookieProtection cookieProtection)
        {
            if (string.IsNullOrEmpty(text) || cookieProtection == CookieProtection.None)
            {
                return text;
            }

            var buf = Encoding.UTF8.GetBytes(text);

            var purposes = GetPurposes(cookieProtection);

            var protectedBytes = MachineKey.Protect(buf, purposes);

            return Convert.ToBase64String(protectedBytes);
        }

        /// <summary>
        /// Maps <see cref="CookieProtection"/> to a stable "purposes" tag set for
        /// <see cref="MachineKey.Protect"/>/<see cref="MachineKey.Unprotect"/>.
        /// Unlike the old <see cref="MachineKeyProtection"/> enum, Protect/Unprotect always
        /// perform both encryption and validation — the purposes string is a binding/context
        /// tag, not a partial-protection mode. It must match exactly between Encode and Decode.
        /// </summary>
        /// <param name="cookieProtection">
        /// The cookie protection.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> array of purposes.
        /// </returns>
        private static string[] GetPurposes(CookieProtection cookieProtection)
        {
            return new[] { "NSMSOLUTION.MachineKeyCryptography", cookieProtection.ToString() };
        }

        #endregion Methods        
    }
}