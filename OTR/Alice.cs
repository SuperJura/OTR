using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OTR
{
    public class Alice
    {
        public byte[] AlicePublicKey;
        byte[] AlicePrivateKey = null;
        byte[] encryptedMessage = null;
        byte[] iv = null;
        private ECDiffieHellmanCng alice;

        public Alice()
        {
            alice = new ECDiffieHellmanCng();

            alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            alice.HashAlgorithm = CngAlgorithm.Sha256;
            AlicePublicKey = alice.PublicKey.ToByteArray();
        }

        public Alice(byte[] bobPublicKey)
        {
            using (ECDiffieHellmanCng alice = new ECDiffieHellmanCng())
            {

                alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                alice.HashAlgorithm = CngAlgorithm.Sha256;
                AlicePublicKey = alice.PublicKey.ToByteArray();
                CngKey k = CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob);
                AlicePrivateKey = alice.DeriveKeyMaterial(CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }

        public void SetPrivateKey(byte[] bobPublicKey)
        {
            AlicePrivateKey = alice.DeriveKeyMaterial(CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
        }


        private void Send(byte[] publicKey, string secretMessage, out byte[] encryptedMessage, out byte[] iv)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = publicKey;
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

        private string Receive(byte[] encryptedMessage, byte[] iv)
        {

            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = AlicePrivateKey;
                aes.IV = iv;
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
