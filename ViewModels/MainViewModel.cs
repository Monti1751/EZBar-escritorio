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
        private readonly ExcelExporter _excelExporter;
        
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

        partial void OnFiltroPagosChanged(string value) => AplicarFiltrosPagos();
        partial void OnFiltroPedidosChanged(string value) => AplicarFiltrosPedidos();

        public MainViewModel(IPagoRepository pagoRepository, IPedidoRepository pedidoRepository, ExcelExporter excelExporter)
        {
            _pagoRepository = pagoRepository;
            _pedidoRepository = pedidoRepository;
            _excelExporter = excelExporter;
            
            _pagosFiltrados = new ObservableCollection<Pago>();
            _pedidosFiltrados = new ObservableCollection<Pedido>();
            
            _pagosCache = new List<Pago>();
            _pedidosCache = new List<Pedido>();
        }


        [RelayCommand]
        public async Task CargarDatosAsync()
        {
            if (IsLoading) return;

            ActualizarStatus("Sincronizando con la API...");
            SetLoading(true);

            try
            {
                ActualizarStatus("Obteniendo pagos...");
                var pagos = await _pagoRepository.ObtenerTodosAsync();
                _pagosCache = pagos.ToList();
                
                ActualizarStatus($"Pagos OK ({_pagosCache.Count}). Obteniendo pedidos...");
                var pedidos = await _pedidoRepository.ObtenerTodosAsync();
                _pedidosCache = pedidos.ToList();

                ActualizarStatus("Actualizando tablas...");
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AplicarFiltrosPagos();
                    AplicarFiltrosPedidos();
                });
                
                ActualizarStatus($"Éxito: {_pagosCache.Count} pagos y {_pedidosCache.Count} pedidos cargados.");
            }
            catch (Exception ex)
            {
                var msg = $"Error: {ex.Message}";
                if (ex.InnerException != null) msg += $" -> {ex.InnerException.Message}";
                ActualizarStatus(msg);
                
                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    System.Windows.MessageBox.Show(msg, "Error de Red", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                });
            }
            finally
            {
                SetLoading(false);
            }
        }

        [RelayCommand]
        public void AplicarFiltrosPagos()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                var resultado = _pagoRepository.FiltrarPagos(_pagosCache, FiltroPagos, null);
                PagosFiltrados.Clear();
                foreach (var p in resultado) PagosFiltrados.Add(p);
            });
        }

        [RelayCommand]
        public void AplicarFiltrosPedidos()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                var resultado = _pedidoRepository.FiltrarPedidos(_pedidosCache, FiltroPedidos, null);
                PedidosFiltrados.Clear();
                foreach (var p in resultado) PedidosFiltrados.Add(p);
            });
        }

        private void ActualizarStatus(string msg)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                StatusMessage = msg;
            });
        }

        private void SetLoading(bool loading)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                IsLoading = loading;
            });
        }

        [RelayCommand]
        public async Task CambiarEstadoPagoAsync(Pago pago)
        {
            if (pago == null || pago.PedidoId == 0) return;
            
            SetLoading(true);
            try 
            {
                ActualizarStatus($"Completando pedido {pago.PedidoId}...");
                // Completar el pago en realidad significa marcar el pedido como pagado en esta app
                bool exito = await _pedidoRepository.ActualizarEstadoPedidoAsync(pago.PedidoId, "pagado");
                
                if (exito) 
                {
                    ActualizarStatus($"Pedido {pago.PedidoId} completado y archivado.");
                    await CargarDatosAsync();
                }
                else 
                {
                    ActualizarStatus($"Error al completar pedido {pago.PedidoId}");
                }
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
            if (exito) 
            {
                ActualizarStatus($"Pedido {pedido.Id} marcado como listo");
                await CargarDatosAsync();
            }
            else 
            {
                ActualizarStatus($"Error al actualizar pedido {pedido.Id}");
            }
            SetLoading(false);
        }

        [RelayCommand]
        public async Task PagarPedidoAsync(Pedido pedido)
        {
            if (pedido == null) return;

            var r = System.Windows.MessageBox.Show($"¿Confirmar pago de {pedido.Total:C} para la mesa {pedido.MesaId}?", 
                "Confirmar Pago", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            
            if (r != System.Windows.MessageBoxResult.Yes) return;

            SetLoading(true);
            try 
            {
                ActualizarStatus("Registrando pago...");
                // 1. Crear el registro de pago (por defecto en efectivo para simplificar)
                bool pagoOk = await _pagoRepository.CrearPagoAsync(pedido.Id, pedido.Total, "efectivo");
                
                if (pagoOk)
                {
                    ActualizarStatus("Actualizando estado del pedido...");
                    // 2. Marcar el pedido como pagado
                    bool pedidoOk = await _pedidoRepository.ActualizarEstadoPedidoAsync(pedido.Id, "pagado");
                    
                    if (pedidoOk)
                    {
                        ActualizarStatus($"Pedido {pedido.Id} pagado correctamente.");
                        await CargarDatosAsync();
                    }
                    else
                    {
                        ActualizarStatus("Pago registrado pero falló actualizar el pedido.");
                    }
                }
                else 
                {
                    ActualizarStatus("Error al registrar el pago en la API.");
                }
            }
            catch (Exception ex)
            {
                ActualizarStatus($"Error en el proceso de pago: {ex.Message}");
            }
            finally 
            {
                SetLoading(false);
            }
        }

        [RelayCommand]
        public void ExportarPagos()
        {
            _excelExporter.ExportarPagos(PagosFiltrados, "Pagos_Export.xlsx");
        }

        [RelayCommand]
        public void ExportarPedidos()
        {
            _excelExporter.ExportarPedidos(PedidosFiltrados, "Pedidos_Export.xlsx");
        }
    }
}
