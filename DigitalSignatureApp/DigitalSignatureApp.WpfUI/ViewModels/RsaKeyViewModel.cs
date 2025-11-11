using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Infrastructure.Services;
using DigitalSignatureApp.Domain.Entities;
using System;
using System.IO;
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

        public RsaKeyViewModel()
        {
            _rsaService = new RsaKeyService();
            AppendLog("RSA module initialized.");
        }

        // ---------------- Commands ----------------

        [RelayCommand]
        private void GenerateKeys()
        {
            try
            {
                _currentKeyPair = _rsaService.GenerateKeyPair();
                PublicKeyText = _currentKeyPair.PublicKey.KeyValue;
                PrivateKeyText = _currentKeyPair.PrivateKey.KeyValue;

                StatusMessage = "RSA ključevi generirani.";
                AppendLog("Generated RSA key pair.");

                SaveKeysCommand.NotifyCanExecuteChanged();
                CopyPublicKeyCommand.NotifyCanExecuteChanged();
                CopyPrivateKeyCommand.NotifyCanExecuteChanged();
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
                await Task.Run(() => _rsaService.SaveKeysToFiles(_currentKeyPair));

                StatusMessage = "RSA ključevi spremljeni u AppData.";
                AppendLog($"Saved public key -> {_rsaService.PublicKeyPath}");
                AppendLog($"Saved private key -> {_rsaService.PrivateKeyPath}");

                SaveKeysCommand.NotifyCanExecuteChanged();
                CopyPublicKeyCommand.NotifyCanExecuteChanged();
                CopyPrivateKeyCommand.NotifyCanExecuteChanged();
            });
        }

        private bool CanSaveKeys() => _currentKeyPair != null;
        [RelayCommand]
        private async Task LoadKeysAsync()
        {
            await RunSafeAsync(async () =>
            {
                var keys = await Task.Run(() => _rsaService.LoadKeysFromFiles());
                _currentKeyPair = keys;

                PublicKeyText = _currentKeyPair.PublicKey.KeyValue;
                PrivateKeyText = _currentKeyPair.PrivateKey.KeyValue;

                StatusMessage = "RSA ključevi učitani iz datoteka.";
                AppendLog("Loaded RSA keys from files.");

                SaveKeysCommand.NotifyCanExecuteChanged();
                CopyPublicKeyCommand.NotifyCanExecuteChanged();
                CopyPrivateKeyCommand.NotifyCanExecuteChanged();
            });
        }


        [RelayCommand(CanExecute = nameof(CanCopyPublicKey))]
        private void CopyPublicKey() => TryCopyToClipboard(PublicKeyText, "Public key copied to clipboard.");

        [RelayCommand(CanExecute = nameof(CanCopyPrivateKey))]
        private void CopyPrivateKey() => TryCopyToClipboard(PrivateKeyText, "Private key copied to clipboard.");

        private bool CanCopyPublicKey() => !string.IsNullOrEmpty(PublicKeyText);
        private bool CanCopyPrivateKey() => !string.IsNullOrEmpty(PrivateKeyText);

        // ---------------- Helper methods ----------------
        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }

        private void TryCopyToClipboard(string text, string logMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
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
    }
}
