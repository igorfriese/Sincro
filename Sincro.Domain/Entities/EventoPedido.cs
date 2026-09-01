using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain.Entities
{
    public class EventoPedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public Pedido? Pedido { get; set; }
        public string Etapa { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
        public string? Observacao { get; set; }
        public string? RegistradoPorId { get; set; }
    }
}
