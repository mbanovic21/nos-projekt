using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Infrastructure.Services;
using DigitalSignatureApp.Domain.Entities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using DigitalSignatureApp.Application.Interfaces;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public partial class RsaKeyViewModel : ObservableObject
    {
        private readonly IRsaKeyService _rsaService;

        // UI-bindable properties
        [ObservableProperty] private string publicKeyText = string.Empty;
        [ObservableProperty] private string privateKeyText = string.Empty;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private string log = string.Empty;

        private KeyPair _currentKeyPair;

        // Paths for storing keys
        private readonly string _keysFolder;
        private readonly string _publicKeyPath;
        private readonly string _privateKeyPath;

        public RsaKeyViewModel()
        {
            _rsaService = new RsaKeyService();
            _keysFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                        "DigitalSignatureApp", "keys");
            Directory.CreateDirectory(_keysFolder);
            _publicKeyPath = Path.Combine(_keysFolder, "rsa_public_key.txt");
            _privateKeyPath = Path.Combine(_keysFolder, "rsa_private_key.txt");

            AppendLog("RSA module initialized.");
        }

        // ---------------- Commands ----------------

        [RelayCommand]
        private void GenerateKeys()
        {
            _currentKeyPair = _rsaService.GenerateKeyPair();

            PublicKeyText = _currentKeyPair.PublicKey.KeyValue;
            PrivateKeyText = _currentKeyPair.PrivateKey.KeyValue;

            StatusMessage = "RSA ključevi generirani.";
            AppendLog("Generated RSA key pair.");
        }

        [RelayCommand(CanExecute = nameof(CanSaveKeys))]
        private void SaveKeys()
        {
            try
            {
                _rsaService.SaveKeysToFiles(_currentKeyPair);
                StatusMessage = "RSA ključevi spremljeni u AppData.";
                AppendLog($"Saved public key -> {_publicKeyPath}");
                AppendLog($"Saved private key -> {_privateKeyPath}");
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri spremanju ključeva: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        private bool CanSaveKeys() => _currentKeyPair != null;

        [RelayCommand]
        private void LoadKeys()
        {
            try
            {
                if (!File.Exists(_publicKeyPath) || !File.Exists(_privateKeyPath))
                {
                    StatusMessage = "Nisu pronađeni spremljeni RSA ključevi.";
                    AppendLog(StatusMessage);
                    return;
                }

                _currentKeyPair = _rsaService.LoadKeysFromFiles();
                PublicKeyText = _currentKeyPair.PublicKey.KeyValue;
                PrivateKeyText = _currentKeyPair.PrivateKey.KeyValue;

                StatusMessage = "RSA ključevi učitani iz datoteka.";
                AppendLog("Loaded RSA keys from files.");
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri učitavanju ključeva: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        [RelayCommand]
        private void CopyPublicKey()
        {
            if (!string.IsNullOrEmpty(PublicKeyText))
            {
                Clipboard.SetText(PublicKeyText);
                AppendLog("Public key copied to clipboard.");
            }
        }

        [RelayCommand]
        private void CopyPrivateKey()
        {
            if (!string.IsNullOrEmpty(PrivateKeyText))
            {
                Clipboard.SetText(PrivateKeyText);
                AppendLog("Private key copied to clipboard.");
            }
        }

        // ---------------- Helper ----------------
        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }
    }
}
