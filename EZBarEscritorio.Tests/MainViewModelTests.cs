using Moq;
using FluentAssertions;
using EZBarEscritorio.ViewModels;
using EZBarEscritorio.Repositories;
using EZBarEscritorio.Services;
using EZBarEscritorio.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Xunit;

namespace EZBarEscritorio.Tests
{
    public class MainViewModelTests
    {
        private readonly Mock<IPagoRepository> _pagoRepoMock;
        private readonly Mock<IPedidoRepository> _pedidoRepoMock;
        private readonly Mock<IExcelExporter> _excelExporterMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly MainViewModel _viewModel;

        public MainViewModelTests()
        {
            _pagoRepoMock = new Mock<IPagoRepository>();
            _pedidoRepoMock = new Mock<IPedidoRepository>();
            _excelExporterMock = new Mock<IExcelExporter>();
            _dialogServiceMock = new Mock<IDialogService>();

            _viewModel = new MainViewModel(
                _pagoRepoMock.Object, 
                _pedidoRepoMock.Object, 
                _excelExporterMock.Object, 
                _dialogServiceMock.Object);
        }

        [Fact]
        public async Task CargarDatosAsync_Success_UpdatesCollections()
        {
            // Arrange
            var pagos = new List<Pago> { new Pago { Id = 1, MetodoPago = "Efectivo", Monto = 100 } };
            var pedidos = new List<Pedido> { new Pedido { Id = 1, NombreMesa = "Mesa 1", Estado = "pendiente" } };

            _pagoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(pagos);
            _pedidoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(pedidos);
            
            _pagoRepoMock.Setup(r => r.FiltrarPagos(It.IsAny<IEnumerable<Pago>>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                .Returns(pagos);
            _pedidoRepoMock.Setup(r => r.FiltrarPedidos(It.IsAny<IEnumerable<Pedido>>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
                .Returns(pedidos);

            // Act
            await _viewModel.CargarDatosAsync();

            // Assert
            _viewModel.IsLoading.Should().BeFalse();
            _viewModel.IsConnectionError.Should().BeFalse();
            _viewModel.PagosFiltrados.Should().HaveCount(1);
            _viewModel.PedidosFiltrados.Should().HaveCount(1);
        }

        [Fact]
        public async Task CargarDatosAsync_OnException_SetsConnectionError()
        {
            // Arrange
            _pagoRepoMock.Setup(r => r.ObtenerTodosAsync()).ThrowsAsync(new System.Net.Http.HttpRequestException("Network error"));

            // Act
            await _viewModel.CargarDatosAsync();

            // Assert
            _viewModel.IsLoading.Should().BeFalse();
            _viewModel.IsConnectionError.Should().BeTrue();
            _viewModel.StatusMessage.Should().Contain("Error de conexión");
        }

        [Fact]
        public void FiltroPagos_Changed_CallsFiltrarPagos()
        {
            // Act
            _viewModel.FiltroPagos = "Efectivo";

            // Assert
            _pagoRepoMock.Verify(r => r.FiltrarPagos(It.IsAny<IEnumerable<Pago>>(), "Efectivo", It.IsAny<DateTime?>()), Times.AtLeastOnce());
        }

        [Fact]
        public async Task PagarPedidoAsync_Success_CallsCreatePagoAndReloads()
        {
            // Arrange
            var pedido = new Pedido { Id = 1, Total = 50, NombreMesa = "Mesa 1" };
            string metodo = "efectivo";
            decimal entregado = 50;
            decimal cambio = 0;

            _dialogServiceMock.Setup(d => d.ShowPagoDialog(pedido.Total, out metodo, out entregado, out cambio))
                .Returns(true);
            
            _pagoRepoMock.Setup(r => r.CrearPagoAsync(pedido.Id, pedido.Total, "efectivo", 50, 0))
                .ReturnsAsync(true);
            
            _pagoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Pago>());
            _pedidoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Pedido>());

            // Act
            await _viewModel.PagarPedidoAsync(pedido);

            // Assert
            _pagoRepoMock.Verify(r => r.CrearPagoAsync(pedido.Id, pedido.Total, "efectivo", 50, 0), Times.Once);
            _pagoRepoMock.Verify(r => r.ObtenerTodosAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task CambiarEstadoPedidoAsync_Success_CallsRepoAndReloads()
        {
            // Arrange
            var pedido = new Pedido { Id = 1, Estado = "Pendiente" };
            _pedidoRepoMock.Setup(r => r.ActualizarEstadoPedidoAsync(pedido.Id, "Listo"))
                .ReturnsAsync(true);
            
            _pagoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Pago>());
            _pedidoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Pedido>());

            // Act
            await _viewModel.CambiarEstadoPedidoAsync(pedido);

            // Assert
            _pedidoRepoMock.Verify(r => r.ActualizarEstadoPedidoAsync(pedido.Id, "Listo"), Times.Once);
            _pedidoRepoMock.Verify(r => r.ObtenerTodosAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public void ExportarPagos_DialogAccepted_CallsExporter()
        {
            // Arrange
            _dialogServiceMock.Setup(d => d.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("test.xlsx");

            // Act
            _viewModel.ExportarPagos();

            // Assert
            _excelExporterMock.Verify(e => e.ExportarPagos(It.IsAny<IEnumerable<Pago>>(), "test.xlsx"), Times.Once);
            _dialogServiceMock.Verify(d => d.ShowMessage(It.IsAny<string>(), "Éxito", false), Times.Once);
        }

        [Fact]
        public async Task CambiarEstadoPedidoAsync_Failure_UpdatesStatus()
        {
            // Arrange
            var pedido = new Pedido { Id = 1 };
            _pedidoRepoMock.Setup(r => r.ActualizarEstadoPedidoAsync(pedido.Id, "Listo"))
                .ReturnsAsync(false);

            // Act
            await _viewModel.CambiarEstadoPedidoAsync(pedido);

            // Assert
            _viewModel.StatusMessage.Should().Contain("Error");
        }

        [Fact]
        public async Task PagarPedidoAsync_ApiFailure_UpdatesStatusAndReloads()
        {
            // Arrange
            var pedido = new Pedido { Id = 1, Total = 50 };
            string metodo = "efectivo";
            decimal entregado = 50;
            decimal cambio = 0;

            _dialogServiceMock.Setup(d => d.ShowPagoDialog(pedido.Total, out metodo, out entregado, out cambio))
                .Returns(true);
            
            _pagoRepoMock.Setup(r => r.CrearPagoAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ReturnsAsync(false);

            // Act
            await _viewModel.PagarPedidoAsync(pedido);

            // Assert
            _pagoRepoMock.Verify(r => r.CrearPagoAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Once);
            _pagoRepoMock.Verify(r => r.ObtenerTodosAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public void ExportarPagos_Exception_ShowsErrorMessage()
        {
            // (Existing code...)
            _dialogServiceMock.Setup(d => d.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("test.xlsx");
            _excelExporterMock.Setup(e => e.ExportarPagos(It.IsAny<IEnumerable<Pago>>(), "test.xlsx"))
                .Throws(new Exception("Write error"));

            _viewModel.ExportarPagos();
            _dialogServiceMock.Verify(d => d.ShowMessage(It.IsAny<string>(), "Error", true), Times.Once);
        }

        [Fact]
        public async Task CambiarEstadoPagoAsync_CallsDialogAndReloads()
        {
            // (Existing code...)
            var pago = new Pago { Id = 1, PedidoId = 10 };

            await _viewModel.CambiarEstadoPagoAsync(pago);

            _dialogServiceMock.Verify(d => d.ShowMessage(It.IsAny<string>(), "Pago Completado", false), Times.Once);
            _pagoRepoMock.Verify(r => r.ObtenerTodosAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public void FiltroPedidos_Changed_CallsFiltrarPedidos()
        {
            // Act
            _viewModel.FiltroPedidos = "Mesa 1";

            // Assert
            _pedidoRepoMock.Verify(r => r.FiltrarPedidos(It.IsAny<IEnumerable<Pedido>>(), "Mesa 1", It.IsAny<DateTime?>()), Times.AtLeastOnce());
        }

        [Fact]
        public void LimpiarFiltros_ResetsValues()
        {
            // Arrange
            _viewModel.FiltroPagos = "Test";
            _viewModel.FiltroPedidos = "Mesa 1";
            _viewModel.FechaSeleccionadaPagos = DateTime.Now;

            // Act
            _viewModel.LimpiarFiltros();

            // Assert
            _viewModel.FiltroPagos.Should().BeEmpty();
            _viewModel.FiltroPedidos.Should().BeEmpty();
            _viewModel.FechaSeleccionadaPagos.Should().BeNull();
        }

        [Fact]
        public async Task CargarDatosAsync_EmptyResponse_ClearsCollections()
        {
            // (Existing code...)
            _pagoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Pago>());
            _pedidoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Pedido>());

            await _viewModel.CargarDatosAsync();

            _viewModel.PagosFiltrados.Should().BeEmpty();
            _viewModel.StatusMessage.Should().Contain("0 pagos");
        }

        [Fact]
        public void AplicarFiltrosPagos_WithDate_CallsRepoWithDate()
        {
            // Arrange
            var date = new DateTime(2023, 5, 10);
            _viewModel.FechaSeleccionadaPagos = date;

            // Assert
            _pagoRepoMock.Verify(r => r.FiltrarPagos(It.IsAny<IEnumerable<Pago>>(), It.IsAny<string>(), date), Times.AtLeastOnce());
        }

        [Fact]
        public async Task RetryConnectionAsync_CallsCargarDatos()
        {
            // (Existing code...)
            _pagoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Pago>());
            _pedidoRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Pedido>());

            await _viewModel.RetryConnectionAsync();

            _pagoRepoMock.Verify(r => r.ObtenerTodosAsync(), Times.AtLeastOnce());
        }
    }
}
