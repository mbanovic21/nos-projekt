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
        {
            _hashService = new HashService(); // moze se i injectati
            AppendLog("Hash module initialized.");
        }

        [RelayCommand]
        private void SelectInputFile()
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                InputFilePath = dlg.FileName;
                AppendLog($"Odabrana datoteka: {InputFilePath}");

                ComputeHashCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand(CanExecute = nameof(CanComputeHash))]
        private async Task ComputeHashAsync()
        {
            if (!File.Exists(InputFilePath))
            {
                MessageBox.Show("Odabrana datoteka ne postoji.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AppendLog($"Početak izračuna SHA-256 za: {InputFilePath}");
            var start = DateTime.Now;

            try
            {
                HashValue = await _hashService.ComputeHashAsync(InputFilePath);
                var duration = DateTime.Now - start;
                AppendLog($"SHA-256 hash izračunat: {HashValue} (Trajanje: {duration.TotalMilliseconds} ms)");

                CopyHashCommand.NotifyCanExecuteChanged();
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

        private bool CanComputeHash() => !string.IsNullOrEmpty(InputFilePath) && File.Exists(InputFilePath);
        private bool CanCopyHash() => !string.IsNullOrEmpty(HashValue);

        [RelayCommand]
        private async Task SaveHashToFileAsync()
        {
            if (!File.Exists(InputFilePath))
            {
                MessageBox.Show("Odabrana datoteka ne postoji.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }
    }
}
