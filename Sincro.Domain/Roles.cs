using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sincro.Domain
{
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Gestor = "Gestor";
        public const string Vendedor = "Vendedor";

        public static readonly string[] Todas = { Administrador, Gestor, Vendedor };
    }
}