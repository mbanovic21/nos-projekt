using DigitalSignatureApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Application.Interfaces
{
    public interface IKeyService
    {
        KeyPair GenerateKeyPair(int keySize = 2048);
        void SaveKeysToFiles(KeyPair keyPair);
        KeyPair LoadKeysFromFiles(string publicKeyPath, string privateKeyPath);
    }
}
