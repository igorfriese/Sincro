using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain.Entities
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }    
        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }     
        public string? ResponsavelId { get; set; }
        public int Quantidade { get; set; }
        public DateTime Prazo { get; set; }
        public bool Urgente { get; set; }
        public string Coluna { get; set; } = string.Empty;
    }
}
