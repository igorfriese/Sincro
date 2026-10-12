namespace Sincro.Application.Services
{
    // Gera códigos no formato PREFIXO-0001 (CLI-0001, PRD-0001...).
  
    internal static class CodigoSequencial
    {
        public static string Proximo(string prefixo, IEnumerable<string> codigosExistentes)
        {
            var inicio = prefixo + "-";
            var maior = codigosExistentes
                .Where(c => c.StartsWith(inicio, StringComparison.Ordinal))
                .Select(c => int.TryParse(c.AsSpan(inicio.Length), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"{inicio}{maior + 1:D4}";
        }
    }
}