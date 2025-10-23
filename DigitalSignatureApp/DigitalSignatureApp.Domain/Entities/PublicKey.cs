using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Domain.Entities
{
    public class PublicKey : Key
    {
        public PublicKey() : base() { }
        public PublicKey(string path, string value) : base(path, value) 
        {
            KeyType = Enums.KeyType.PublicKey;
        }
    }
}
