using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace Utilities
{
    public class EncryptionPassword
    {
        private static string _Salt;

        public static string Salt
        {
            get
            {
                return EncryptionPassword._Salt;
            }
            set
            {
                EncryptionPassword._Salt = value;
            }
        }

        static EncryptionPassword()
        {
            EncryptionPassword._Salt = "";
        }

        public EncryptionPassword()
        {
        }

        private static string BytesToHex(byte[] toConvert)
        {
            StringBuilder s = new StringBuilder(toConvert.Length * 2);
            byte[] numArray = toConvert;
            for (int i = 0; i < numArray.Length; i++)
            {
                byte b = numArray[i];
                s.Append(b.ToString("x2"));
            }
            return s.ToString();
        }

        [Obsolete]
        public static string CreatePasswordHash(string pwd, string salt)
        {
            string saltAndPwd = string.Concat(pwd, salt);
            return FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, "sha1");
        }

        private static string CreateSalt(int size)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }

        public static string GetRandomSalt()
        {
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] salt = new byte[32];
            random.GetBytes(salt);
            return EncryptionPassword.BytesToHex(salt);
        }

        public static string HashPassword(string password)
        {
            string salt = EncryptionPassword.GetRandomSalt();
            string hash = EncryptionPassword.Sha256Hex(string.Concat(salt, password));
            return string.Concat(salt, hash);
        }

        private static string Sha256Hex(string toHash)
        {
            SHA256Managed hash = new SHA256Managed();
            byte[] utf8 = Encoding.UTF8.GetBytes(toHash);
            return EncryptionPassword.BytesToHex(hash.ComputeHash(utf8));
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            if (correctHash.Length < 128)
            {
                throw new ArgumentException("correctHash must be 128 hex characters!");
            }
            string salt = correctHash.Substring(0, 64);
            string validHash = correctHash.Substring(64, 64);
            string passHash = EncryptionPassword.Sha256Hex(string.Concat(salt, password));
            return string.Compare(validHash, passHash) == 0;
        }
    }
}