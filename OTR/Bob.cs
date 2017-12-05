using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OTR
{
    public class Bob
    {
        public byte[] BobPublicKey;
        internal byte[] BobDeriveKey;
        private ECDiffieHellmanCng bob;
        public Bob()
        {
            bob = new ECDiffieHellmanCng();
            
            bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            bob.HashAlgorithm = CngAlgorithm.Sha256;
            BobPublicKey = bob.PublicKey.ToByteArray();
            
        }


        public Bob(byte[] alicePublicKey)
        {
            using (ECDiffieHellmanCng bob = new ECDiffieHellmanCng())
            {

                bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                bob.HashAlgorithm = CngAlgorithm.Sha256;
                BobPublicKey = bob.PublicKey.ToByteArray();
                BobDeriveKey = bob.DeriveKeyMaterial(CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }

        public void SetPrivateKey(byte[] alicePublicKey)
        {
            BobDeriveKey = bob.DeriveKeyMaterial(CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
        }
    }
}
