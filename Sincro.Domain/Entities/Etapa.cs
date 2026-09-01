using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain.Entities
{
    public class Etapa
    {
        public int Id { get; set; }
        public string Chave { get; set; }
        public string Nome { get; set; }
        public string Cor { get; set; }
        public int Ordem { get; set; }
    }
}
