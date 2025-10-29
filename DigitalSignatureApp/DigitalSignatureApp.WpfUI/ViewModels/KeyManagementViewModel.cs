using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Domain.Entities;
using DigitalSignatureApp.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public class KeyManagementViewModel : INotifyPropertyChanged
    {
        private readonly RsaKeyService _rsaService = new RsaKeyService();

        private string _publicKeyPath = "javni_kljuc.txt";
        private string _privateKeyPath = "privatni_kljuc.txt";
        private string _statusMessage = string.Empty;
        private KeyPair? _currentKeyPair;

        public string PublicKeyPath
        {
            get => _publicKeyPath;
            set { _publicKeyPath = value; OnPropertyChanged(); }
        }

        public string PrivateKeyPath
        {
            get => _privateKeyPath;
            set { _privateKeyPath = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand GenerateKeysCommand { get; }
        public ICommand SaveKeysCommand { get; }
        public ICommand LoadKeysCommand { get; }

        public KeyManagementViewModel()
        {
            GenerateKeysCommand = new RelayCommand(GenerateKeys);
            SaveKeysCommand = new RelayCommand(SaveKeys, () => _currentKeyPair != null);
            LoadKeysCommand = new RelayCommand(LoadKeys);
        }

        private void GenerateKeys()
        {
            _currentKeyPair = _rsaService.GenerateKeyPair();
            StatusMessage = "Ključevi su uspješno generirani.";
        }

        private void SaveKeys()
        {
            if (_currentKeyPair == null)
            {
                StatusMessage = "Nema ključeva za spremanje.";
                return;
            }

            _rsaService.SaveKeysToFiles(_currentKeyPair, PublicKeyPath, PrivateKeyPath);
            StatusMessage = $"Ključevi spremljeni u:\n{PublicKeyPath}\n{PrivateKeyPath}";
        }

        private void LoadKeys()
        {
            _currentKeyPair = _rsaService.LoadKeysFromFiles(PublicKeyPath, PrivateKeyPath);
            StatusMessage = "Ključevi su uspješno učitani iz datoteka.";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
