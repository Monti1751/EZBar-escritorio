using Microsoft.Win32;
using System.Windows;

namespace EZBarEscritorio.Services
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DialogService : IDialogService
    {
        public bool? ShowPagoDialog(decimal total, out string metodoPago, out decimal montoEntregado, out decimal cambio)
        {
            var dialog = new PagoWindow(total);
            bool? result = dialog.ShowDialog();
            
            metodoPago = dialog.MetodoPagoSeleccionado;
            montoEntregado = dialog.MontoEntregado;
            cambio = dialog.Cambio;
            
            return result;
        }

        public string? ShowSaveFileDialog(string defaultFileName, string filter)
        {
            var dialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                DefaultExt = ".xlsx",
                Filter = filter
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public void ShowMessage(string message, string title, bool isError = false)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, isError ? MessageBoxImage.Error : MessageBoxImage.Information);
        }
    }
}
