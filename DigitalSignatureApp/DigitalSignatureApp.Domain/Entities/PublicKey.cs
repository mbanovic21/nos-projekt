using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Domain.Entities
{
    public class PublicKey : Key
    {
        public PublicKey(string value) : base(value) 
        {
            KeyPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DigitalSignatureApp", "keys", "privatni_kljuc.txt");
            KeyType = Enums.KeyType.PublicKey;
        }
    }
}
