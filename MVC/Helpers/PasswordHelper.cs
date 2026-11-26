//using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace MVC.Helpers
{
    public class PasswordHelper
    {
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const int IterationCount = 10000;
        public static string GeneratePasswordHash(string password, out string salt)
        {
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, SaltSize))
            {
                rfc2898DeriveBytes.IterationCount = IterationCount;
                byte[] hashData = rfc2898DeriveBytes.GetBytes(HashSize);
                byte[] saltData = rfc2898DeriveBytes.Salt;
                salt = Convert.ToBase64String(saltData);
                return Convert.ToBase64String(hashData);
            }
        }
        public static bool VerifyPassword(string password, string passwordHash, string PasswordSalt)
        {
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, SaltSize))
            {
                rfc2898DeriveBytes.IterationCount = IterationCount;
                rfc2898DeriveBytes.Salt = Convert.FromBase64String(PasswordSalt);
                byte[] hashData = rfc2898DeriveBytes.GetBytes(HashSize);
                return Convert.ToBase64String(hashData) == passwordHash;
            }
        }
        public static string HashPassword(string password, byte[] saltBytes)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                return Convert.ToBase64String(hash);
            }
        }
        public static string HashPassword(string password, out string salt)
        {
            byte[] saltBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000);
            byte[] hash = pbkdf2.GetBytes(32);

            salt = Convert.ToBase64String(saltBytes);
            return Convert.ToBase64String(hash);
        }

        public static byte[] GetSecureSalt()
        {
            //char[] chars = new char[32];
            //chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            //byte[] data = new byte[1];
            //using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            //{
            //    crypto.GetNonZeroBytes(data);
            //    data = new byte[32];
            //    crypto.GetNonZeroBytes(data);
            //}

            byte[] saltByte = new byte[SaltSize];

            using (var random = new RNGCryptoServiceProvider())
            {

                random.GetBytes(saltByte);
            }
            return saltByte;
        }
        public static string HashUsingPbkdf2(string password, out string salt)
        {
            byte[] saltData = new byte[SaltSize];

            using (var random = new RNGCryptoServiceProvider())
            {

                random.GetBytes(saltData);
            }
            byte[] derivedKey = KeyDerivation.Pbkdf2(password, saltData, KeyDerivationPrf.HMACSHA256, iterationCount: 100000, HashSize);
            salt = Convert.ToBase64String(saltData);
            return Convert.ToBase64String(derivedKey);
        }
        public static string HashUsingPbkdf2(string password, byte[] saltData)
        {
            byte[] derivedKey = KeyDerivation.Pbkdf2(password, saltData, KeyDerivationPrf.HMACSHA256, iterationCount: 100000, HashSize);
            return Convert.ToBase64String(derivedKey);
        }
    }
}