using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EZBarEscritorio;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (s, e) => {
            if (DataContext is EZBarEscritorio.ViewModels.MainViewModel vm) {
                try {
                    await vm.CargarDatosAsync();
                } catch {
                    // El error ya debería estar manejado dentro de CargarDatosAsync
                    // pero esto evita que burbujee si algo falla catastróficamente.
                }
            }
        };
    }
}