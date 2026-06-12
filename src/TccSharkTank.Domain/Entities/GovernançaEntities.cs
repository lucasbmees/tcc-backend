namespace TccSharkTank.Domain.Entities;

public sealed class GovDenuncia : AuditableEntityBase<long>
{
    public long DenuncianteId { get; set; }
    public string TipoAlvo { get; set; } = string.Empty; // "Ideia", "Usuario"
    public long AlvoId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendente"; // "Pendente", "Analisada", "Arquivada"
    public string? ObservacaoAdm { get; set; }

    public UsuUsuario? Denunciante { get; set; }
}
