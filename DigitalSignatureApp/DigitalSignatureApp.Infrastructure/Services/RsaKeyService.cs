using DigitalSignatureApp.Application.Interfaces;
using DigitalSignatureApp.Domain.Entities;
using System.Security.Cryptography;

public class RsaKeyService : IRsaKeyService
{
    private readonly string _keysFolder;
    private readonly string _publicKeyPath;
    private readonly string _privateKeyPath;

    public string PublicKeyPath => _publicKeyPath;
    public string PrivateKeyPath => _privateKeyPath;

    public RsaKeyService()
    {
        _keysFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DigitalSignatureApp", "keys");

        Directory.CreateDirectory(_keysFolder);

        _publicKeyPath = Path.Combine(_keysFolder, "rsa_public_key.txt");
        _privateKeyPath = Path.Combine(_keysFolder, "rsa_private_key.txt");
    }

    public KeyPair GenerateKeyPair(int keySize = 2048)
    {
        using var rsa = RSA.Create(keySize);

        var publicKeyValue = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
        var publicKey = new PublicKey(publicKeyValue) { KeyPath = _publicKeyPath };

        var privateKeyValue = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
        var privateKey = new PrivateKey(privateKeyValue) { KeyPath = _privateKeyPath };

        return new KeyPair(publicKey, privateKey);
    }

    public void SaveKeysToFiles(KeyPair keyPair)
    {
        File.WriteAllText(_publicKeyPath, keyPair.PublicKey.KeyValue);
        File.WriteAllText(_privateKeyPath, keyPair.PrivateKey.KeyValue);

        keyPair.PublicKey.KeyPath = _publicKeyPath;
        keyPair.PrivateKey.KeyPath = _privateKeyPath;
    }

    public KeyPair LoadKeysFromFiles()
    {
        if (!File.Exists(_publicKeyPath) || !File.Exists(_privateKeyPath))
        {
            var newKeys = GenerateKeyPair();
            SaveKeysToFiles(newKeys);
            return newKeys;
        }

        var publicKeyValue = File.ReadAllText(_publicKeyPath);
        var privateKeyValue = File.ReadAllText(_privateKeyPath);

        var publicKey = new PublicKey(publicKeyValue) { KeyPath = _publicKeyPath };
        var privateKey = new PrivateKey(privateKeyValue) { KeyPath = _privateKeyPath };

        return new KeyPair(publicKey, privateKey);
    }
}
