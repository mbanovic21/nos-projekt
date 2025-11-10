using DigitalSignatureApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Infrastructure.Services
{
    public class SignatureService : ISignatureService
    {
        public byte[] SignData(string filePath, string privateKeyPemBase64)
        {
            var dataHash = ComputeHash(filePath);

            var privateKeyBytes = Convert.FromBase64String(privateKeyPemBase64);
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

            return rsa.SignHash(dataHash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public bool VerifySignature(string filePath, byte[] signature, string publicKeyPemBase64)
        {
            var dataHash = ComputeHash(filePath);
            var publicKeyBytes = Convert.FromBase64String(publicKeyPemBase64);
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

            return rsa.VerifyHash(dataHash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public void SaveSignatureToFile(byte[] signature, string signatureFilePath)
        {
            File.WriteAllText(signatureFilePath, Convert.ToBase64String(signature));
        }

        public byte[] LoadSignatureFromFile(string signatureFilePath)
        {
            var b64 = File.ReadAllText(signatureFilePath);
            return Convert.FromBase64String(b64);
        }

        private byte[] ComputeHash(string filePath)
        {
            using var sha = SHA256.Create();
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return sha.ComputeHash(fs);
        }
    }
}
