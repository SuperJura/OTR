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
        internal byte[] PreviousAliceDeriveKey = null;
        internal byte[] CurrentAliceDeriveKey = null;
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
                CurrentAliceDeriveKey = alice.DeriveKeyMaterial(CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }

        public void SetPrivateKey(byte[] bobPublicKey)
        {
            CurrentAliceDeriveKey = alice.DeriveKeyMaterial(CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
        }

        public void GenerateNewKey()
        {
            using (ECDiffieHellmanCng alice = new ECDiffieHellmanCng())
            {
                alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                alice.HashAlgorithm = CngAlgorithm.Sha256;
                AlicePublicKey = alice.PublicKey.ToByteArray();
                PreviousAliceDeriveKey = CurrentAliceDeriveKey;
            }
        }

        public void GenerateNewKey(byte[] bobPublicKey)
        {
            using (ECDiffieHellmanCng alice = new ECDiffieHellmanCng())
            {
                alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                alice.HashAlgorithm = CngAlgorithm.Sha256;
                AlicePublicKey = alice.PublicKey.ToByteArray();
                PreviousAliceDeriveKey = CurrentAliceDeriveKey;
                CurrentAliceDeriveKey = alice.DeriveKeyMaterial(CngKey.Import(bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }
    }
}
