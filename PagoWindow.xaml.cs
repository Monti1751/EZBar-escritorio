using System.Windows;
using System.Windows.Controls;

namespace EZBarEscritorio
{
    public partial class PagoWindow : Window
    {
        public string MetodoPagoSeleccionado { get; private set; }
        public decimal MontoEntregado { get; private set; }
        public decimal Cambio { get; private set; }
        
        private decimal _totalPedido;

        public PagoWindow(decimal totalPedido)
        {
            InitializeComponent();
            _totalPedido = totalPedido;
            txtTotal.Text = $"Total a Pagar: {totalPedido:C}";
            
            // Forzar la validación inicial del botón Confirmar para Efectivo
            ValidarEfectivo();
        }

        private void cmbMetodoPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMetodoPago == null || panelEfectivo == null || btnConfirmar == null) return;

            if (cmbMetodoPago.SelectedIndex == 0) // Efectivo
            {
                panelEfectivo.Visibility = Visibility.Visible;
                ValidarEfectivo();
            }
            else // Tarjeta o Transferencia
            {
                panelEfectivo.Visibility = Visibility.Collapsed;
                btnConfirmar.IsEnabled = true; // No hay que calcular cambio, se asume cobro exacto por datáfono/transferencia
            }
        }

        private void txtMontoRecibido_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidarEfectivo();
        }

        private void ValidarEfectivo()
        {
            if (txtMontoRecibido == null || txtCambio == null || btnConfirmar == null) return;

            // Reemplazar coma por punto o viceversa según el locale para facilitar la conversión
            string input = txtMontoRecibido.Text.Replace(".", ",");

            if (decimal.TryParse(input, out decimal recibido))
            {
                decimal cambio = recibido - _totalPedido;
                if (cambio >= 0)
                {
                    txtCambio.Text = $"Cambio a devolver: {cambio:C}";
                    txtCambio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 125, 50)); // Verde
                    btnConfirmar.IsEnabled = true;
                }
                else
                {
                    txtCambio.Text = "Cambio: Monto insuficiente";
                    txtCambio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(211, 47, 47)); // Rojo
                    btnConfirmar.IsEnabled = false;
                }
            }
            else
            {
                txtCambio.Text = "Cambio: ---";
                txtCambio.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(62, 39, 35)); // Marrón oscuro
                btnConfirmar.IsEnabled = false;
            }
        }

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            MetodoPagoSeleccionado = ((ComboBoxItem)cmbMetodoPago.SelectedItem).Content.ToString().ToLower();
            
            if (panelEfectivo.Visibility == Visibility.Visible)
            {
                string input = txtMontoRecibido.Text.Replace(".", ",");
                if (decimal.TryParse(input, out decimal recibido))
                {
                    MontoEntregado = recibido;
                    Cambio = recibido - _totalPedido;
                }
            }
            else
            {
                MontoEntregado = _totalPedido;
                Cambio = 0;
            }

            DialogResult = true;
            Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
