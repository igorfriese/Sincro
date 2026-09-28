using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sincro.Domain.Entities;

namespace Sincro.Application.Services
{
    public class EtapaService
    {
        public (bool Permitido, string? Motivo) ValidarTransicao(
            List<Etapa> etapas,
            string chaveAtual,
            string chaveDestino)

        {
            if (chaveAtual == chaveDestino)
            {
                return (true, null);
            }

            var etapasOrdenadas = etapas
                .OrderBy(e => e.Ordem)
                .ToList();

            var indiceAtual = etapasOrdenadas
                .FindIndex(e => e.Chave == chaveAtual);

            var indiceDestino = etapasOrdenadas
                .FindIndex(e => e.Chave == chaveDestino);

            var indiceUltima = etapasOrdenadas.Count - 1;

            if (indiceAtual == -1 || indiceDestino == -1)
            {
                return (false, "Etapa inválida.");
            }

            if (indiceAtual == indiceUltima)
            {
                return (false, $"Este pedido já está \"{etapasOrdenadas[indiceUltima].Nome}\" e não pode ser mais movimentado.");
            }

            return (true, null);
        }
    }
}
