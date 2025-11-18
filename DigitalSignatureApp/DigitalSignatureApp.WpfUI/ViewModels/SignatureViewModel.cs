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

        private bool HasInputFile =>
            !string.IsNullOrWhiteSpace(InputFilePath) &&
            File.Exists(InputFilePath);

        public SignatureViewModel()
            : this(new RsaKeyService(), new SignatureService())
        {
        }

        // DI-friendly konstruktor
        public SignatureViewModel(IRsaKeyService rsaService, ISignatureService signatureService)
        {
            _rsaService = rsaService ?? throw new ArgumentNullException(nameof(rsaService));
            _signatureService = signatureService ?? throw new ArgumentNullException(nameof(signatureService));

            AppendLog("Digital Signature module initialized.");
        }

        // ---------------- Commands ----------------

        [RelayCommand]
        private void SelectInputFile()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != true)
                return;

            InputFilePath = dlg.FileName;
            AppendLog($"Selected file: {InputFilePath}");
        }

        [RelayCommand(CanExecute = nameof(CanSign))]
        private async Task SignFileAsync()
        {
            if (!EnsureInputFileExists())
                return;

            await RunSafeAsync(async () =>
            {
                AppendLog($"Starting digital signature computation for {InputFilePath}");

                var keyPair = await Task.Run(() => _rsaService.LoadKeysFromFiles());

                var signatureBytes = await Task.Run(() =>
                    _signatureService.SignData(InputFilePath, keyPair.PrivateKey.KeyValue)
                );

                SignatureValue = Convert.ToBase64String(signatureBytes);

                var signatureFile = GetSignatureFilePath();
                await Task.Run(() => _signatureService.SaveSignatureToFile(signatureBytes, signatureFile));

                AppendLog($"Signature saved: {signatureFile}");
            });
        }

        [RelayCommand(CanExecute = nameof(CanVerify))]
        private async Task VerifySignatureAsync()
        {
            if (!EnsureInputFileExists())
                return;

            await RunSafeAsync(async () =>
            {
                AppendLog($"Starting signature verification for {InputFilePath}");

                var keyPair = await Task.Run(() => _rsaService.LoadKeysFromFiles());
                var signatureFile = GetSignatureFilePath();

                if (!File.Exists(signatureFile))
                {
                    AppendLog("Signature file not found!");
                    return;
                }

                var signatureBytes = await Task.Run(() =>
                    _signatureService.LoadSignatureFromFile(signatureFile)
                );

                var verified = await Task.Run(() =>
                    _signatureService.VerifySignature(InputFilePath, signatureBytes, keyPair.PublicKey.KeyValue)
                );

                AppendLog(verified ? "Signature is VALID ✅" : "Signature is INVALID ❌");
            });
        }

        // ---------------- CanExecute ----------------

        private bool CanSign() => HasInputFile;

        private bool CanVerify() =>
            HasInputFile &&
            File.Exists(GetSignatureFilePath());

        // ---------------- Observable callbacks ----------------

        partial void OnInputFilePathChanged(string value) => UpdateCanExecute();

        partial void OnSignatureValueChanged(string value) => UpdateCanExecute();

        // ---------------- Helpers ----------------

        private string GetSignatureFilePath() =>
            string.IsNullOrWhiteSpace(InputFilePath)
                ? string.Empty
                : Path.ChangeExtension(InputFilePath, ".sig");

        private bool EnsureInputFileExists()
        {
            if (HasInputFile)
                return true;

            AppendLog("Selected file does not exist.");
            return false;
        }

        private void UpdateCanExecute()
        {
            SignFileCommand?.NotifyCanExecuteChanged();
            VerifySignatureCommand?.NotifyCanExecuteChanged();
        }

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
