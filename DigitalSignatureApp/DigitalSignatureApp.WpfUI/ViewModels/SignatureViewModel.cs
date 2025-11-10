using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Application.Interfaces;
using DigitalSignatureApp.Infrastructure.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public partial class SignatureViewModel : ObservableObject
    {
        private readonly IRsaKeyService _rsaService;

        [ObservableProperty] private string inputFilePath = string.Empty;
        [ObservableProperty] private string signatureValue = string.Empty;
        [ObservableProperty] private string log = string.Empty;

        public SignatureViewModel()
        {
            _rsaService = new RsaKeyService(); // inject if needed
            AppendLog("Digital Signature module initialized.");
        }

        [RelayCommand]
        private void SelectInputFile()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                InputFilePath = dlg.FileName;
                AppendLog($"Selected file: {InputFilePath}");
            }
        }

        [RelayCommand(CanExecute = nameof(CanSign))]
        private async Task SignFileAsync()
        {
            if (!File.Exists(InputFilePath))
                return;

            AppendLog($"Starting digital signature computation for {InputFilePath}");
            try
            {
                await Task.Run(() =>
                {
                    var keyPair = _rsaService.LoadKeysFromFiles();
                    var data = File.ReadAllBytes(InputFilePath);

                    using var rsa = RSA.Create();
                    rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(keyPair.PrivateKey.KeyValue), out _);

                    var signatureBytes = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    SignatureValue = Convert.ToBase64String(signatureBytes);

                    // Save signature to file
                    var signatureFile = Path.ChangeExtension(InputFilePath, ".sig");
                    File.WriteAllBytes(signatureFile, signatureBytes);

                    AppendLog($"Signature saved: {signatureFile}");
                });
            } catch (Exception ex)
            {
                AppendLog($"Error computing signature: {ex.Message}");
            }
        }

        [RelayCommand(CanExecute = nameof(CanVerify))]
        private async Task VerifySignatureAsync()
        {
            if (!File.Exists(InputFilePath))
                return;

            AppendLog($"Starting signature verification for {InputFilePath}");
            try
            {
                await Task.Run(() =>
                {
                    var keyPair = _rsaService.LoadKeysFromFiles();
                    var data = File.ReadAllBytes(InputFilePath);
                    var signatureFile = Path.ChangeExtension(InputFilePath, ".sig");

                    if (!File.Exists(signatureFile))
                    {
                        AppendLog("Signature file not found!");
                        return;
                    }

                    var signatureBytes = File.ReadAllBytes(signatureFile);

                    using var rsa = RSA.Create();
                    rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(keyPair.PublicKey.KeyValue), out _);

                    var verified = rsa.VerifyData(data, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    AppendLog(verified ? "Signature is VALID ✅" : "Signature is INVALID ❌");
                });
            } catch (Exception ex)
            {
                AppendLog($"Error verifying signature: {ex.Message}");
            }
        }

        private bool CanSign() => !string.IsNullOrEmpty(InputFilePath) && File.Exists(InputFilePath);
        private bool CanVerify() => !string.IsNullOrEmpty(InputFilePath) && File.Exists(InputFilePath) && File.Exists(Path.ChangeExtension(InputFilePath, ".sig"));

        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }
    }
}
