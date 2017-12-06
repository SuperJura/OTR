using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OTR
{
    public class Bob : IOTRUser
    {
        public byte[] PublicKey;
        internal byte[] PreviousDeriveKey;
        internal byte[] CurrentDeriveKey;
        private ECDiffieHellmanCng bob;
        public Bob()
        {
            bob = new ECDiffieHellmanCng();           
            bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            bob.HashAlgorithm = CngAlgorithm.Sha256;
            PublicKey = bob.PublicKey.ToByteArray();        
        }


        public Bob(byte[] alicePublicKey)
        {
            using (ECDiffieHellmanCng bob = new ECDiffieHellmanCng())
            {

                bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                bob.HashAlgorithm = CngAlgorithm.Sha256;
                PublicKey = bob.PublicKey.ToByteArray();
                CurrentDeriveKey = bob.DeriveKeyMaterial(CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }

        public void SetDeriveKey(byte[] alicePublicKey)
        {
            CurrentDeriveKey = bob.DeriveKeyMaterial(CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
        }

        public void GenerateNewKey()
        {
            bob = new ECDiffieHellmanCng();
            bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            bob.HashAlgorithm = CngAlgorithm.Sha256;
            PublicKey = bob.PublicKey.ToByteArray();
            PreviousDeriveKey = CurrentDeriveKey;          
        }

        public void GenerateNewKey(byte[] alicePublicKey)
        {
            using (ECDiffieHellmanCng bob = new ECDiffieHellmanCng())
            {
                bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                bob.HashAlgorithm = CngAlgorithm.Sha256;
                PublicKey = bob.PublicKey.ToByteArray();
                PreviousDeriveKey = CurrentDeriveKey;
                CurrentDeriveKey = bob.DeriveKeyMaterial(CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }
    }
}
