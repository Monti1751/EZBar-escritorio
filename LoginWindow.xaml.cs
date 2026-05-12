using System.Windows;
using System.Windows.Controls;
using EZBarEscritorio.ViewModels;

namespace EZBarEscritorio
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel && !viewModel.IsPasswordVisible)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void VerContrasena_Changed(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                // Si pasamos de visible a oculto, sincronizamos el PasswordBox
                if (!viewModel.IsPasswordVisible)
                {
                    PasswordInput.Password = viewModel.Password;
                }
            }
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
