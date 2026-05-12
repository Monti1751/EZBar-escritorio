using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;
using EZBarEscritorio.Infrastructure.Network;

namespace EZBarEscritorio.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthInterceptor _authInterceptor;
        private readonly IApiService _apiService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isPasswordVisible;

        [ObservableProperty]
        private bool _showApiUrl;

        [ObservableProperty]
        private string _apiUrl = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public event Action? OnLoginSuccess;

        public LoginViewModel(AuthInterceptor authInterceptor, IApiService apiService, string defaultUrl)
        {
            _authInterceptor = authInterceptor;
            _apiService = apiService;
            ApiUrl = defaultUrl;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Usuario y contraseña requeridos.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Aplicar la URL que el usuario haya escrito en el campo de texto
                if (!string.IsNullOrWhiteSpace(ApiUrl))
                {
                    _apiService.SetBaseUrl(ApiUrl);
                }

                // Configurar las credenciales en el interceptor
                _authInterceptor.SetCredentials(Username, Password);

                // Intentar una llamada de prueba para verificar credenciales
                // En este caso, intentamos obtener pagos para ver si el 401 salta
                await _apiService.GetAsync<object>("/api/pagos");
                
                OnLoginSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                _authInterceptor.SetCredentials(string.Empty, string.Empty); // Limpiar si falla
                ErrorMessage = "Error de autenticación o conexión. Verifique sus credenciales.";
                // Si queremos más detalle: ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
