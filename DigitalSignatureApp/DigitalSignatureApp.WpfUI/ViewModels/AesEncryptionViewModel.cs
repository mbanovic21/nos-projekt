using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Application.Interfaces;
using DigitalSignatureApp.Infrastructure.Services;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public class AesEncryptionViewModel : INotifyPropertyChanged
    {
        private readonly IAesService _aesService;
        private string _inputFilePath;
        private string _outputFilePath;
        private byte[] _key;
        private byte[] _iv;
        private bool _isEncryptMode = true;

        public string InputFilePath
        {
            get => _inputFilePath;
            set
            {
                _inputFilePath = value;
                OnPropertyChanged();
                RefreshCommands();
            }
        }

        public string OutputFilePath
        {
            get => _outputFilePath;
            set
            {
                _outputFilePath = value;
                OnPropertyChanged();
                RefreshCommands();
            }
        }

        public bool IsEncryptMode
        {
            get => _isEncryptMode;
            set
            {
                _isEncryptMode = value;
                OnPropertyChanged();
                RefreshCommands();
            }
        }

        public IRelayCommand SelectInputFileCommand { get; }
        public IRelayCommand SelectOutputFileCommand { get; }
        public IRelayCommand GenerateKeyCommand { get; }
        public IRelayCommand SaveKeyCommand { get; }
        public IRelayCommand LoadKeyCommand { get; }
        public IRelayCommand ExecuteCryptoCommand { get; }

        public AesEncryptionViewModel()
        {
            _aesService = new AesService();

            SelectInputFileCommand = new RelayCommand(SelectInputFile);
            SelectOutputFileCommand = new RelayCommand(SelectOutputFile);
            GenerateKeyCommand = new RelayCommand(GenerateKey);
            SaveKeyCommand = new RelayCommand(SaveKeyToFiles, () => _key != null && _iv != null);
            LoadKeyCommand = new RelayCommand(LoadKeyFromFiles);
            ExecuteCryptoCommand = new RelayCommand(ExecuteCrypto, CanExecuteCrypto);
        }

        private void SelectInputFile()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                InputFilePath = dialog.FileName;
                OutputFilePath = IsEncryptMode
                    ? Path.ChangeExtension(InputFilePath, ".enc")
                    : Path.ChangeExtension(InputFilePath, ".dec.txt");
            }
        }

        private void SelectOutputFile()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                OutputFilePath = dialog.FileName;
            }
        }

        private void GenerateKey()
        {
            (_key, _iv) = _aesService.GenerateKey();
            MessageBox.Show("AES ključ i IV uspješno generirani!", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshCommands();
        }

        private void SaveKeyToFiles()
        {
            try
            {
                File.WriteAllBytes("aes_key.bin", _key);
                File.WriteAllBytes("aes_iv.bin", _iv);
                MessageBox.Show("AES ključ i IV spremljeni!", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex)
            {
                MessageBox.Show($"Greška pri spremanju ključa: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadKeyFromFiles()
        {
            try
            {
                _key = File.ReadAllBytes("aes_key.bin");
                _iv = File.ReadAllBytes("aes_iv.bin");
                MessageBox.Show("AES ključ i IV učitani!", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshCommands();
            } catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju ključa: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteCrypto()
        {
            return !string.IsNullOrEmpty(InputFilePath)
                   && !string.IsNullOrEmpty(OutputFilePath)
                   && _key != null && _iv != null;
        }

        private void ExecuteCrypto()
        {
            try
            {
                if (IsEncryptMode)
                {
                    _aesService.EncryptFile(InputFilePath, OutputFilePath, _key, _iv);
                    MessageBox.Show($"Datoteka enkriptirana!\nPutanja: {OutputFilePath}", "AES", MessageBoxButton.OK, MessageBoxImage.Information);
                } else
                {
                    _aesService.DecryptFile(InputFilePath, OutputFilePath, _key, _iv);
                    MessageBox.Show($"Datoteka dekriptirana!\nPutanja: {OutputFilePath}", "AES", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            } catch (Exception ex)
            {
                MessageBox.Show($"Greška: {ex.Message}", "AES", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshCommands()
        {
            ExecuteCryptoCommand.NotifyCanExecuteChanged();
            SaveKeyCommand.NotifyCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
