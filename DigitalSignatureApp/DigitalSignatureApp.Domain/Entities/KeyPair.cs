using DigitalSignatureApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Domain.Entities
{
    public class KeyPair
    {
        public IKey PublicKey { get; set; }
        public IKey PrivateKey { get; set; }

        public KeyPair(IKey publicKey, IKey privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }
    }
}
