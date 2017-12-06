using System.Security.Cryptography;

namespace OTR
{
    public class User
    {
        public byte[] PublicKey;
        public byte[] PreviousDeriveKey;
        public byte[] CurrentDeriveKey;
        private ECDiffieHellmanCng algorithm;

        public User()
        {
            InitAlgorithm();
            PublicKey = algorithm.PublicKey.ToByteArray();
        }

        private void InitAlgorithm()
        {
            algorithm = new ECDiffieHellmanCng();
            algorithm.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            algorithm.HashAlgorithm = CngAlgorithm.Sha256;
        }

        public User(byte[] otherPubKey)
        {
            InitAlgorithm();
            PublicKey = algorithm.PublicKey.ToByteArray();
            CurrentDeriveKey = algorithm.DeriveKeyMaterial(CngKey.Import(otherPubKey, CngKeyBlobFormat.EccPublicBlob));
        }

        public void SetDeriveKey(byte[] alicePublicKey)
        {
            CurrentDeriveKey = algorithm.DeriveKeyMaterial(CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob));
        }

        public void GenerateNewKey()
        {
            algorithm = new ECDiffieHellmanCng();
            algorithm.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            algorithm.HashAlgorithm = CngAlgorithm.Sha256;
            PublicKey = algorithm.PublicKey.ToByteArray();
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
