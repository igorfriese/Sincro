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

        public ClienteService(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<ListaClientesDto> ListarAsync(int pagina, int tamanho)
        {
            pagina = Math.Max(pagina, 1);
            tamanho = Math.Clamp(tamanho, 1, 100);

            var clientes = await _repository.ListarTodosAsync();
            var itens = clientes
                .OrderByDescending(c => c.Id)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(ParaLista)
                .ToList();

            return new ListaClientesDto(clientes.Count, pagina, tamanho, itens);
        }

        public async Task<ClienteDetailDto> ObterAsync(int id)
        {
            var c = await ObterOuFalharAsync(id);

            return new ClienteDetailDto(c.Id, c.Nome, c.Email ?? "", c.Telefone ?? "",
                c.Endereco ?? "", c.Documento ?? "", true, DateTime.UtcNow);
        }

        public async Task<ClienteListaDto> CriarAsync(CriarClienteDto dto)
        {
            var existentes = await _repository.ListarTodosAsync();

            var cliente = new Cliente
            {
                Codigo = CodigoSequencial.Proximo("CLI", existentes.Select(c => c.Codigo)),
                Nome = dto.Nome,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Endereco = dto.Endereco,
                Documento = dto.CNPJ,
                TokenAcompanhamento = Guid.NewGuid().ToString()
            };

            await _repository.AdicionarAsync(cliente);
            await _repository.SalvarAsync();

            return ParaLista(cliente);
        }

        public async Task AtualizarAsync(int id, AtualizarClienteDto dto)
        {
            var cliente = await ObterOuFalharAsync(id);

            cliente.Nome = dto.Nome;
            cliente.Email = dto.Email;
            cliente.Telefone = dto.Telefone;
            cliente.Endereco = dto.Endereco;

            _repository.Atualizar(cliente);
            await _repository.SalvarAsync();
        }

        public async Task DeletarAsync(int id)
        {
            var cliente = await ObterOuFalharAsync(id);

            try
            {
                _repository.Remover(cliente);
                await _repository.SalvarAsync();
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

        private static ClienteListaDto ParaLista(Cliente c)
            => new(c.Id, c.Nome, c.Email ?? "", c.Telefone ?? "", DateTime.UtcNow);
    }
}