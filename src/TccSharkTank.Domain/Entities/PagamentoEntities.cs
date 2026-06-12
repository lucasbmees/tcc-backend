namespace TccSharkTank.Domain.Entities;

public sealed class PgtPagamento : AuditableEntityBase<long>
{
    public long UsuarioId { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Metodo { get; set; } = "Cartão (Simulado)";
    public string Status { get; set; } = "Aprovado"; // Sempre aprovado na simulação

    public UsuUsuario? Usuario { get; set; }
}
