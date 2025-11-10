using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Domain.Interfaces
{
    public interface ISignatureService
    {
        byte[] SignData(string filePath, string privateKeyPemBase64);
        bool VerifySignature(string filePath, byte[] signature, string publicKeyPemBase64);
        void SaveSignatureToFile(byte[] signature, string signatureFilePath);
        byte[] LoadSignatureFromFile(string signatureFilePath);
    }
}
