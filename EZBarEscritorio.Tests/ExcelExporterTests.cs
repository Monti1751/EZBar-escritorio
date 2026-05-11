using FluentAssertions;
using EZBarEscritorio.Services;
using EZBarEscritorio.Domain.Models;
using System.Collections.Generic;
using System;
using System.IO;
using Xunit;

namespace EZBarEscritorio.Tests
{
    public class ExcelExporterTests
    {
        [Fact]
        public void ExportarPagos_NullOrEmpty_DoesNotThrow()
        {
            var exporter = new ExcelExporter();
            var fileName = Path.Combine(Path.GetTempPath(), "test_pagos.xlsx");

            // Test with null
            Action actNull = () => exporter.ExportarPagos(null!, fileName);
            actNull.Should().NotThrow();

            // Test with empty
            Action actEmpty = () => exporter.ExportarPagos(new List<Pago>(), fileName);
            actEmpty.Should().NotThrow();

            if (File.Exists(fileName)) File.Delete(fileName);
        }

        [Fact]
        public void ExportarPedidos_NullOrEmpty_DoesNotThrow()
        {
            var exporter = new ExcelExporter();
            var fileName = Path.Combine(Path.GetTempPath(), "test_pedidos.xlsx");

            // Test with null
            Action actNull = () => exporter.ExportarPedidos(null!, fileName);
            actNull.Should().NotThrow();

            if (File.Exists(fileName)) File.Delete(fileName);
        }
    }
}
