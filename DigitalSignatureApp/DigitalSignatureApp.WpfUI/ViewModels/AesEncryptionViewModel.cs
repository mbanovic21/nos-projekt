using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Application.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using DigitalSignatureApp.Infrastructure.Services;
using System.IO;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public class AesEncryptionViewModel : INotifyPropertyChanged
    {
        private readonly IAesService _aesService;
        private string _inputFilePath;
        private string _outputFilePath;
        private byte[] _key;
        private byte[] _iv;

        public string InputFilePath
        {
            get => _inputFilePath;
            set 
            { 
                _inputFilePath = value; 
                OnPropertyChanged();
                EncryptCommand.NotifyCanExecuteChanged();
                DecryptCommand.NotifyCanExecuteChanged();
            }
        }

        public string OutputFilePath
        {
            get => _outputFilePath;
            set 
            { 
                _outputFilePath = value; 
                OnPropertyChanged();
                EncryptCommand.NotifyCanExecuteChanged();
                DecryptCommand.NotifyCanExecuteChanged();
            }
        }

        public IRelayCommand SelectInputFileCommand { get; }
        public IRelayCommand SelectOutputFileCommand { get; }
        public IRelayCommand GenerateKeyCommand { get; }
        public IRelayCommand EncryptCommand { get; }
        public IRelayCommand DecryptCommand { get; }

        public AesEncryptionViewModel()
        {
            _aesService = new AesService();

            SelectInputFileCommand = new RelayCommand(SelectInputFile);
            SelectOutputFileCommand = new RelayCommand(SelectOutputFile);
            GenerateKeyCommand = new RelayCommand(GenerateKey);
            EncryptCommand = new RelayCommand(EncryptFile, CanExecuteEncrypt);
            DecryptCommand = new RelayCommand(DecryptFile, CanExecuteDecrypt);
        }

        private void SelectInputFile()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                InputFilePath = dialog.FileName;
            }
        }

        private void SelectOutputFile()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Encrypted files (*.enc)|*.enc|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                OutputFilePath = dialog.FileName;
            }
        }

        private void GenerateKey()
        {
            (_key, _iv) = _aesService.GenerateKey();
            if(_key != null && _iv != null)
                MessageBox.Show("AES ključ i IV uspješno generirani!", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);

            EncryptCommand.NotifyCanExecuteChanged();
            DecryptCommand.NotifyCanExecuteChanged();
        }

        private bool CanExecuteEncrypt() => 
            !string.IsNullOrEmpty(InputFilePath) && 
            _key != null && _iv != null;

        private bool CanExecuteDecrypt() =>
            !string.IsNullOrEmpty(InputFilePath) &&
            !string.IsNullOrEmpty(OutputFilePath) &&
            _key != null && _iv != null;

        private void EncryptFile()
        {
            try
            {
                var outputPath = Path.ChangeExtension(InputFilePath, ".enc");
                _aesService.EncryptFile(InputFilePath, outputPath, _key, _iv);

                MessageBox.Show($"Datoteka je uspješno enkriptirana!\nPutanja: {outputPath}",
                                "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex)
            {
                MessageBox.Show($"Greška pri enkripciji: {ex.Message}",
                                "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DecryptFile()
        {
            try
            {
                _aesService.DecryptFile(InputFilePath, OutputFilePath, _key, _iv);
                MessageBox.Show("Datoteka uspješno dekriptirana.", "AES Dekripcija", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex)
            {
                MessageBox.Show($"Greška pri dekripciji: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
