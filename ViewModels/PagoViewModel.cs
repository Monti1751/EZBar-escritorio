using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace EZBarEscritorio.ViewModels
{
    public partial class PagoViewModel : ObservableObject
    {
        private readonly decimal _totalPedido;

        [ObservableProperty]
        private string _metodoPago = "Efectivo";

        [ObservableProperty]
        private string _montoRecibidoStr = "";

        [ObservableProperty]
        private string _cambioTexto = "Cambio: ---";

        [ObservableProperty]
        private bool _esMontoValido;

        [ObservableProperty]
        private bool _mostrarEfectivo = true;

        public decimal MontoEntregado { get; private set; }
        public decimal Cambio { get; private set; }

        public PagoViewModel(decimal totalPedido)
        {
            _totalPedido = totalPedido;
            ValidarEfectivo();
        }

        partial void OnMetodoPagoChanged(string value)
        {
            MostrarEfectivo = (value == "Efectivo");
            if (!MostrarEfectivo)
            {
                EsMontoValido = true;
                MontoEntregado = _totalPedido;
                Cambio = 0;
            }
            else
            {
                ValidarEfectivo();
            }
        }

        partial void OnMontoRecibidoStrChanged(string value)
        {
            ValidarEfectivo();
        }

        private void ValidarEfectivo()
        {
            if (!MostrarEfectivo) return;

            string input = MontoRecibidoStr.Replace(".", ",");
            if (decimal.TryParse(input, out decimal recibido))
            {
                decimal cambio = recibido - _totalPedido;
                if (cambio >= 0)
                {
                    CambioTexto = $"Cambio a devolver: {cambio:C}";
                    EsMontoValido = true;
                    MontoEntregado = recibido;
                    Cambio = cambio;
                }
                else
                {
                    CambioTexto = "Cambio: Monto insuficiente";
                    EsMontoValido = false;
                }
            }
            else
            {
                CambioTexto = "Cambio: ---";
                EsMontoValido = false;
            }
        }
    }
}
