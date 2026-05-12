using System.Collections.Generic;
using ClosedXML.Excel;
using EZBarEscritorio.Domain.Models;
using System.IO;
using System;

namespace EZBarEscritorio.Services
{
    public class ExcelExporter : IExcelExporter
    {
        private void ValidarRuta(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new ArgumentException("La ruta del archivo no puede estar vacía.");

            // Validar extensión
            if (!ruta.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Solo se permiten archivos con extensión .xlsx por seguridad.");

            // Validar caracteres inválidos en la ruta
            if (ruta.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException("La ruta contiene caracteres no válidos.");

            // Validar que no sea una ruta de sistema crítica (ejemplo simplificado)
            string fullPath = Path.GetFullPath(ruta);
            if (fullPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows), StringComparison.OrdinalIgnoreCase) ||
                fullPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.System), StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Seguridad: No se permite exportar archivos a directorios del sistema.");
            }
        }

        public void ExportarPagos(IEnumerable<Pago> pagos, string rutaArchivo)
        {
            ValidarRuta(rutaArchivo);
            if (pagos == null) return;
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pagos");
            
            // ... resto del código ...
            // Cabeceras
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Fecha";
            worksheet.Cell(1, 3).Value = "Método";
            worksheet.Cell(1, 4).Value = "Total";
            worksheet.Cell(1, 5).Value = "Entregado";
            worksheet.Cell(1, 6).Value = "Cambio";
            worksheet.Cell(1, 7).Value = "Estado";

            // Estilo cabecera
            var headerRange = worksheet.Range(1, 1, 1, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4E342E");
            headerRange.Style.Font.FontColor = XLColor.White;

            // Datos
            int row = 2;
            foreach (var p in pagos)
            {
                worksheet.Cell(row, 1).Value = p.Id;
                worksheet.Cell(row, 2).Value = p.FechaPago;
                worksheet.Cell(row, 3).Value = p.MetodoPago;
                worksheet.Cell(row, 4).Value = p.Monto;
                worksheet.Cell(row, 5).Value = p.MontoEntregado;
                worksheet.Cell(row, 6).Value = p.Cambio;
                worksheet.Cell(row, 7).Value = p.Estado;
                
                // Formato moneda
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 €";
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 €";
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00 €";
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(rutaArchivo);
        }

        public void ExportarPedidos(IEnumerable<Pedido> pedidos, string rutaArchivo)
        {
            ValidarRuta(rutaArchivo);
            if (pedidos == null) return;
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pedidos");

            // Cabeceras
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Fecha";
            worksheet.Cell(1, 3).Value = "Mesa";
            worksheet.Cell(1, 4).Value = "Zona";
            worksheet.Cell(1, 5).Value = "Total";
            worksheet.Cell(1, 6).Value = "Estado";

            // Estilo cabecera
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4E342E");
            headerRange.Style.Font.FontColor = XLColor.White;

            // Datos
            int row = 2;
            foreach (var p in pedidos)
            {
                worksheet.Cell(row, 1).Value = p.Id;
                worksheet.Cell(row, 2).Value = p.FechaPedido;
                worksheet.Cell(row, 3).Value = p.NombreMesa;
                worksheet.Cell(row, 4).Value = p.Zona;
                worksheet.Cell(row, 5).Value = p.Total;
                worksheet.Cell(row, 6).Value = p.Estado;

                // Formato moneda
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 €";
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(rutaArchivo);
        }
    }
}
