using Moq;
using FluentAssertions;
using EZBarEscritorio.Repositories;
using EZBarEscritorio.Infrastructure.Network;
using EZBarEscritorio.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Xunit;

namespace EZBarEscritorio.Tests
{
    public class RepositoryTests
    {
        private readonly Mock<IApiService> _apiServiceMock;
        private readonly PagoRepository _pagoRepo;
        private readonly PedidoRepository _pedidoRepo;

        public RepositoryTests()
        {
            _apiServiceMock = new Mock<IApiService>();
            _pagoRepo = new PagoRepository(_apiServiceMock.Object);
            _pedidoRepo = new PedidoRepository(_apiServiceMock.Object);
        }

        [Fact]
        public async Task PagoRepository_ObtenerTodosAsync_ReturnsMappedModels()
        {
            // Arrange
            var dtos = new List<PagoDto>
            {
                new PagoDto(1, 100, "efectivo", DateTime.Now, 100, 0, 
                    new PedidoDto(10, new MesaDto(1, 1, "Sala", "Mesa 1"), 100, "pagado", DateTime.Now))
            };
            _apiServiceMock.Setup(s => s.GetAsync<PagoDto>("/api/pagos"))
                .ReturnsAsync(dtos);

            // Act
            var result = await _pagoRepo.ObtenerTodosAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
            result.First().PedidoId.Should().Be(10);
            result.First().MetodoPago.Should().Be("efectivo");
        }

        [Fact]
        public async Task PedidoRepository_ObtenerTodosAsync_ReturnsMappedModels()
        {
            // Arrange
            var dtos = new List<PedidoDto>
            {
                new PedidoDto(1, new MesaDto(5, 5, "Terraza", "Mesa 5"), 50, "pendiente", DateTime.Now)
            };
            _apiServiceMock.Setup(s => s.GetAsync<PedidoDto>("/api/pedidos"))
                .ReturnsAsync(dtos);

            // Act
            var result = await _pedidoRepo.ObtenerTodosAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
            result.First().NombreMesa.Should().Be("Mesa 5");
            result.First().Zona.Should().Be("Terraza");
        }

        [Fact]
        public void PagoRepository_FiltrarPagos_AppliesFiltersCorrectly()
        {
            // Arrange
            var pagos = new List<Pago>
            {
                new Pago { Id = 1, MetodoPago = "Efectivo", FechaPago = new DateTime(2023, 1, 1) },
                new Pago { Id = 2, MetodoPago = "Tarjeta", FechaPago = new DateTime(2023, 1, 2) }
            };

            // Act & Assert
            _pagoRepo.FiltrarPagos(pagos, "Tarjeta", null).Should().HaveCount(1);
            _pagoRepo.FiltrarPagos(pagos, "1", null).Should().HaveCount(1);
            _pagoRepo.FiltrarPagos(pagos, "", new DateTime(2023, 1, 1)).Should().HaveCount(1);
            _pagoRepo.FiltrarPagos(pagos, null, null).Should().HaveCount(2);
            _pagoRepo.FiltrarPagos(pagos, "Inexistente", null).Should().BeEmpty();
        }

        [Fact]
        public void PedidoRepository_FiltrarPedidos_AppliesFiltersCorrectly()
        {
            // Arrange
            var pedidos = new List<Pedido>
            {
                new Pedido { Id = 1, MesaId = 1, Estado = "pendiente", FechaPedido = new DateTime(2023, 1, 1) },
                new Pedido { Id = 2, MesaId = 2, Estado = "pagado", FechaPedido = new DateTime(2023, 1, 2) }
            };

            // Act & Assert
            // El repositorio filtra por estados activos: "pendiente", "en_preparacion", "listo"
            _pedidoRepo.FiltrarPedidos(pedidos, null, null).Should().HaveCount(1); // Solo el pendiente
            _pedidoRepo.FiltrarPedidos(pedidos, "1", null).Should().HaveCount(1);
            _pedidoRepo.FiltrarPedidos(pedidos, "MesaInexistente", null).Should().BeEmpty();
            _pedidoRepo.FiltrarPedidos(pedidos, null, new DateTime(2023, 1, 1)).Should().HaveCount(1);
        }

        [Fact]
        public async Task PedidoRepository_ActualizarEstadoPedidoAsync_GeneralEndpoint()
        {
            // Arrange
            _apiServiceMock.Setup(s => s.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await _pedidoRepo.ActualizarEstadoPedidoAsync(1, "pagado");

            // Assert
            result.Should().BeTrue();
            _apiServiceMock.Verify(s => s.PutAsync("/api/pedidos/1", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PagoRepository_ActualizarEstadoPagoAsync_CallsApi()
        {
            // Arrange
            _apiServiceMock.Setup(s => s.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await _pagoRepo.ActualizarEstadoPagoAsync(1, "Completado");

            // Assert
            result.Should().BeTrue();
            _apiServiceMock.Verify(s => s.PutAsync("/api/pagos/1", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PedidoRepository_ActualizarEstadoPedidoAsync_FinalizarEndpoint()
        {
            // Arrange
            _apiServiceMock.Setup(s => s.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            var result = await _pedidoRepo.ActualizarEstadoPedidoAsync(1, "Listo");

            // Assert
            result.Should().BeTrue();
            _apiServiceMock.Verify(s => s.PutAsync("/api/pedidos/1/finalizar", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PagoRepository_ActualizarEstadoPagoAsync_Failure()
        {
            // Arrange
            _apiServiceMock.Setup(s => s.PutAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(false);

            // Act
            var result = await _pagoRepo.ActualizarEstadoPagoAsync(1, "Fallido");

            // Assert
            result.Should().BeFalse();
        }
    }
}
