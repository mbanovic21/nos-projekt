using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private UserControl _currentView;

        public IRelayCommand ShowAesCommand { get; }
        public IRelayCommand ShowRsaCommand { get; }
        public IRelayCommand ShowHashCommand { get; }
        public IRelayCommand ShowSignatureCommand { get; }

        public DashboardViewModel()
        {
            // Defaultni pogled
            CurrentView = new Views.AesEncryptionView();

            ShowAesCommand = new RelayCommand(() => CurrentView = new Views.AesEncryptionView());
            ShowRsaCommand = new RelayCommand(() => CurrentView = new Views.RsaKeyView());
            ShowHashCommand = new RelayCommand(() => CurrentView = new Views.HashView());
            ShowSignatureCommand = new RelayCommand(() => CurrentView = new Views.SignatureView());
        }
    }
}
