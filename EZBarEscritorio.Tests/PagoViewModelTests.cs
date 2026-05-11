using FluentAssertions;
using EZBarEscritorio.ViewModels;
using Xunit;

namespace EZBarEscritorio.Tests
{
    public class PagoViewModelTests
    {
        [Fact]
        public void InitialState_IsCorrect()
        {
            var vm = new PagoViewModel(100);
            vm.MetodoPago.Should().Be("Efectivo");
            vm.MostrarEfectivo.Should().BeTrue();
            vm.EsMontoValido.Should().BeFalse();
        }

        [Fact]
        public void ValidEfectivo_SetsValuesCorrectly()
        {
            var vm = new PagoViewModel(100);
            vm.MontoRecibidoStr = "150";
            
            vm.EsMontoValido.Should().BeTrue();
            vm.MontoEntregado.Should().Be(150);
            vm.Cambio.Should().Be(50);
            vm.CambioTexto.Should().Contain("50,00");
        }

        [Fact]
        public void InsufficientEfectivo_SetsInvalid()
        {
            var vm = new PagoViewModel(100);
            vm.MontoRecibidoStr = "50";
            
            vm.EsMontoValido.Should().BeFalse();
            vm.CambioTexto.Should().Contain("insuficiente");
        }

        [Fact]
        public void ChangeToTarjeta_SetsValidAndValues()
        {
            var vm = new PagoViewModel(100);
            vm.MetodoPago = "Tarjeta";
            
            vm.MostrarEfectivo.Should().BeFalse();
            vm.EsMontoValido.Should().BeTrue();
            vm.MontoEntregado.Should().Be(100);
            vm.Cambio.Should().Be(0);
        }
    }
}
