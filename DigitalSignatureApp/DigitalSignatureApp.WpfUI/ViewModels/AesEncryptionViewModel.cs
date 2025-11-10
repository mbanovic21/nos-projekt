using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Application.Interfaces;
using DigitalSignatureApp.Infrastructure.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public partial class AesEncryptionViewModel : ObservableObject
    {
        private readonly IAesService _aesService;

        // UI-bindable properties
        [ObservableProperty] private string inputFilePath = string.Empty;
        [ObservableProperty] private string outputFilePath = string.Empty;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private string aesKeyBase64 = string.Empty;
        [ObservableProperty] private string aesIvBase64 = string.Empty;
        [ObservableProperty] private string aesKeyHex = string.Empty;
        [ObservableProperty] private string aesIvHex = string.Empty;
        [ObservableProperty] private string log = string.Empty;
        [ObservableProperty] private bool showHex = false;

        private byte[] _key;
        private byte[] _iv;

        // Paths for saving keys (AppData)
        private readonly string _keysFolder;
        private readonly string _keyFilePath;
        private readonly string _ivFilePath;

        public AesEncryptionViewModel()
        {
            _aesService = new AesService();
            _keysFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DigitalSignatureApp", "keys");
            Directory.CreateDirectory(_keysFolder);
            _keyFilePath = Path.Combine(_keysFolder, "aes_key.bin");
            _ivFilePath = Path.Combine(_keysFolder, "aes_iv.bin");

            // Initialize log
            AppendLog("AES module initialized.");
        }

        // -------------------- Commands --------------------

        [RelayCommand]
        private void SelectInputFile()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                InputFilePath = dlg.FileName;
                AppendLog($"Selected input file: {InputFilePath}");
                // set default output if empty
                if (string.IsNullOrEmpty(OutputFilePath))
                    OutputFilePath = Path.ChangeExtension(InputFilePath, ".enc");
                UpdateCanExecute();
            }
        }

        [RelayCommand]
        private void SelectOutputFile()
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Encrypted files (*.enc)|*.enc|All files (*.*)|*.*",
                FileName = Path.GetFileName(OutputFilePath)
            };
            if (dlg.ShowDialog() == true)
            {
                OutputFilePath = dlg.FileName;
                AppendLog($"Selected output file: {OutputFilePath}");
                UpdateCanExecute();
            }
        }

        [RelayCommand]
        private void GenerateKey()
        {
            (_key, _iv) = _aesService.GenerateKey();
            if (_key == null || _iv == null)
            {
                StatusMessage = "Greška: ključ nije generiran.";
                AppendLog(StatusMessage);
                return;
            }

            AesKeyBase64 = Convert.ToBase64String(_key);
            AesIvBase64 = Convert.ToBase64String(_iv);

            AesKeyHex = BitConverter.ToString(_key).Replace("-", "");
            AesIvHex = BitConverter.ToString(_iv).Replace("-", "");

            StatusMessage = "Generirani AES ključ i IV.";
            AppendLog("Generated AES key and IV.");

            UpdateCanExecute();
        }

        [RelayCommand(CanExecute = nameof(CanSaveKeys))]
        private void SaveKeyFiles()
        {
            try
            {
                File.WriteAllBytes(_keyFilePath, _key);
                File.WriteAllBytes(_ivFilePath, _iv);
                StatusMessage = $"Ključ i IV spremljeni u {_keysFolder}";
                AppendLog($"Saved key -> {_keyFilePath}");
                AppendLog($"Saved iv  -> {_ivFilePath}");
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri spremanju ključa: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        private bool CanSaveKeys() => _key != null && _iv != null;

        [RelayCommand]
        private void LoadKeyFiles()
        {
            try
            {
                if (!File.Exists(_keyFilePath) || !File.Exists(_ivFilePath))
                {
                    StatusMessage = "Nisu pronađene spremljene datoteke ključa u AppData.";
                    AppendLog(StatusMessage);
                    return;
                }

                _key = File.ReadAllBytes(_keyFilePath);
                _iv = File.ReadAllBytes(_ivFilePath);

                AesKeyBase64 = Convert.ToBase64String(_key);
                AesIvBase64 = Convert.ToBase64String(_iv);
                AesKeyHex = BitConverter.ToString(_key).Replace("-", "");
                AesIvHex = BitConverter.ToString(_iv).Replace("-", "");

                StatusMessage = "Ključ i IV učitani iz datoteka.";
                AppendLog($"Loaded key from {_keyFilePath}");
                UpdateCanExecute();
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri učitavanju ključa: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        [RelayCommand(CanExecute = nameof(CanEncrypt))]
        private void EncryptFile()
        {
            try
            {
                if (string.IsNullOrEmpty(InputFilePath) || !File.Exists(InputFilePath))
                {
                    StatusMessage = "Ulazna datoteka ne postoji.";
                    AppendLog(StatusMessage);
                    return;
                }

                var output = string.IsNullOrEmpty(OutputFilePath) ? Path.ChangeExtension(InputFilePath, ".enc") : OutputFilePath;
                _aesService.EncryptFile(InputFilePath, output, _key, _iv);

                StatusMessage = $"Datoteka enkriptirana -> {output}";
                AppendLog($"Encrypt: {InputFilePath} -> {output} (key size: {_key.Length * 8} bits)");
                AppendLog($"Output size: {new FileInfo(output).Length} bytes");
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri enkripciji: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        private bool CanEncrypt() =>
            !string.IsNullOrEmpty(InputFilePath) &&
            File.Exists(InputFilePath) &&
            _key != null && _iv != null;

        [RelayCommand(CanExecute = nameof(CanDecrypt))]
        private void DecryptFile()
        {
            try
            {
                if (string.IsNullOrEmpty(InputFilePath) || !File.Exists(InputFilePath))
                {
                    StatusMessage = "Ulazna datoteka (kriptirani) ne postoji.";
                    AppendLog(StatusMessage);
                    return;
                }

                var output = string.IsNullOrEmpty(OutputFilePath) ? Path.ChangeExtension(InputFilePath, ".dec") : OutputFilePath;
                _aesService.DecryptFile(InputFilePath, output, _key, _iv);

                StatusMessage = $"Datoteka dekriptirana -> {output}";
                AppendLog($"Decrypt: {InputFilePath} -> {output}");
                AppendLog($"Output size: {new FileInfo(output).Length} bytes");
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri dekripciji: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        private bool CanDecrypt() =>
            !string.IsNullOrEmpty(InputFilePath) &&
            File.Exists(InputFilePath) &&
            _key != null && _iv != null;

        // Toggle display between Base64 and Hex (bound to UI checkbox)
        partial void OnShowHexChanged(bool value)
        {
            // when toggled, update status (UI bound properties already set)
            AppendLog($"ShowHex set to: {value}");
        }

        // Helper methods
        private void UpdateCanExecute()
        {
            (EncryptFileCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (DecryptFileCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (SaveKeyFilesCommand as IRelayCommand)?.NotifyCanExecuteChanged();
        }

        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }
    }
}
