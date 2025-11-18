using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Application.Interfaces;
using DigitalSignatureApp.Infrastructure.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public partial class AesEncryptionViewModel : ObservableObject
    {
        private readonly IAesService _aesService;

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

        private readonly string _keysFolder;
        private readonly string _keyFilePath;
        private readonly string _ivFilePath;

        private bool HasKeyAndIv => _key is { Length: > 0 } && _iv is { Length: > 0 };

        public AesEncryptionViewModel()
        {
            _aesService = new AesService();

            _keysFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DigitalSignatureApp", "keys");

            Directory.CreateDirectory(_keysFolder);

            _keyFilePath = Path.Combine(_keysFolder, "aes_key.bin");
            _ivFilePath = Path.Combine(_keysFolder, "aes_iv.bin");

            AppendLog("AES module initialized.");
        }

        // -------------------- File selection --------------------

        [RelayCommand]
        private void SelectInputFile()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != true)
                return;

            InputFilePath = dlg.FileName;
            AppendLog($"Odabrana ulazna datoteka: {InputFilePath}");

            string ext = Path.GetExtension(InputFilePath)?.ToLower() ?? string.Empty;
            string outputFolder = Path.GetDirectoryName(InputFilePath)!;
            string baseName = Path.GetFileNameWithoutExtension(InputFilePath);

            if (ext == ".enc")
            {
                string originalExtension = Path.GetExtension(baseName);
                string cleanBaseName = Path.GetFileNameWithoutExtension(baseName);
                string newExtension = string.IsNullOrEmpty(originalExtension) ? ".bin" : originalExtension;

                OutputFilePath = Path.Combine(outputFolder, $"{cleanBaseName}_decrypted{newExtension}");
                AppendLog($"Automatski postavljena izlazna datoteka (za dekripciju): {OutputFilePath}");
            } else
            {
                OutputFilePath = InputFilePath + ".enc";
                AppendLog($"Automatski postavljena izlazna datoteka (za enkripciju): {OutputFilePath}");
            }

            UpdateCanExecute();
        }

        [RelayCommand]
        private void SelectOutputFile()
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Encrypted files (*.enc)|*.enc|All files (*.*)|*.*",
                FileName = Path.GetFileName(OutputFilePath)
            };

            if (dlg.ShowDialog() != true)
                return;

            OutputFilePath = dlg.FileName;
            AppendLog($"Selected output file: {OutputFilePath}");

            UpdateCanExecute();
        }

        // -------------------- Key management --------------------

        [RelayCommand]
        private void GenerateKey()
        {
            var (key, iv) = _aesService.GenerateKey();

            if (key == null || iv == null)
            {
                StatusMessage = "Greška: ključ nije generiran.";
                AppendLog(StatusMessage);
                return;
            }

            SetKeyAndIv(key, iv);

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

        private bool CanSaveKeys() => HasKeyAndIv;

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

                var key = File.ReadAllBytes(_keyFilePath);
                var iv = File.ReadAllBytes(_ivFilePath);

                SetKeyAndIv(key, iv);

                StatusMessage = "Ključ i IV učitani iz datoteka.";
                AppendLog($"Loaded key from {_keyFilePath}");

                UpdateCanExecute();
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri učitavanju ključa: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        // -------------------- Encrypt / Decrypt --------------------

        [RelayCommand(CanExecute = nameof(CanEncrypt))]
        private void EncryptFile()
        {
            try
            {
                if (!File.Exists(InputFilePath))
                {
                    StatusMessage = "Ulazna datoteka ne postoji.";
                    AppendLog(StatusMessage);
                    return;
                }

                var output = string.IsNullOrEmpty(OutputFilePath)
                    ? InputFilePath + ".enc"
                    : OutputFilePath;

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
            HasKeyAndIv;

        [RelayCommand(CanExecute = nameof(CanDecrypt))]
        private void DecryptFile()
        {
            try
            {
                if (!File.Exists(InputFilePath))
                {
                    StatusMessage = "Ulazna datoteka (kriptirana) ne postoji.";
                    AppendLog(StatusMessage);
                    return;
                }

                string outputFolder = Path.GetDirectoryName(InputFilePath)!;

                string outputPath = _aesService.DecryptFile(InputFilePath, outputFolder, _key, _iv);

                StatusMessage = $"Datoteka dekriptirana i spremljena u: {outputPath}";
                AppendLog($"Decrypt: {InputFilePath} -> {outputPath}");
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri dekripciji: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        private bool CanDecrypt() =>
            !string.IsNullOrEmpty(InputFilePath) &&
            File.Exists(InputFilePath) &&
            HasKeyAndIv;

        // -------------------- Copy commands --------------------

        [RelayCommand(CanExecute = nameof(CanCopyKeyAndIv))]
        private void CopyKeyBase64()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AesKeyBase64))
                {
                    StatusMessage = "Nema dostupnog AES ključa za kopiranje.";
                    AppendLog(StatusMessage);
                    return;
                }

                Clipboard.SetText(AesKeyBase64);
                StatusMessage = "AES ključ (Base64) kopiran u međuspremnik.";
                AppendLog(StatusMessage);
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri kopiranju ključa: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        [RelayCommand(CanExecute = nameof(CanCopyKeyAndIv))]
        private void CopyIvBase64()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AesIvBase64))
                {
                    StatusMessage = "Nema dostupnog AES IV za kopiranje.";
                    AppendLog(StatusMessage);
                    return;
                }

                Clipboard.SetText(AesIvBase64);
                StatusMessage = "AES IV (Base64) kopiran u međuspremnik.";
                AppendLog(StatusMessage);
            } catch (Exception ex)
            {
                StatusMessage = $"Greška pri kopiranju IV-a: {ex.Message}";
                AppendLog(StatusMessage);
            }
        }

        private bool CanCopyKeyAndIv() => HasKeyAndIv;

        // -------------------- Observable callbacks --------------------

        partial void OnShowHexChanged(bool value)
        {
            AppendLog($"ShowHex set to: {value}");
        }

        // -------------------- Helpers --------------------

        private void SetKeyAndIv(byte[] key, byte[] iv)
        {
            _key = key;
            _iv = iv;

            AesKeyBase64 = Convert.ToBase64String(_key);
            AesIvBase64 = Convert.ToBase64String(_iv);

            AesKeyHex = BitConverter.ToString(_key).Replace("-", string.Empty);
            AesIvHex = BitConverter.ToString(_iv).Replace("-", string.Empty);
        }

        private void UpdateCanExecute()
        {
            EncryptFileCommand?.NotifyCanExecuteChanged();
            DecryptFileCommand?.NotifyCanExecuteChanged();
            SaveKeyFilesCommand?.NotifyCanExecuteChanged();
            CopyKeyBase64Command?.NotifyCanExecuteChanged();
            CopyIvBase64Command?.NotifyCanExecuteChanged();
        }

        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }
    }
}
