using System;
using System.Windows;
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
            var baseUrl = Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7082";
            var tokenJwt = Configuration.GetValue<string>("ApiSettings:JwtToken") ?? "";

            services.AddTransient(sp => new AuthInterceptor(tokenJwt));

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
