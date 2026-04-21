using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZBarEscritorio.Domain.Models;
using EZBarEscritorio.Infrastructure.Network;

namespace EZBarEscritorio.Repositories
{
    public interface IPagoRepository
    {
        Task<IEnumerable<Pago>> ObtenerTodosAsync();
        IEnumerable<Pago> FiltrarPagos(IEnumerable<Pago> pagos, string query, DateTime? fecha);
        Task<bool> ActualizarEstadoPagoAsync(int id, string nuevoEstado);
        Task<bool> CrearPagoAsync(int pedidoId, decimal monto, string metodoPago);
    }

    public class PagoRepository : IPagoRepository
    {
        private readonly IApiService _apiService;

        public PagoRepository(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IEnumerable<Pago>> ObtenerTodosAsync()
        {
            var dtos = await _apiService.GetAsync<PagoDto>("/api/pagos");
            
            return dtos.Select(dto => new Pago
            {
                Id = dto.Id,
                PedidoId = dto.Pedido?.Id ?? 0,
                Monto = dto.Monto,
                MetodoPago = dto.MetodoPago,
                Estado = dto.Pedido?.Estado ?? "Pagado",
                FechaPago = dto.FechaPago
            });
        }

        public IEnumerable<Pago> FiltrarPagos(IEnumerable<Pago> pagos, string query, DateTime? fecha)
        {
            var result = pagos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                result = result.Where(p => p.MetodoPago.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                                           p.Id.ToString() == query);
            }

            if (fecha.HasValue)
            {
                result = result.Where(p => p.FechaPago.Date == fecha.Value.Date);
            }

            return result.ToList();
        }

        public async Task<bool> ActualizarEstadoPagoAsync(int id, string nuevoEstado)
        {
            // El backend acepta PUT /api/pagos/{id} con el objeto
            var payload = new { pago_id = id, estado = nuevoEstado }; // El backend Java usa pago_id
            return await _apiService.PutAsync($"/api/pagos/{id}", payload);
        }

        public async Task<bool> CrearPagoAsync(int pedidoId, decimal monto, string metodoPago)
        {
            // Nota: Para simplificar, usamos empleado_id = 1 (Juan o el que exista)
            // En un sistema real esto vendría del usuario logueado.
            var payload = new 
            { 
                pedido = new { pedido_id = pedidoId },
                empleado = new { empleado_id = 1 }, // Fallback al admin/caja
                metodo_pago = metodoPago.ToLower(),
                monto = monto
            };
            
            return await _apiService.PostAsync("/api/pagos", payload);
        }
    }
}
