using System;
using System.Text.Json.Serialization;

namespace EZBarEscritorio.Domain.Models
{
    // DTO Inmutable para Pago
    public record PagoDto(
        [property: JsonPropertyName("pago_id")] int Id,
        [property: JsonPropertyName("monto")] decimal Monto,
        [property: JsonPropertyName("metodo_pago")] string MetodoPago,
        [property: JsonPropertyName("fecha_hora_pago")] DateTime FechaPago,
        [property: JsonPropertyName("pedido")] PedidoDto Pedido
    );

    // Modelo de Dominio de Pago
    public class Pago
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; }
        public string Estado { get; set; } // Enlazado al estado del pedido si no existe campo estado en pagos
        public DateTime FechaPago { get; set; }
    }
}
