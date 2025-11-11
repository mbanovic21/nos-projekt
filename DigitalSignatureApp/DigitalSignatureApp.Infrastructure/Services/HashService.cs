using DigitalSignatureApp.Domain.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Infrastructure.Services
{
    public class HashService : IHashService
    {
        public async Task<string> ComputeHashAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Datoteka ne postoji.", filePath);

            return await Task.Run(() =>
            {
                using var sha = SHA256.Create();
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var hash = sha.ComputeHash(fs);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            });
        }

        public async Task SaveHashToFileAsync(string filePath, string hashFilePath)
        {
            var hash = await ComputeHashAsync(filePath);
            await File.WriteAllTextAsync(hashFilePath, hash);
        }
    }
}
