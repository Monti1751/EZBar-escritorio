using System.Collections.Generic;
using EZBarEscritorio.Domain.Models;

namespace EZBarEscritorio.Services
{
    public interface IExcelExporter
    {
        void ExportarPagos(IEnumerable<Pago> pagos, string rutaArchivo);
        void ExportarPedidos(IEnumerable<Pedido> pedidos, string rutaArchivo);
    }
}
