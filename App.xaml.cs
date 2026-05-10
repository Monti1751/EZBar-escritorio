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
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Configurar cultura a español (España) para usar € en lugar de $
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

            var mainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var baseUrl = Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:8080";
            var username = Configuration.GetValue<string>("ApiSettings:Username") ?? "admin";
            var password = Configuration.GetValue<string>("ApiSettings:Password") ?? "admin123";

            services.AddTransient(sp => new AuthInterceptor(username, password));

            services.AddHttpClient<IApiService, NgrokApiService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            }).AddHttpMessageHandler<AuthInterceptor>();

            services.AddTransient<IPagoRepository, PagoRepository>();
            services.AddTransient<IPedidoRepository, PedidoRepository>();

            services.AddTransient<ExcelExporter>();

            services.AddTransient<MainViewModel>();
        }
    }
}
