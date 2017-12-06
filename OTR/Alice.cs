using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OTR
{
    public class Alice : IOTRUser
    {
        public byte[] PublicKey;
        public byte[] PreviousDeriveKey = null;
        public byte[] CurrentDeriveKey = null;
        private ECDiffieHellmanCng alice;

        public Alice()
        {
            alice = new ECDiffieHellmanCng();

            alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            alice.HashAlgorithm = CngAlgorithm.Sha256;
            PublicKey = alice.PublicKey.ToByteArray();
        }

        public Alice(byte[] bobPublicKey)
        {
            using (ECDiffieHellmanCng alice = new ECDiffieHellmanCng())
            {

                alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                alice.HashAlgorithm = CngAlgorithm.Sha256;
                PublicKey = alice.PublicKey.ToByteArray();
                CurrentDeriveKey = alice.DeriveKeyMaterial(CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }

        public void SetDeriveKey(byte[] bobPublicKey)
        {
            CurrentDeriveKey = alice.DeriveKeyMaterial(CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
        }

        public void GenerateNewKey()
        {
            alice = new ECDiffieHellmanCng();
            alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            alice.HashAlgorithm = CngAlgorithm.Sha256;
            PublicKey = alice.PublicKey.ToByteArray();
            PreviousDeriveKey = CurrentDeriveKey;           
        }

        public void GenerateNewKey(byte[] bobPublicKey)
        {
            using (ECDiffieHellmanCng alice = new ECDiffieHellmanCng())
            {
                alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                alice.HashAlgorithm = CngAlgorithm.Sha256;
                PublicKey = alice.PublicKey.ToByteArray();
                PreviousDeriveKey = CurrentDeriveKey;
                CurrentDeriveKey = alice.DeriveKeyMaterial(CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }
    }
}
