namespace AssaultCubeHack
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class ENCRYDECRYPTO
    {


        // ENCRYPT
        // Public Function ENCRYPTO(keys)

        // Dim TextInBytes() As Byte = Encoding.ASCII.GetBytes(keys)
        // Dim crypt As New DESCryptoServiceProvider()
        // Dim KEYB() As Byte = Encoding.ASCII.GetBytes(Decode(_busy))
        // crypt.Key = KEYB
        // crypt.IV = KEYB
        // Dim Icrypt As ICryptoTransform = crypt.CreateEncryptor()
        // Dim result() As Byte = Icrypt.TransformFinalBlock(TextInBytes, 0, TextInBytes.Length)
        // keys = Convert.ToBase64String(result)

        // Return keys

        // End Function

        public static string _busy = "_1Z00@1-"
+ "0à@19-1"
+ "9(@01àn90J-10-i9à@1c-100à@1-G"
+ "101(90à-t@t_àb1)@90à2-19"
+ ")à9-@8-1=";

        public static string DECRYPTO(string keys)
        {
            byte[] Resultat = Convert.FromBase64String((string)keys);
            DESCryptoServiceProvider crypt = new DESCryptoServiceProvider();
            byte[] KEYB = Encoding.ASCII.GetBytes(Decode(_busy));
            crypt.Key = KEYB;
            crypt.IV = KEYB;
            ICryptoTransform Icrypt = crypt.CreateDecryptor();

            byte[] data = Icrypt.TransformFinalBlock(Resultat, 0, Resultat.Length);
            keys = Encoding.UTF8.GetString(data);

            return (string)keys;
        }

        public static string Decode08(string text)
        {
            text = text.Replace("_", null).Replace("-", null).Replace("(", null).Replace(")", null).Replace("@", null).Replace("à", null);
            return Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }

        public static string Decode(string text)
        {
            text = text.Replace("_", null).Replace("-", null).Replace("(", null).Replace(")", null).Replace("0", null).Replace("1", null).Replace("9", null).Replace("@", null).Replace("à", null);
            return Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }

        public static string Decode2(string text)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }
    }
}
