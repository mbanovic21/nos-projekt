using DigitalSignatureApp.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

public class AesService : IAesService
{
    public (byte[] Key, byte[] IV) GenerateKey()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        Console.WriteLine("Generated AES Key: " + Convert.ToBase64String(aes.Key));
        Console.WriteLine("Generated AES IV: " + Convert.ToBase64String(aes.IV));

        return (aes.Key, aes.IV);
    }

    public void EncryptFile(string inputFilePath, string outputFilePath, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var inputFile = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
        using var outputFile = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        using var cryptoStream = new CryptoStream(outputFile, aes.CreateEncryptor(), CryptoStreamMode.Write);

        string originalExtension = Path.GetExtension(inputFilePath);
        var extBytes = Encoding.UTF8.GetBytes(originalExtension);
        outputFile.WriteByte((byte)extBytes.Length);
        outputFile.Write(extBytes, 0, extBytes.Length);

        inputFile.CopyTo(cryptoStream);
    }

    public string DecryptFile(string inputFilePath, string outputFolderPath, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var inputFile = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);

        int extLength = inputFile.ReadByte();
        var extBytes = new byte[extLength];
        inputFile.Read(extBytes, 0, extLength);
        string originalExtension = Encoding.UTF8.GetString(extBytes);

        string fileName = Path.GetFileName(inputFilePath);

        if (fileName.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
            fileName = fileName.Substring(0, fileName.Length - 4);

        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string outputPath = Path.Combine(outputFolderPath, $"{baseName}_decrypted{originalExtension}");

        using var cryptoStream = new CryptoStream(inputFile, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var outputFile = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

        cryptoStream.CopyTo(outputFile);

        return outputPath;
    }
}
