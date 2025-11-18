using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalSignatureApp.Domain.Interfaces;
using DigitalSignatureApp.Infrastructure.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public partial class HashViewModel : ObservableObject
    {
        private readonly IHashService _hashService;

        [ObservableProperty] private string inputFilePath = string.Empty;
        [ObservableProperty] private string hashValue = string.Empty;
        [ObservableProperty] private string log = string.Empty;

        public HashViewModel()
            : this(new HashService())
        {
        }

        // omogućava DI ako zatreba
        public HashViewModel(IHashService hashService)
        {
            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
            AppendLog("Hash module initialized.");
        }

        // -------------------- Commands --------------------

        [RelayCommand]
        private void SelectInputFile()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != true)
                return;

            InputFilePath = dlg.FileName;
            AppendLog($"Odabrana datoteka: {InputFilePath}");
        }

        [RelayCommand(CanExecute = nameof(CanComputeHash))]
        private async Task ComputeHashAsync()
        {
            if (!EnsureInputFileExists())
                return;

            AppendLog($"Početak izračuna SHA-256 za: {InputFilePath}");
            var start = DateTime.Now;

            try
            {
                HashValue = await _hashService.ComputeHashAsync(InputFilePath);
                var duration = DateTime.Now - start;
                AppendLog($"SHA-256 hash izračunat: {HashValue} (Trajanje: {duration.TotalMilliseconds} ms)");
            } catch (Exception ex)
            {
                AppendLog($"Greška pri izračunu hash-a: {ex.Message}");
            }
        }

        [RelayCommand(CanExecute = nameof(CanCopyHash))]
        private void CopyHash()
        {
            try
            {
                Clipboard.SetText(HashValue);
                AppendLog("Hash kopiran u clipboard.");
            } catch (Exception ex)
            {
                AppendLog($"Greška pri kopiranju hash-a: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveHashToFileAsync()
        {
            if (!EnsureInputFileExists())
                return;

            var hashFilePath = Path.ChangeExtension(InputFilePath, ".hash");

            try
            {
                await _hashService.SaveHashToFileAsync(InputFilePath, hashFilePath);
                AppendLog($"Hash spremljen u datoteku: {hashFilePath}");
            } catch (Exception ex)
            {
                AppendLog($"Greška pri spremanju hash-a: {ex.Message}");
            }
        }

        // -------------------- CanExecute --------------------

        private bool CanComputeHash() =>
            !string.IsNullOrWhiteSpace(InputFilePath) &&
            File.Exists(InputFilePath);

        private bool CanCopyHash() =>
            !string.IsNullOrWhiteSpace(HashValue);

        // -------------------- Observable callbacks --------------------

        partial void OnInputFilePathChanged(string value)
        {
            UpdateCanExecute();
        }

        partial void OnHashValueChanged(string value)
        {
            UpdateCanExecute();
        }

        // -------------------- Helpers --------------------

        private bool EnsureInputFileExists()
        {
            if (File.Exists(InputFilePath))
                return true;

            var message = "Odabrana datoteka ne postoji.";
            MessageBox.Show(message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            AppendLog(message);
            return false;
        }

        private void UpdateCanExecute()
        {
            ComputeHashCommand?.NotifyCanExecuteChanged();
            CopyHashCommand?.NotifyCanExecuteChanged();
        }

        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }
    }
}
