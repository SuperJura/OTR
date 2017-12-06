using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTR
{
    interface IOTRUser
    {
        void SetDeriveKey(byte[] publicKey);
        void GenerateNewKey();
        void GenerateNewKey(byte[] publicKey);
    }
}
