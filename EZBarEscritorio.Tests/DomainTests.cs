using FluentAssertions;
using EZBarEscritorio.Domain.Models;
using System;
using Xunit;

namespace EZBarEscritorio.Tests
{
    public class DomainTests
    {
        [Fact]
        public void Pago_Properties_Work()
        {
            var now = DateTime.Now;
            var pago = new Pago
            {
                Id = 1,
                PedidoId = 10,
                Monto = 100,
                MetodoPago = "Efectivo",
                Estado = "Pagado",
                FechaPago = now,
                MontoEntregado = 100,
                Cambio = 0
            };

            pago.Id.Should().Be(1);
            pago.PedidoId.Should().Be(10);
            pago.Monto.Should().Be(100);
            pago.MetodoPago.Should().Be("Efectivo");
            pago.Estado.Should().Be("Pagado");
            pago.FechaPago.Should().Be(now);
            pago.MontoEntregado.Should().Be(100);
            pago.Cambio.Should().Be(0);
        }

        [Fact]
        public void Pedido_Properties_Work()
        {
            var now = DateTime.Now;
            var pedido = new Pedido
            {
                Id = 1,
                MesaId = 5,
                NombreMesa = "Mesa 5",
                Zona = "Sala",
                Total = 50,
                Estado = "Pendiente",
                FechaPedido = now
            };

            pedido.Id.Should().Be(1);
            pedido.MesaId.Should().Be(5);
            pedido.NombreMesa.Should().Be("Mesa 5");
            pedido.Zona.Should().Be("Sala");
            pedido.Total.Should().Be(50);
            pedido.Estado.Should().Be("Pendiente");
            pedido.FechaPedido.Should().Be(now);
        }

        [Fact]
        public void Dtos_Work()
        {
            var mesaDto = new MesaDto(1, 1, "Sala", "Mesa 1");
            var pedidoDto = new PedidoDto(10, mesaDto, 100, "pendiente", DateTime.Now);
            var pagoDto = new PagoDto(1, 100, "efectivo", DateTime.Now, 100, 0, pedidoDto);

            pagoDto.Id.Should().Be(1);
            pagoDto.Pedido.Id.Should().Be(10);
            pagoDto.Pedido.Mesa.Nombre.Should().Be("Mesa 1");
        }
    }
}
