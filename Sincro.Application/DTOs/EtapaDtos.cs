using System.ComponentModel.DataAnnotations;

namespace Sincro.Application.DTOs
{
    // ---------- Etapas (colunas do Kanban) ----------
    // Sem Ordem: a etapa nova entra antes da última, e a ordem só muda pelo /reordenar
    public record CriarEtapaDto(
        [Required(ErrorMessage = "Chave é obrigatória")]
        [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "Chave deve ter só letras minúsculas, números e hífen")]
        [MaxLength(50, ErrorMessage = "Chave deve ter no máximo 50 caracteres")]
        string Chave,

        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(40, ErrorMessage = "Nome deve ter no máximo 40 caracteres")]
        string Nome,

        [Required(ErrorMessage = "Cor é obrigatória")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor deve estar no formato #RRGGBB")]
        string Cor);

    // Sem Chave (imutável: pedidos e eventos guardam a chave) e sem Ordem
    public record AtualizarEtapaDto(
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(40, ErrorMessage = "Nome deve ter no máximo 40 caracteres")]
        string Nome,

        [Required(ErrorMessage = "Cor é obrigatória")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor deve estar no formato #RRGGBB")]
        string Cor);

    // Ids de TODAS as etapas, na nova ordem (tudo de uma vez, sem estados intermediários)
    public record ReordenarEtapasDto(
        [Required(ErrorMessage = "Informe a nova ordem das etapas")] List<int> Ids);

    public record EtapaDto(int Id, string Chave, string Nome, string Cor, int Ordem);
}
