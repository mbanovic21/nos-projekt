using DigitalSignatureApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Application.Interfaces
{
    public interface IRsaKeyService
    {
        KeyPair GenerateKeyPair(int keySize = 2048);
        void SaveKeysToFiles(KeyPair keyPair);
        KeyPair LoadKeysFromFiles();
    }
}
