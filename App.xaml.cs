using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EZBarEscritorio.Infrastructure.Network;
using EZBarEscritorio.Repositories;
using EZBarEscritorio.Services;
using EZBarEscritorio.ViewModels;

namespace EZBarEscritorio
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }

        private void ConfigureServices(IServiceCollection services)
        {
            var baseUrl = Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:8080";

            // Interceptor como Singleton para que LoginViewModel y HttpClient compartan la misma instancia
            services.AddSingleton<AuthInterceptor>();

            services.AddHttpClient<IApiService, NgrokApiService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            }).AddHttpMessageHandler<AuthInterceptor>();

            services.AddTransient<IPagoRepository, PagoRepository>();
            services.AddTransient<IPedidoRepository, PedidoRepository>();

            services.AddTransient<IExcelExporter, ExcelExporter>();
            services.AddTransient<IDialogService, DialogService>();

            services.AddTransient<LoginViewModel>(sp => 
                new LoginViewModel(sp.GetRequiredService<AuthInterceptor>(), 
                                 sp.GetRequiredService<IApiService>(), 
                                 baseUrl));
            
            services.AddTransient<MainViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Manejador global de excepciones
            this.DispatcherUnhandledException += (s, ev) => {
                if (ev.Exception is System.Net.Http.HttpRequestException || ev.Exception is System.Net.Sockets.SocketException)
                {
                    ev.Handled = true; 
                }
            };

            var culture = new CultureInfo("es-ES");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            // Cargar configuración
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Iniciar con la ventana de Login
            MostrarLogin();
        }

        private void MostrarLogin()
        {
            var loginViewModel = ServiceProvider.GetRequiredService<LoginViewModel>();
            var loginWindow = new LoginWindow
            {
                DataContext = loginViewModel
            };

            loginViewModel.OnLoginSuccess += () => {
                // Aseguramos que el cambio de ventana ocurra en el hilo de UI
                Application.Current.Dispatcher.Invoke(() => {
                    // Evitamos que la app se cierre al cerrar la ventana de login
                    var oldMode = Application.Current.ShutdownMode;
                    Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                    MostrarMainWindow();
                    loginWindow.Close();

                    // Restauramos el modo normal para que se cierre al cerrar la MainWindow
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                });
            };

            loginWindow.Show();
        }

        private void MostrarMainWindow()
        {
            var mainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }
    }
}
