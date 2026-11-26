namespace Utilities
{
    public class EncryAndDecry
    {
        private static string passPhrase;

        private static string initVector;
        private static string saltValue;
        static EncryAndDecry()
        {
            passPhrase = "Pas5prse";
            initVector = "1B2c3D4e5F6g7HabVC8";
            saltValue = "thea";
        }

        public EncryAndDecry()
        {
        }

        public static string Decryp(string Field)
        {
            EncrypAndDecrypPWD enp = new EncrypAndDecrypPWD(passPhrase, initVector, 4, 8, 256, "MD5", saltValue, 2);
            return enp.Decrypt(Field);
        }

        public static string Encryp(string Field)
        {
            if (Field == "") return Field;

            EncrypAndDecrypPWD enp = new EncrypAndDecrypPWD(passPhrase, initVector, 4, 8, 256, "MD5", saltValue, 2);
            return enp.Encrypt(Field);
        }
    }
}