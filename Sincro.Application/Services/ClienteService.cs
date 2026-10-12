using Microsoft.EntityFrameworkCore;
using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;
using Sincro.Application.Interfaces;
using Sincro.Domain.Entities;
using Sincro.Domain.Interfaces;

namespace Sincro.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ClienteService(IClienteRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ListaClientesDto> ListarAsync(int pagina, int tamanho, bool incluirToken)
        {
            pagina = Math.Max(pagina, 1);
            tamanho = Math.Clamp(tamanho, 1, 100);

            var clientes = await _repository.ListarTodosAsync();
            var itens = clientes
                .OrderBy(c => c.Nome)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(c => ParaDto(c, incluirToken))
                .ToList();

            return new ListaClientesDto(clientes.Count, pagina, tamanho, itens);
        }

        public async Task<ClienteDto> ObterAsync(int id, bool incluirToken)
            => ParaDto(await ObterOuFalharAsync(id), incluirToken);

        public async Task<ClienteDto> CriarAsync(CriarClienteDto dto)
        {
            var existentes = await _repository.ListarTodosAsync();
            var documento = ValidarDocumento(dto.Documento);

            if (existentes.Any(c => SoDigitos(c.Documento) == SoDigitos(documento)))
                throw new RegraDeNegocioException("Já existe um cliente com este CNPJ");

            var cliente = new Cliente
            {
                Codigo = CodigoSequencial.Proximo("CLI", existentes.Select(c => c.Codigo)),
                Nome = dto.Nome.Trim(),
                Documento = documento,
                Email = Limpar(dto.Email),
                Telefone = Limpar(dto.Telefone),
                Endereco = Limpar(dto.Endereco),
                TokenAcompanhamento = Guid.NewGuid().ToString("N")
            };

            await _repository.AdicionarAsync(cliente);
            await _unitOfWork.SalvarAsync();

            return ParaDto(cliente, incluirToken: true);
        }

        public async Task<ClienteDto> AtualizarAsync(int id, AtualizarClienteDto dto)
        {
            var cliente = await ObterOuFalharAsync(id);
            var documento = ValidarDocumento(dto.Documento);

            var existentes = await _repository.ListarTodosAsync();
            if (existentes.Any(c => c.Id != id && SoDigitos(c.Documento) == SoDigitos(documento)))
                throw new RegraDeNegocioException("Já existe outro cliente com este CNPJ");

            cliente.Nome = dto.Nome.Trim();
            cliente.Documento = documento;
            cliente.Email = Limpar(dto.Email);
            cliente.Telefone = Limpar(dto.Telefone);
            cliente.Endereco = Limpar(dto.Endereco);

            await _unitOfWork.SalvarAsync();

            return ParaDto(cliente, incluirToken: true);
        }

        public async Task DeletarAsync(int id)
        {
            var cliente = await ObterOuFalharAsync(id);

            try
            {
                _repository.Remover(cliente);
                await _unitOfWork.SalvarAsync();
            }
            catch (DbUpdateException)
            {
                throw new RegraDeNegocioException("Cliente possui pedidos e não pode ser deletado");
            }
        }

        // ---------- helpers ----------

        private async Task<Cliente> ObterOuFalharAsync(int id)
            => await _repository.ObterPorIdAsync(id)
               ?? throw new NaoEncontradoException("Cliente não encontrado");

        // Aceita com ou sem pontuação, mas precisa ter os 14 dígitos do CNPJ
        private static string ValidarDocumento(string documento)
        {
            if (SoDigitos(documento).Length != 14)
                throw new RegraDeNegocioException("CNPJ deve ter 14 dígitos");

            return documento.Trim();
        }

        private static string SoDigitos(string? texto)
            => new((texto ?? "").Where(char.IsDigit).ToArray());

        private static string? Limpar(string? texto)
            => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

        private static ClienteDto ParaDto(Cliente c, bool incluirToken)
            => new(c.Id, c.Codigo, c.Nome, c.Documento, c.Telefone, c.Email, c.Endereco,
                incluirToken ? c.TokenAcompanhamento : null);
    }
}
