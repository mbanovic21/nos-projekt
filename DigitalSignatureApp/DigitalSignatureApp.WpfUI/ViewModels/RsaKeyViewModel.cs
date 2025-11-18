using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Infrastructure.Services;
using DigitalSignatureApp.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Windows;
using DigitalSignatureApp.Application.Interfaces;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public partial class RsaKeyViewModel : ObservableObject
    {
        private readonly IRsaKeyService _rsaService;

        [ObservableProperty] private string publicKeyText = string.Empty;
        [ObservableProperty] private string privateKeyText = string.Empty;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private string log = string.Empty;

        private KeyPair _currentKeyPair;
        private bool HasKeyPair => _currentKeyPair != null;

        public RsaKeyViewModel()
            : this(new RsaKeyService())
        {
        }

        // omogućuje DI/testiranje
        public RsaKeyViewModel(IRsaKeyService rsaService)
        {
            _rsaService = rsaService ?? throw new ArgumentNullException(nameof(rsaService));
            AppendLog("RSA module initialized.");
        }

        // ---------------- Commands ----------------

        [RelayCommand]
        private void GenerateKeys()
        {
            try
            {
                var keyPair = _rsaService.GenerateKeyPair();
                SetKeyPair(keyPair);

                StatusMessage = "RSA ključevi generirani.";
                AppendLog("Generated RSA key pair.");
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri generiranju ključeva: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        [RelayCommand(CanExecute = nameof(CanSaveKeys))]
        private async Task SaveKeysAsync()
        {
            await RunSafeAsync(async () =>
            {
                if (!HasKeyPair)
                    throw new InvalidOperationException("Nema generiranih/učitanih ključeva za spremanje.");

                await Task.Run(() => _rsaService.SaveKeysToFiles(_currentKeyPair));

                StatusMessage = "RSA ključevi spremljeni u AppData.";
                AppendLog($"Saved public key -> {_rsaService.PublicKeyPath}");
                AppendLog($"Saved private key -> {_rsaService.PrivateKeyPath}");
            });
        }

        private bool CanSaveKeys() => HasKeyPair;

        [RelayCommand]
        private async Task LoadKeysAsync()
        {
            await RunSafeAsync(async () =>
            {
                var keys = await Task.Run(() => _rsaService.LoadKeysFromFiles());
                SetKeyPair(keys);

                StatusMessage = "RSA ključevi učitani iz datoteka.";
                AppendLog("Loaded RSA keys from files.");
            });
        }

        [RelayCommand(CanExecute = nameof(CanCopyPublicKey))]
        private void CopyPublicKey() =>
            TryCopyToClipboard(PublicKeyText, "Public key copied to clipboard.");

        [RelayCommand(CanExecute = nameof(CanCopyPrivateKey))]
        private void CopyPrivateKey() =>
            TryCopyToClipboard(PrivateKeyText, "Private key copied to clipboard.");

        private bool CanCopyPublicKey() => !string.IsNullOrWhiteSpace(PublicKeyText);
        private bool CanCopyPrivateKey() => !string.IsNullOrWhiteSpace(PrivateKeyText);

        // ---------------- Observable callbacks ----------------

        partial void OnPublicKeyTextChanged(string value) => UpdateCanExecute();
        partial void OnPrivateKeyTextChanged(string value) => UpdateCanExecute();

        // ---------------- Helper methods ----------------

        private void SetKeyPair(KeyPair keyPair)
        {
            _currentKeyPair = keyPair ?? throw new ArgumentNullException(nameof(keyPair));

            PublicKeyText = _currentKeyPair.PublicKey?.KeyValue ?? string.Empty;
            PrivateKeyText = _currentKeyPair.PrivateKey?.KeyValue ?? string.Empty;

            UpdateCanExecute();
        }

        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }

        private void TryCopyToClipboard(string text, string logMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    Clipboard.SetText(text);
                    AppendLog(logMessage);
                }
            } catch (Exception ex)
            {
                AppendLog($"Greška pri kopiranju: {ex.Message}");
            }
        }

        private async Task RunSafeAsync(Func<Task> action)
        {
            try
            {
                await action();
            } catch (Exception ex)
            {
                StatusMessage = $"Greška: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        private void UpdateCanExecute()
        {
            SaveKeysCommand?.NotifyCanExecuteChanged();
            CopyPublicKeyCommand?.NotifyCanExecuteChanged();
            CopyPrivateKeyCommand?.NotifyCanExecuteChanged();
        }
    }
}
