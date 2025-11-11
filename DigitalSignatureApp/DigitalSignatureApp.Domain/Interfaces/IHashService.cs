using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Domain.Interfaces
{
    public interface IHashService
    {
        Task<string> ComputeHashAsync(string filePath);
        Task SaveHashToFileAsync(string filePath, string hashFilePath);
    }
}
