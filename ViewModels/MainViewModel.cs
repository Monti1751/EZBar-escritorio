using EZBarEscritorio.Domain.Models;
using EZBarEscritorio.Repositories;
using EZBarEscritorio.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace EZBarEscritorio.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IPagoRepository _pagoRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IExcelExporter _excelExporter;
        private readonly IDialogService _dialogService;
        private System.Windows.Threading.DispatcherTimer _timer;
        
        private List<Pago> _pagosCache;
        private List<Pedido> _pedidosCache;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _filtroPagos;

        [ObservableProperty]
        private string _filtroPedidos;

        [ObservableProperty]
        private ObservableCollection<Pago> _pagosFiltrados;

        [ObservableProperty]
        private ObservableCollection<Pedido> _pedidosFiltrados;

        [ObservableProperty]
        private string _statusMessage = "Listo";

        [ObservableProperty]
        private bool _isConnectionError;

        [ObservableProperty]
        private DateTime? _fechaSeleccionadaPagos;

        partial void OnFechaSeleccionadaPagosChanged(DateTime? value) => AplicarFiltrosPagos();

        partial void OnFiltroPagosChanged(string value) => AplicarFiltrosPagos();
        partial void OnFiltroPedidosChanged(string value) => AplicarFiltrosPedidos();

        public MainViewModel(IPagoRepository pagoRepository, IPedidoRepository pedidoRepository, IExcelExporter excelExporter, IDialogService dialogService)
        {
            _pagoRepository = pagoRepository;
            _pedidoRepository = pedidoRepository;
            _excelExporter = excelExporter;
            _dialogService = dialogService;
            
            _pagosFiltrados = new ObservableCollection<Pago>();
            _pedidosFiltrados = new ObservableCollection<Pedido>();
            
            _pagosCache = new List<Pago>();
            _pedidosCache = new List<Pedido>();

            // Configurar sincronización en tiempo real (cada 30 segundos en lugar de 10)
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(30);
            _timer.Tick += async (s, e) => {
                // Solo recargamos en segundo plano si no estamos ya cargando manualmente y no hay error
                if (!IsLoading && !IsConnectionError) {
                    try {
                        var pagos = await _pagoRepository.ObtenerTodosAsync();
                        _pagosCache = pagos.ToList();
                        var pedidos = await _pedidoRepository.ObtenerTodosAsync();
                        _pedidosCache = pedidos.ToList();
                        
                        AplicarFiltrosPagos();
                        AplicarFiltrosPedidos();
                    } catch { 
                        // El timer falla silenciosamente para no molestar
                    }
                }
            };
            if (System.Windows.Application.Current != null)
            {
                _timer.Start();
            }

            // La carga inicial se disparará desde el evento Loaded de la vista para evitar bloqueos prematuros
            // _ = CargarDatosAsync();
        }


        [RelayCommand]
        public async Task CargarDatosAsync()
        {
            if (IsLoading) return;
            
            // Aseguramos que la UI tenga tiempo de procesar estados iniciales
            await Task.Yield();

            SetLoading(true);
            IsConnectionError = false; // Resetear error al intentar de nuevo
            ActualizarStatus("Sincronizando con la API...");
            
            try
            {
                ActualizarStatus("Obteniendo pagos...");
                var pagos = await _pagoRepository.ObtenerTodosAsync();
                _pagosCache = pagos.ToList();
                
                ActualizarStatus($"Pagos OK ({_pagosCache.Count}). Obteniendo pedidos...");
                var pedidos = await _pedidoRepository.ObtenerTodosAsync();
                _pedidosCache = pedidos.ToList();

                ActualizarStatus("Actualizando tablas...");
                AplicarFiltrosPagos();
                AplicarFiltrosPedidos();
                
                ActualizarStatus($"Éxito: {_pagosCache.Count} pagos y {_pedidosCache.Count} pedidos cargados.");
            }
            catch (Exception ex)
            {
                var msg = $"Error de conexión: {ex.Message}";
                if (ex.InnerException != null) msg += $" -> {ex.InnerException.Message}";
                
                // Si es un timeout o error de red, lo reflejamos en la UI
                ActualizarStatus(msg);
                IsConnectionError = true;
            }
            finally
            {
                SetLoading(false);
            }
        }

        [RelayCommand]
        public async Task RetryConnectionAsync()
        {
            await CargarDatosAsync();
        }

        [RelayCommand]
        public void AplicarFiltrosPagos()
        {
            var resultado = _pagoRepository.FiltrarPagos(_pagosCache, FiltroPagos, FechaSeleccionadaPagos);
            
            // Aseguramos que la actualización de la colección observable sea en el hilo de UI
            ExecuteOnUIThread(() => {
                PagosFiltrados.Clear();
                foreach (var p in resultado) PagosFiltrados.Add(p);
            });
        }

        [RelayCommand]
        public void AplicarFiltrosPedidos()
        {
            var resultado = _pedidoRepository.FiltrarPedidos(_pedidosCache, FiltroPedidos, FechaSeleccionadaPagos);
            
            ExecuteOnUIThread(() => {
                PedidosFiltrados.Clear();
                foreach (var p in resultado) PedidosFiltrados.Add(p);
            });
        }

        private void ActualizarStatus(string msg)
        {
            ExecuteOnUIThread(() => StatusMessage = msg);
        }

        private void SetLoading(bool loading)
        {
            ExecuteOnUIThread(() => IsLoading = loading);
        }

        [RelayCommand]
        public async Task CambiarEstadoPagoAsync(Pago pago)
        {
            if (pago == null || pago.PedidoId == 0) return;
            
            SetLoading(true);
            try 
            {
                ActualizarStatus($"Pago {pago.Id} verificado. El pedido {pago.PedidoId} ya está completado.");
                _dialogService.ShowMessage($"El pago {pago.Id} del pedido {pago.PedidoId} ya se encuentra completado y registrado correctamente.", "Pago Completado");
                SetLoading(false);
                await CargarDatosAsync();
            }
            finally 
            {
                SetLoading(false);
            }
        }

        [RelayCommand]
        public async Task CambiarEstadoPedidoAsync(Pedido pedido)
        {
            if (pedido == null) return;
            SetLoading(true);
            bool exito = await _pedidoRepository.ActualizarEstadoPedidoAsync(pedido.Id, "Listo");
            SetLoading(false);
            if (exito) 
            {
                ActualizarStatus($"Pedido {pedido.Id} marcado como listo");
                await CargarDatosAsync();
            }
            else 
            {
                ActualizarStatus($"Error al actualizar pedido {pedido.Id}");
            }
        }

        [RelayCommand]
        public async Task PagarPedidoAsync(Pedido pedido)
        {
            if (pedido == null) return;

            // Abrir la nueva ventana de pago a través del servicio
            string metodoPago;
            decimal montoEntregado;
            decimal cambio;
            
            bool? result = _dialogService.ShowPagoDialog(pedido.Total, out metodoPago, out montoEntregado, out cambio);
            
            if (result != true) return;

            // Optimistic UI Update: Ocultar inmediatamente de la tabla para dar sensación de tiempo real
            ExecuteOnUIThread(() => {
                PedidosFiltrados.Remove(pedido);
            });

            SetLoading(true);
            try 
            {
                ActualizarStatus($"Registrando pago con {metodoPago}...");
                // 1. Crear el registro de pago con el método seleccionado
                bool pagoOk = await _pagoRepository.CrearPagoAsync(pedido.Id, pedido.Total, metodoPago, montoEntregado, cambio);
                
                SetLoading(false); // IMPORTANTE: Liberar para que CargarDatosAsync pueda ejecutarse
                
                if (pagoOk)
                {
                    ActualizarStatus($"Pedido {pedido.Id} pagado correctamente.");
                    await CargarDatosAsync();
                }
                else 
                {
                    ActualizarStatus("Error al registrar el pago en la API.");
                    await CargarDatosAsync(); // Revertir estado si falla
                }
            }
            catch (Exception ex)
            {
                SetLoading(false);
                ActualizarStatus($"Error en el proceso de pago: {ex.Message}");
                await CargarDatosAsync(); // Revertir estado si falla
            }
            finally 
            {
                SetLoading(false);
            }
        }

        [RelayCommand]
        public void ExportarPagos()
        {
            string? fileName = _dialogService.ShowSaveFileDialog("Pagos_Export.xlsx", "Excel documents (.xlsx)|*.xlsx");

            if (fileName != null)
            {
                try
                {
                    _excelExporter.ExportarPagos(PagosFiltrados, fileName);
                    _dialogService.ShowMessage("Archivo de Pagos exportado correctamente.", "Éxito");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Error al exportar: {ex.Message}", "Error", true);
                }
            }
        }

        [RelayCommand]
        public void ExportarPedidos()
        {
            string? fileName = _dialogService.ShowSaveFileDialog("Pedidos_Export.xlsx", "Excel documents (.xlsx)|*.xlsx");

            if (fileName != null)
            {
                try
                {
                    _excelExporter.ExportarPedidos(PedidosFiltrados, fileName);
                    _dialogService.ShowMessage("Archivo de Pedidos exportado correctamente.", "Éxito");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Error al exportar: {ex.Message}", "Error", true);
                }
            }
        }

        [RelayCommand]
        public void LimpiarFiltros()
        {
            FiltroPagos = string.Empty;
            FiltroPedidos = string.Empty;
            FechaSeleccionadaPagos = null;
        }
        private void ExecuteOnUIThread(Action action)
        {
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
