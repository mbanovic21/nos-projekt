using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Application.Interfaces
{
    public interface IAesService
    {
        (byte[] Key, byte[] IV) GenerateKey();
        void EncryptFile(string inputFilePath, string outputFilePath, byte[] key, byte[] iv);
        string DecryptFile(string inputFilePath, string outputFolderPath, byte[] key, byte[] iv);
    }
}
