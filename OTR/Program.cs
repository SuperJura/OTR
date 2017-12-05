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
            byte[] KeyForSigning;
            IM InstantMessaging = new IM();
            KeyForSigning = Encoding.UTF8.GetBytes("KeyForSigning");
            InstantMessaging.Send(alice.AliceDeriveKey, "Hello", out SecretMsgToSend,out iv);
            //MAC.SignFile(KeyForSigning, "", "");
            string DecryptedMsg = InstantMessaging.Receive(bob.BobDeriveKey, SecretMsgToSend, iv);

        }
    }
}
