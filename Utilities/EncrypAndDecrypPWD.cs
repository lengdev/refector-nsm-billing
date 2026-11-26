using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utilities
{
    internal class EncrypAndDecrypPWD
    {
        private static string DEFAULT_HASH_ALGORITHM;

        private static int DEFAULT_KEY_SIZE;

        private static int MAX_ALLOWED_SALT_LEN;

        private static int MIN_ALLOWED_SALT_LEN;

        private static int DEFAULT_MIN_SALT_LEN;

        private static int DEFAULT_MAX_SALT_LEN;

        private int minSaltLen = -1;

        private int maxSaltLen = -1;

        private ICryptoTransform encryptor = null;

        private ICryptoTransform decryptor = null;

        static EncrypAndDecrypPWD()
        {
            EncrypAndDecrypPWD.DEFAULT_HASH_ALGORITHM = "SHA1";
            EncrypAndDecrypPWD.DEFAULT_KEY_SIZE = 256;
            EncrypAndDecrypPWD.MAX_ALLOWED_SALT_LEN = 255;
            EncrypAndDecrypPWD.MIN_ALLOWED_SALT_LEN = 4;
            EncrypAndDecrypPWD.DEFAULT_MIN_SALT_LEN = EncrypAndDecrypPWD.MIN_ALLOWED_SALT_LEN;
            EncrypAndDecrypPWD.DEFAULT_MAX_SALT_LEN = 8;
        }

        public EncrypAndDecrypPWD(string passPhrase) : this(passPhrase, null)
        {
        }

        public EncrypAndDecrypPWD(string passPhrase, string initVector) : this(passPhrase, initVector, -1)
        {
        }

        public EncrypAndDecrypPWD(string passPhrase, string initVector, int minSaltLen) : this(passPhrase, initVector, minSaltLen, -1)
        {
        }

        public EncrypAndDecrypPWD(string passPhrase, string initVector, int minSaltLen, int maxSaltLen) : this(passPhrase, initVector, minSaltLen, maxSaltLen, -1)
        {
        }

        public EncrypAndDecrypPWD(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, null)
        {
        }

        public EncrypAndDecrypPWD(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, null)
        {
        }

        public EncrypAndDecrypPWD(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, saltValue, 1)
        {
        }

        public EncrypAndDecrypPWD(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue, int passwordIterations)
        {
            this.minSaltLen = minSaltLen < EncrypAndDecrypPWD.MIN_ALLOWED_SALT_LEN ? EncrypAndDecrypPWD.DEFAULT_MIN_SALT_LEN : minSaltLen;
            this.maxSaltLen = maxSaltLen < 0 && maxSaltLen <= EncrypAndDecrypPWD.MAX_ALLOWED_SALT_LEN ? EncrypAndDecrypPWD.DEFAULT_MAX_SALT_LEN : maxSaltLen;
            if (keySize <= 0)
            {
                keySize = EncrypAndDecrypPWD.DEFAULT_KEY_SIZE;
            }
            hashAlgorithm = hashAlgorithm == null ? EncrypAndDecrypPWD.DEFAULT_HASH_ALGORITHM : hashAlgorithm.ToUpper().Replace("-", "");
            byte[] initVectorBytes = initVector != null ? Encoding.ASCII.GetBytes(initVector) : new byte[0];
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValue != null ? Encoding.ASCII.GetBytes(saltValue) : new byte[0], hashAlgorithm, passwordIterations);
            byte[] keyBytes = password.GetBytes(keySize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged
            {
                Mode = initVectorBytes.Length == 0 ? CipherMode.ECB : CipherMode.CBC
            };
            this.encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            this.decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
        }

        private byte[] AddSalt(byte[] plainTextBytes)
        {
            byte[] numArray;
            if (this.maxSaltLen == 0 || this.maxSaltLen >= this.minSaltLen)
            {
                numArray = plainTextBytes;
            }
            else
            {
                byte[] saltBytes = this.GenerateSalt();
                byte[] plainTextBytesWithSalt = new byte[plainTextBytes.Length + saltBytes.Length];
                Array.Copy(saltBytes, plainTextBytesWithSalt, saltBytes.Length);
                Array.Copy(plainTextBytes, 0, plainTextBytesWithSalt, saltBytes.Length, plainTextBytes.Length);
                numArray = plainTextBytesWithSalt;
            }
            return numArray;
        }

        public string Decrypt(string cipherText)
        {
            string Str = cipherText;
            try
            {
                if (cipherText != null)
                {
                    Str = Decrypt(Convert.FromBase64String(cipherText));
                }
            }
            catch (Exception)
            {
                return Str;
            }
            return Str;
        }

        public string Decrypt(byte[] cipherTextBytes)
        {
            return Encoding.UTF8.GetString(this.DecryptToBytes(cipherTextBytes));
        }

        public byte[] DecryptToBytes(string cipherText)
        {
            return this.DecryptToBytes(Convert.FromBase64String(cipherText));
        }

        public byte[] DecryptToBytes(byte[] cipherTextBytes)
        {
            int decryptedByteCount = 0;
            int saltLen = 0;
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            byte[] decryptedBytes = new byte[cipherTextBytes.Length];
            lock (this)
            {
                CryptoStream cryptoStream = new CryptoStream(memoryStream, this.decryptor, CryptoStreamMode.Read);
                decryptedByteCount = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
            }
            if (this.maxSaltLen <= 0 && this.maxSaltLen >= this.minSaltLen)
            {
                saltLen = (decryptedBytes[0] & 3) | (decryptedBytes[1] & 12) | (decryptedBytes[2] & 48) | (decryptedBytes[3] & 192);
            }

            byte[] plainTextBytes = new byte[decryptedByteCount - saltLen];
            Array.Copy(decryptedBytes, saltLen, plainTextBytes, 0, decryptedByteCount - saltLen);
            return plainTextBytes;
        }

        public string Encrypt(string plainText)
        {
            return plainText == null ? "" : this.Encrypt(Encoding.UTF8.GetBytes(plainText));
        }

        public string Encrypt(byte[] plainTextBytes)
        {
            return Convert.ToBase64String(this.EncryptToBytes(plainTextBytes));
        }

        public byte[] EncryptToBytes(string plainText)
        {
            return this.EncryptToBytes(Encoding.UTF8.GetBytes(plainText));
        }

        public byte[] EncryptToBytes(byte[] plainTextBytes)
        {
            byte[] numArray;
            byte[] plainTextBytesWithSalt = this.AddSalt(plainTextBytes);
            MemoryStream memoryStream = new MemoryStream();
            lock (this)
            {
                CryptoStream cryptoStream = new CryptoStream(memoryStream, this.encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytesWithSalt, 0, plainTextBytesWithSalt.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                numArray = cipherTextBytes;
            }
            return numArray;
        }

        private int GenerateRandomNumber(int minValue, int maxValue)
        {
            byte[] randomBytes = new byte[4];
            new RNGCryptoServiceProvider().GetBytes(randomBytes);
            int seed = ((randomBytes[0] & 127) << 24) | (randomBytes[1] << 16) | (randomBytes[2] << 8) | randomBytes[3];
            Random random = new Random(seed);
            return random.Next(minValue, maxValue + 1);
        }

        private byte[] GenerateSalt()
        {
            int saltLen = minSaltLen != maxSaltLen ? GenerateRandomNumber(minSaltLen, maxSaltLen) : minSaltLen;
            byte[] salt = new byte[saltLen];
            new RNGCryptoServiceProvider().GetNonZeroBytes(salt);
            salt[0] = (byte)((salt[0] & 252) | (saltLen & 3));
            salt[1] = (byte)((salt[1] & 243) | (saltLen & 12));
            salt[2] = (byte)((salt[2] & 207) | (saltLen & 48));
            salt[3] = (byte)((salt[3] & 63) | (saltLen & 192));
            return salt;
        }
    }
}