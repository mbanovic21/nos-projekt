using DigitalSignatureApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Infrastructure.Services
{
    public class HashService : IHashService
    {
        public byte[] ComputeHash(string filePath)
        {
            using var sha = SHA256.Create();
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return sha.ComputeHash(fs);
        }

        public void SaveHashToFile(string filePath, string hashFilePath)
        {
            var hash = ComputeHash(filePath);
            var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            File.WriteAllText(hashFilePath, hex);
        }
    }
}
