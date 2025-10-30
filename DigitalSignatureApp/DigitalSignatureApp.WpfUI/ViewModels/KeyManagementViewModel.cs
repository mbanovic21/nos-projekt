using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Domain.Entities;
using DigitalSignatureApp.Infrastructure.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public class KeyManagementViewModel : INotifyPropertyChanged
    {
        private readonly RsaKeyService _rsaKeyService;
        private KeyPair _currentKeyPair;

        public string PublicKeyValue
        {
            get => _currentKeyPair?.PublicKey?.KeyValue ?? string.Empty;
            set
            {
                if (_currentKeyPair?.PublicKey != null)
                {
                    _currentKeyPair.PublicKey.KeyValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PrivateKeyValue
        {
            get => _currentKeyPair?.PrivateKey?.KeyValue ?? string.Empty;
            set
            {
                if (_currentKeyPair?.PrivateKey != null)
                {
                    _currentKeyPair.PrivateKey.KeyValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusMessage { get; set; } = string.Empty;

        public ICommand GenerateKeysCommand { get; }
        public ICommand SaveKeysCommand { get; }
        public ICommand LoadKeysCommand { get; }

        public KeyManagementViewModel()
        {
            _rsaKeyService = new RsaKeyService();
            _currentKeyPair = _rsaKeyService.LoadKeysFromFiles();

            GenerateKeysCommand = new RelayCommand(GenerateKeys);
            SaveKeysCommand = new RelayCommand(SaveKeys, () => _currentKeyPair != null);
            LoadKeysCommand = new RelayCommand(LoadKeys);
        }

        private void GenerateKeys()
        {
            _currentKeyPair = _rsaKeyService.GenerateKeyPair();
            OnPropertyChanged(nameof(PublicKeyValue));
            OnPropertyChanged(nameof(PrivateKeyValue));
            StatusMessage = "✅ Ključevi su generirani!";
            OnPropertyChanged(nameof(StatusMessage));
        }

        private void SaveKeys()
        {
            try
            {
                _rsaKeyService.SaveKeysToFiles(_currentKeyPair);
                StatusMessage = "💾 Ključevi su uspješno spremljeni!";
                OnPropertyChanged(nameof(StatusMessage));
            } catch (System.Exception ex)
            {
                StatusMessage = $"❌ Greška pri spremanju ključeva: {ex.Message}";
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private void LoadKeys()
        {
            try
            {
                _currentKeyPair = _rsaKeyService.LoadKeysFromFiles();
                OnPropertyChanged(nameof(PublicKeyValue));
                OnPropertyChanged(nameof(PrivateKeyValue));
                StatusMessage = "📂 Ključevi su uspješno učitani!";
                OnPropertyChanged(nameof(StatusMessage));
            } catch (System.Exception ex)
            {
                StatusMessage = $"❌ Greška pri učitavanju ključeva: {ex.Message}";
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
