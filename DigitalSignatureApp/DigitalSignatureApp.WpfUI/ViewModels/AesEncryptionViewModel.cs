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
            set { _inputFilePath = value; OnPropertyChanged(); }
        }

        public string OutputFilePath
        {
            get => _outputFilePath;
            set { _outputFilePath = value; OnPropertyChanged(); }
        }

        public ICommand SelectInputFileCommand { get; }
        public ICommand SelectOutputFileCommand { get; }
        public ICommand GenerateKeyCommand { get; }
        public ICommand EncryptCommand { get; }
        public ICommand DecryptCommand { get; }

        public AesEncryptionViewModel(IAesService aesService)
        {
            _aesService = aesService;

            SelectInputFileCommand = new RelayCommand(SelectInputFile);
            SelectOutputFileCommand = new RelayCommand(SelectOutputFile);
            GenerateKeyCommand = new RelayCommand(GenerateKey);
            EncryptCommand = new RelayCommand(EncryptFile, CanExecuteCrypto);
            DecryptCommand = new RelayCommand(DecryptFile, CanExecuteCrypto);
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
            MessageBox.Show("AES ključ i IV uspješno generirani!", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanExecuteCrypto() =>
            !string.IsNullOrEmpty(InputFilePath) &&
            !string.IsNullOrEmpty(OutputFilePath) &&
            _key != null && _iv != null;

        private void EncryptFile()
        {
            try
            {
                _aesService.EncryptFile(InputFilePath, OutputFilePath, _key, _iv);
                MessageBox.Show("Datoteka uspješno enkriptirana.", "AES Enkripcija", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex)
            {
                MessageBox.Show($"Greška pri enkripciji: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
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
}
