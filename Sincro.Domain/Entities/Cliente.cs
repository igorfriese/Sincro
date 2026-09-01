using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string? Documento { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string TokenAcompanhamento { get; set; }
    }
}
