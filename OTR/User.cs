using System.Security.Cryptography;
using System.Text;

namespace OTR
{
    public class User
    {
        public byte[] PublicKey;
        public byte[] PreviousDeriveKey;
        public byte[] CurrentDeriveKey;
        public byte[] KeyForSigning;
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
            InitAlgorithm();
            PublicKey = algorithm.PublicKey.ToByteArray();
            PreviousDeriveKey = CurrentDeriveKey;          
        }

        public void GenerateNewKey(byte[] otherPubKey)
        {
            using (ECDiffieHellmanCng user = new ECDiffieHellmanCng())
            {
                user.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                user.HashAlgorithm = CngAlgorithm.Sha256;
                PublicKey = user.PublicKey.ToByteArray();
                PreviousDeriveKey = CurrentDeriveKey;
                CurrentDeriveKey = user.DeriveKeyMaterial(CngKey.Import(otherPubKey, CngKeyBlobFormat.EccPublicBlob));
            }
        }

        public void SetKeyForSigning(string key)
        {
            KeyForSigning = Encoding.UTF8.GetBytes(key);
        }
    }
}
