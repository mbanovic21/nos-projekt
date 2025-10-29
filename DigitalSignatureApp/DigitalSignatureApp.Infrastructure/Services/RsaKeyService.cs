using DigitalSignatureApp.Application.Interfaces;
using DigitalSignatureApp.Domain.Entities;
using DigitalSignatureApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Infrastructure.Services
{
    public class RsaKeyService : IKeyService
    {
        public KeyPair GenerateKeyPair(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);

            IKey publicKey = new PublicKey
            {
                KeyValue = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                CreatedAt = DateTime.UtcNow
            };

            IKey privateKey = new PrivateKey
            {
                KeyValue = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey()),
                CreatedAt = DateTime.UtcNow
            };

            return new KeyPair(publicKey, privateKey);
        }

        public KeyPair LoadKeysFromFiles(string publicKeyPath, string privateKeyPath)
        {
            var pub = new PublicKey
            {
                KeyPath = publicKeyPath,
                KeyValue = File.ReadAllText(publicKeyPath)
            };

            var priv = new PrivateKey
            {
                KeyPath = privateKeyPath,
                KeyValue = File.ReadAllText(privateKeyPath)
            };

            return new KeyPair(pub, priv);
        }

        public void SaveKeysToFiles(KeyPair keyPair, string publicKeyPath, string privateKeyPath)
        {
            File.WriteAllText(publicKeyPath, keyPair.PublicKey.KeyValue);
            File.WriteAllText(privateKeyPath, keyPair.PrivateKey.KeyValue);
        }
    }
}
