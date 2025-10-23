using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Domain.Entities
{
    public class PrivateKey : Key
    {
        public PrivateKey() : base() { }
        public PrivateKey(string path, string value) : base(path, value) 
        {
            KeyType = Enums.KeyType.PrivateKey;
        }
    }
}
