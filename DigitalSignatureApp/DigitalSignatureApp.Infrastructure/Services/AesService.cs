using DigitalSignatureApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Infrastructure.Services
{
    public class AesService : IAesService
    {
        public (byte[] Key, byte[] IV) GenerateKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            return (aes.Key, aes.IV);
        }

        public void EncryptFile(string inputFilePath, string outputFilePath, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var fileStream = new FileStream(outputFilePath, FileMode.Create);
            using var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var inputFile = new FileStream(inputFilePath, FileMode.Open);

            inputFile.CopyTo(cryptoStream);
        }

        public void DecryptFile(string inputFilePath, string outputFilePath, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var fileStream = new FileStream(outputFilePath, FileMode.Create);
            using var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Write);
            using var inputFile = new FileStream(inputFilePath, FileMode.Open);

            inputFile.CopyTo(cryptoStream);
        }
    }
}
