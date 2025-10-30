using DigitalSignatureApp.Application.Interfaces;
using DigitalSignatureApp.Domain.Entities;
using DigitalSignatureApp.Domain.Enums;
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
        private readonly string keysFolder;
        private readonly string publicKeyPath;
        private readonly string privateKeyPath;

        public RsaKeyService()
        {
            keysFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DigitalSignatureApp", "keys");

            Directory.CreateDirectory(keysFolder);

            publicKeyPath = Path.Combine(keysFolder, "javni_kljuc.txt");
            privateKeyPath = Path.Combine(keysFolder, "privatni_kljuc.txt");
        }

        public KeyPair GenerateKeyPair(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);
            
            var publicKeyValue = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            IKey publicKey = new PublicKey(publicKeyValue);
            Console.WriteLine("Generated Public Key: " + publicKeyValue);
            Console.WriteLine("Generated Public Key Path" + publicKey.KeyPath);

            var privateKeyValue = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
            IKey privateKey = new PrivateKey(privateKeyValue);

            return new KeyPair(publicKey, privateKey);
        }

        public void SaveKeysToFiles(KeyPair keyPair)
        {
            File.WriteAllText(publicKeyPath, keyPair.PublicKey.KeyValue);
            File.WriteAllText(privateKeyPath, keyPair.PrivateKey.KeyValue);

            keyPair.PublicKey.KeyPath = publicKeyPath;
            keyPair.PrivateKey.KeyPath = privateKeyPath;
        }

        public KeyPair LoadKeysFromFiles()
        {
            return LoadKeysFromFiles(publicKeyPath, privateKeyPath);
        }

        private KeyPair LoadKeysFromFiles(string publicKeyPath, string privateKeyPath)
        {
            if (!File.Exists(publicKeyPath) || !File.Exists(privateKeyPath))
            {
                var newKeys = GenerateKeyPair();
                SaveKeysToFiles(newKeys);
                return newKeys;
            }

            var publicKeyValue = File.ReadAllText(publicKeyPath);
            var privateKeyValue = File.ReadAllText(privateKeyPath);

            var publicKey = new PublicKey(publicKeyValue) { KeyPath = publicKeyPath };
            var privateKey = new PrivateKey(privateKeyValue) { KeyPath = privateKeyPath };

            return new KeyPair(publicKey, privateKey);
        }
    }
}
