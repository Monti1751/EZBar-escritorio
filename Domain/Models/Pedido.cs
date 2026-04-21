using System;
using System.Text.Json.Serialization;

namespace EZBarEscritorio.Domain.Models
{
    // DTOs matching Java Backend
    public record MesaDto(
        [property: JsonPropertyName("mesa_id")] int MesaId,
        [property: JsonPropertyName("numero_mesa")] int? Numero
    );

    public record PedidoDto(
        [property: JsonPropertyName("pedido_id")] int Id,
        [property: JsonPropertyName("mesa")] MesaDto Mesa,
        [property: JsonPropertyName("total_pedido")] decimal Total,
        [property: JsonPropertyName("estado")] string Estado,
        [property: JsonPropertyName("fecha_hora_pedido")] DateTime FechaPedido
    );

    // Modelo de Dominio de Pedido
    public class Pedido
    {
        public int Id { get; set; }
        public int MesaId { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public DateTime FechaPedido { get; set; }
    }
}
