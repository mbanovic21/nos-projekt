using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DigitalSignatureApp.WpfUI.ViewModels
{
    public partial class HashViewModel : ObservableObject
    {
        [ObservableProperty] private string inputFilePath = string.Empty;
        [ObservableProperty] private string hashValue = string.Empty;
        [ObservableProperty] private string log = string.Empty;

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
                HashValue = await Task.Run(() => ComputeSha256(InputFilePath));
                var duration = DateTime.Now - start;
                AppendLog($"SHA-256 hash izračunat: {HashValue} (Trajanje: {duration.TotalMilliseconds} ms)");

                CopyHashCommand.NotifyCanExecuteChanged();
            } catch (Exception ex)
            {
                AppendLog($"Greška pri izračunu hash-a: {ex.Message}");
            }
        }

        private bool CanComputeHash() => !string.IsNullOrEmpty(InputFilePath) && File.Exists(InputFilePath);
        private bool CanCopyHash() => !string.IsNullOrEmpty(HashValue);

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

        private string ComputeSha256(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "");
        }

        private void AppendLog(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {message}";
            Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
        }
    }
}
