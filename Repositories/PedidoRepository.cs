using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZBarEscritorio.Domain.Models;
using EZBarEscritorio.Infrastructure.Network;

namespace EZBarEscritorio.Repositories
{
    public interface IPedidoRepository
    {
        Task<IEnumerable<Pedido>> ObtenerTodosAsync();
        IEnumerable<Pedido> FiltrarPedidos(IEnumerable<Pedido> pedidos, string query, DateTime? fecha);
        Task<bool> ActualizarEstadoPedidoAsync(int id, string nuevoEstado);
    }

    public class PedidoRepository : IPedidoRepository
    {
        private readonly IApiService _apiService;

        public PedidoRepository(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IEnumerable<Pedido>> ObtenerTodosAsync()
        {
            var dtos = await _apiService.GetAsync<PedidoDto>("/api/pedidos");
            
            return dtos.Select(dto => new Pedido
            {
                Id = dto.Id,
                MesaId = dto.Mesa?.MesaId ?? 0,
                Total = dto.Total,
                Estado = dto.Estado,
                FechaPedido = dto.FechaPedido
            });
        }

        public IEnumerable<Pedido> FiltrarPedidos(IEnumerable<Pedido> pedidos, string query, DateTime? fecha)
        {
            var result = pedidos.AsQueryable();

            // Filtrar solo pedidos "activos" (no pagados, cancelados o entregados)
            var estadosActivos = new List<string> { "pendiente", "en_preparacion", "listo" };
            result = result.Where(p => p.Estado != null && estadosActivos.Contains(p.Estado.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(query))
            {
                result = result.Where(p => p.Id.ToString() == query || p.MesaId.ToString() == query);
            }

            if (fecha.HasValue)
            {
                result = result.Where(p => p.FechaPedido.Date == fecha.Value.Date);
            }

            return result.ToList();
        }

        public async Task<bool> ActualizarEstadoPedidoAsync(int id, string nuevoEstado)
        {
            // Para cambiar de estado de forma general, obtenemos el pedido actual (o creamos un DTO parcial)
            // El backend Java para Pedidos PUT /pedidos/{id} espera el objeto Pedidos.
            // Para simplificar, si es 'listo' usamos el endpoint específico, si es 'pagado' usamos PUT general.
            
            if (nuevoEstado.ToLower() == "listo")
            {
                return await _apiService.PutAsync($"/api/pedidos/{id}/finalizar", new { });
            }

            // Para otros estados (como 'pagado'), enviamos el objeto parcial al PUT general
            var payload = new { pedido_id = id, estado = nuevoEstado };
            return await _apiService.PutAsync($"/api/pedidos/{id}", payload);
        }
    }
}
