using System;

namespace EZBarEscritorio.Services
{
    public interface IDialogService
    {
        bool? ShowPagoDialog(decimal total, out string metodoPago, out decimal montoEntregado, out decimal cambio);
        string? ShowSaveFileDialog(string defaultFileName, string filter);
        void ShowMessage(string message, string title, bool isError = false);
    }
}
