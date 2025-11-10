using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Domain.Interfaces
{
    public interface IHashService
    {
        byte[] ComputeHash(string filePath);
        void SaveHashToFile(string filePath, string hashFilePath);
    }
}
