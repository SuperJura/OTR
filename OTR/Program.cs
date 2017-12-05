using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTR
{
    class Program
    {
        static void Main(string[] args)
        {
            Alice alice = new Alice();
            Bob bob = new Bob(alice.AlicePublicKey);
            alice.SetPrivateKey(bob.BobPublicKey);
            byte[] SecretMsgToSend;
            byte[] iv;
            byte[] KeyForSigning = Encoding.UTF8.GetBytes("KeyForSigning");
            string Msg = "I am the bone of my sword";

            /*Slanje i primanje poruke*/
            IM InstantMessaging = new IM();
            InstantMessaging.Send(alice.CurrentAliceDeriveKey, Msg, out SecretMsgToSend,out iv);
            
            string DecryptedMsg = InstantMessaging.Receive(bob.CurrentBobDeriveKey, SecretMsgToSend, iv);

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
        }
    }
}
