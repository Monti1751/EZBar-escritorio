using System.Collections.Generic;
using ClosedXML.Excel;
using EZBarEscritorio.Domain.Models;

namespace EZBarEscritorio.Services
{
    public class ExcelExporter
    {
        public void ExportarPagos(IEnumerable<Pago> pagos, string rutaArchivo)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pagos");

            // Cabeceras
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Monto";
            worksheet.Cell(1, 3).Value = "Método de Pago";
            worksheet.Cell(1, 4).Value = "Estado";
            worksheet.Cell(1, 5).Value = "Fecha de Pago";

            // Datos
            int row = 2;
            foreach (var p in pagos)
            {
                worksheet.Cell(row, 1).Value = p.Id;
                worksheet.Cell(row, 2).Value = p.Monto;
                worksheet.Cell(row, 3).Value = p.MetodoPago;
                worksheet.Cell(row, 4).Value = p.Estado;
                worksheet.Cell(row, 5).Value = p.FechaPago;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(rutaArchivo);
        }

        public void ExportarPedidos(IEnumerable<Pedido> pedidos, string rutaArchivo)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pedidos");

            // Cabeceras
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Mesa ID";
            worksheet.Cell(1, 3).Value = "Total";
            worksheet.Cell(1, 4).Value = "Estado";
            worksheet.Cell(1, 5).Value = "Fecha del Pedido";

            // Datos
            int row = 2;
            foreach (var p in pedidos)
            {
                worksheet.Cell(row, 1).Value = p.Id;
                worksheet.Cell(row, 2).Value = p.MesaId;
                worksheet.Cell(row, 3).Value = p.Total;
                worksheet.Cell(row, 4).Value = p.Estado;
                worksheet.Cell(row, 5).Value = p.FechaPedido;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(rutaArchivo);
        }
    }
}
