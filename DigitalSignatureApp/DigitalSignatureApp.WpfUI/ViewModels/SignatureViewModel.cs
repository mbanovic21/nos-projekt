using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Application.Interfaces;
using DigitalSignatureApp.Domain.Interfaces;
using DigitalSignatureApp.Infrastructure.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public partial class SignatureViewModel : ObservableObject
    {
        private readonly IRsaKeyService _rsaService;
        private readonly ISignatureService _signatureService;

        [ObservableProperty] private string inputFilePath = string.Empty;
        [ObservableProperty] private string signatureValue = string.Empty;
        [ObservableProperty] private string log = string.Empty;

        public SignatureViewModel()
        {
            _rsaService = new RsaKeyService();
            _signatureService = new SignatureService();
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

                SignFileCommand.NotifyCanExecuteChanged();
                VerifySignatureCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand(CanExecute = nameof(CanSign))]
        private async Task SignFileAsync()
        {
            if (!File.Exists(InputFilePath))
                return;

            await RunSafeAsync(async () =>
            {
                AppendLog($"Starting digital signature computation for {InputFilePath}");

                var keyPair = await Task.Run(() => _rsaService.LoadKeysFromFiles());

                var signatureBytes = await Task.Run(() =>
                    _signatureService.SignData(InputFilePath, keyPair.PrivateKey.KeyValue)
                );

                SignatureValue = Convert.ToBase64String(signatureBytes);

                var signatureFile = Path.ChangeExtension(InputFilePath, ".sig");
                await Task.Run(() => _signatureService.SaveSignatureToFile(signatureBytes, signatureFile));

                AppendLog($"Signature saved: {signatureFile}");

                VerifySignatureCommand.NotifyCanExecuteChanged();
            });
        }

        [RelayCommand(CanExecute = nameof(CanVerify))]
        private async Task VerifySignatureAsync()
        {
            if (!File.Exists(InputFilePath))
                return;

            await RunSafeAsync(async () =>
            {
                AppendLog($"Starting signature verification for {InputFilePath}");

                var keyPair = await Task.Run(() => _rsaService.LoadKeysFromFiles());
                var signatureFile = Path.ChangeExtension(InputFilePath, ".sig");

                if (!File.Exists(signatureFile))
                {
                    AppendLog("Signature file not found!");
                    return;
                }

                var signatureBytes = await Task.Run(() => _signatureService.LoadSignatureFromFile(signatureFile));

                var verified = await Task.Run(() =>
                    _signatureService.VerifySignature(InputFilePath, signatureBytes, keyPair.PublicKey.KeyValue)
                );

                AppendLog(verified ? "Signature is VALID ✅" : "Signature is INVALID ❌");
            });
        }

        private bool CanSign() => !string.IsNullOrEmpty(InputFilePath) && File.Exists(InputFilePath);
        private bool CanVerify() => !string.IsNullOrEmpty(InputFilePath)
                                     && File.Exists(InputFilePath)
                                     && File.Exists(Path.ChangeExtension(InputFilePath, ".sig"));

        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }

        private async Task RunSafeAsync(Func<Task> action)
        {
            try
            {
                await action();
            } catch (Exception ex)
            {
                AppendLog($"Error: {ex.Message}");
            }
        }
    }
}
