using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OTR
{
    public static class IM
    {
        public static void OTRSend(byte[] publicKey, string secretMessage, out byte[] encryptedMessage, out byte[] iv)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = publicKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();
                iv = aes.IV;

                // Encrypt the message
                using (MemoryStream ciphertext = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] plaintextMessage = Encoding.UTF8.GetBytes(secretMessage);
                    cs.Write(plaintextMessage, 0, plaintextMessage.Length);
                    cs.Close();
                    encryptedMessage = ciphertext.ToArray();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="publicKey">Ključ pošiljavetlja</param>
        /// <param name="secretMessage"></param>
        /// <param name="encryptedMessage"></param>
        public static void OTRSend(byte[] publicKey, string secretMessage, out byte[] encryptedMessage)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = publicKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();
                //iv = aes.IV;

                // Encrypt the message
                using (MemoryStream ciphertext = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] plaintextMessage = Encoding.UTF8.GetBytes(secretMessage);
                    cs.Write(plaintextMessage, 0, plaintextMessage.Length);
                    cs.Close();
                    encryptedMessage = aes.IV;
                    encryptedMessage = encryptedMessage.Concat(ciphertext.ToArray()).ToArray();
                    //encryptedMessage = ciphertext.ToArray();
                }
            }
        }
        public static string OTRReceive(byte[] privateKey, byte[] encryptedMessage)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = privateKey;
                aes.IV = encryptedMessage.Take(aes.BlockSize / 8).ToArray();
                encryptedMessage = encryptedMessage.Skip(aes.BlockSize / 8).ToArray();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                // Decrypt the message
                using (MemoryStream plaintext = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedMessage, 0, encryptedMessage.Length);
                        cs.Close();
                        string message = Encoding.UTF8.GetString(plaintext.ToArray());
                        return message;
                    }
                }
            }
        }

        public static string OTRReceive(byte[] privateKey, byte[] encryptedMessage, byte[] iv)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = privateKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                // Decrypt the message
                using (MemoryStream plaintext = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedMessage, 0, encryptedMessage.Length);
                        cs.Close();
                        string message = Encoding.UTF8.GetString(plaintext.ToArray());
                        return message;
                    }
                }
            }
        }
    }
}
