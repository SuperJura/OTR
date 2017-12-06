using System;
using System.Text;
using static OTR.IM;

namespace OTR
{
    class Program
    {
        static void Main(string[] args)
        {
            User alice = new User();
            User bob = new User(alice.PublicKey);
            alice.SetDeriveKey(bob.PublicKey);

            byte[] SecretMsgToSend;
            byte[] iv;
            byte[] KeyForSigning = Encoding.UTF8.GetBytes("KeyForSigning");
            string Msg = "I am the bone of my sword";

            /*Slanje i primanje poruke*/
            OTRSend(alice.CurrentDeriveKey, Msg, out SecretMsgToSend,out iv);
            
            string DecryptedMsg = OTRReceive(bob.CurrentDeriveKey, SecretMsgToSend, iv);

            /* Poptpisivanje poruke i potvrđivanje */
            byte[] Signed = MAC.Sign(KeyForSigning, Msg);
            if (MAC.Verify(KeyForSigning, Signed))
            {
                Console.WriteLine("Potpis potvrđen");
            }
            else
            {
                Console.WriteLine("Potpis NIJE potvrđen");
            }

            /* Generiranje novih ključeva*/
            // 1. Alice generira svoj novi javni ključ
            alice.GenerateNewKey();
            // 2. Šalje Bobu svoj novi javni ključ u poruci enkriptiranu starim ključem
            Msg  = "Novi javni kljuc Alice";
            OTRSend(alice.PreviousDeriveKey, Msg, out SecretMsgToSend, out iv);
            // 3. Bob prima tu poruku te generira novi set ključeva sa Aliceinim javim ključem
            DecryptedMsg = OTRReceive(bob.CurrentDeriveKey, SecretMsgToSend, iv);
            bob.GenerateNewKey(alice.PublicKey);
            // 4. Bob šalje Alice svoj novi javni ključ enkriptiran starim ključem
            Msg = "Novi javni kljuc Bob";
            OTRSend(bob.PreviousDeriveKey, Msg, out SecretMsgToSend, out iv);
            // 5. Alice prima poruku sa Bobovim javim ključem i dekriptira sa starim ključem
            DecryptedMsg = OTRReceive(alice.PreviousDeriveKey, SecretMsgToSend, iv);
            // 6. Alice postavlja novi ključ koristeči Bobov novi javni ključ i zaboravlja stari
            alice.SetDeriveKey(bob.PublicKey);
            alice.PreviousDeriveKey = null;
            // 7. Alice šalje Bobu novu poruku enkriptiranu sa novim ključem kako bi potvrdila da je zaprimila novi javni ključ
            OTRSend(alice.CurrentDeriveKey, Msg, out SecretMsgToSend, out iv);
            DecryptedMsg = OTRReceive(bob.CurrentDeriveKey, SecretMsgToSend, iv);
            //8. Bob zaboravlja stari ključ;
            bob.PreviousDeriveKey = null;

        }
    }
}
