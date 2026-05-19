using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TccSharkTank.Domain.Entities;

namespace TccSharkTank.Infrastructure.Persistence;

internal sealed class UsuCargoMap : IEntityTypeConfiguration<UsuCargo>
{
    public void Configure(EntityTypeBuilder<UsuCargo> e)
    {
        e.ToTable("usu_cargo");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("usu_cargo_id").ValueGeneratedOnAdd();
        e.Property(x => x.Nome).HasColumnName("usu_cargo_nome").HasMaxLength(50).IsRequired();
        e.HasIndex(x => x.Nome).IsUnique();

        e.HasData(
            new UsuCargo { Id = 1, Nome = "adm" },
            new UsuCargo { Id = 2, Nome = "empreendedor" },
            new UsuCargo { Id = 3, Nome = "investidor" }
        );
    }
}

internal sealed class UsuUsuarioMap : IEntityTypeConfiguration<UsuUsuario>
{
    public void Configure(EntityTypeBuilder<UsuUsuario> e)
    {
        e.ToTable("usu_usuario");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("usu_id").ValueGeneratedOnAdd();
        e.Property(x => x.Cpf).HasColumnName("usu_cpf").HasMaxLength(20).IsRequired();
        e.Property(x => x.Email).HasColumnName("usu_email").HasMaxLength(255).IsRequired();
        e.Property(x => x.Telefone).HasColumnName("usu_telefone").HasMaxLength(30).IsRequired();
        e.Property(x => x.Senha).HasColumnName("usu_senha").HasMaxLength(500).IsRequired();
        e.Property(x => x.CargoId).HasColumnName("usu_cargo_id").IsRequired();
        e.Property(x => x.Status).HasColumnName("usu_status").IsRequired();
        e.Property(x => x.UltimoLogin).HasColumnName("usu_ultimo_login");
        e.Property(x => x.Nome).HasColumnName("usu_nome").HasMaxLength(100).IsRequired();
        e.Property(x => x.Sobrenome).HasColumnName("usu_sobrenome").HasMaxLength(100).IsRequired();

        e.HasIndex(x => x.Email).HasDatabaseName("ix_usu_usuario_usu_email").IsUnique();
        e.HasIndex(x => x.Cpf).HasDatabaseName("ix_usu_usuario_usu_cpf").IsUnique();
        e.HasIndex(x => x.Telefone).IsUnique();

        e.HasOne(x => x.Cargo)
            .WithMany(c => c.Usuarios)
            .HasForeignKey(x => x.CargoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class UsuPerfilMap : IEntityTypeConfiguration<UsuPerfil>
{
    public void Configure(EntityTypeBuilder<UsuPerfil> e)
    {
        e.ToTable("usu_perfil");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("usu_perfil_id").ValueGeneratedOnAdd();
        e.Property(x => x.UsuarioId).HasColumnName("usu_usuario_id").IsRequired();
        e.Property(x => x.Descricao).HasColumnName("usu_perfil_descricao").HasMaxLength(2000);
        e.Property(x => x.Cep).HasColumnName("usu_perfil_cep").HasMaxLength(20);
        e.Property(x => x.DataNasc).HasColumnName("usu_perfil_data_nasc");
        e.Property(x => x.LinkRedes).HasColumnName("usu_perfil_link_redes").HasMaxLength(1000);
        e.Property(x => x.InvestTicketMin).HasColumnName("usu_perfil_invest_ticket_min").HasPrecision(18, 2);
        e.Property(x => x.InvestTicketMax).HasColumnName("usu_perfil_invest_ticket_max").HasPrecision(18, 2);
        e.Property(x => x.InvestInteresses).HasColumnName("usu_perfil_invest_interesses").HasMaxLength(2000);
        e.Property(x => x.ReceberEmailPropostas).HasColumnName("usu_perfil_email_propostas").HasDefaultValue(true);
        e.Property(x => x.ReceberEmailMensagens).HasColumnName("usu_perfil_email_mensagens").HasDefaultValue(true);
        e.Property(x => x.ReceberEmailAlertas).HasColumnName("usu_perfil_email_alertas").HasDefaultValue(true);
        e.Property(x => x.CreateDate).HasColumnName("usu_perfil_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("usu_perfil_update_date").IsRequired();

        e.HasIndex(x => x.UsuarioId).IsUnique();
        e.HasOne(x => x.Usuario)
            .WithOne(u => u.Perfil)
            .HasForeignKey<UsuPerfil>(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class IdaStatusMap : IEntityTypeConfiguration<IdaStatus>
{
    public void Configure(EntityTypeBuilder<IdaStatus> e)
    {
        e.ToTable("ida_status");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("ida_status_id").ValueGeneratedOnAdd();
        e.Property(x => x.Nome).HasColumnName("ida_status_nome").HasMaxLength(50).IsRequired();
        e.HasIndex(x => x.Nome).IsUnique();

        e.HasData(
            new IdaStatus { Id = 1, Nome = "pendente" },
            new IdaStatus { Id = 2, Nome = "aprovada" },
            new IdaStatus { Id = 3, Nome = "reprovada" }
        );
    }
}

internal sealed class IdaEstagioMap : IEntityTypeConfiguration<IdaEstagio>
{
    public void Configure(EntityTypeBuilder<IdaEstagio> e)
    {
        e.ToTable("ida_estagio");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("ida_estagio_id").ValueGeneratedOnAdd();
        e.Property(x => x.Nome).HasColumnName("ida_estagio_nome").HasMaxLength(100).IsRequired();
        e.HasIndex(x => x.Nome).IsUnique();

        e.HasData(
            new IdaEstagio { Id = 1, Nome = "Ideação" },
            new IdaEstagio { Id = 2, Nome = "MVP" },
            new IdaEstagio { Id = 3, Nome = "Tração" },
            new IdaEstagio { Id = 4, Nome = "Scale-up" }
        );
    }
}

internal sealed class IdaCategoriaMap : IEntityTypeConfiguration<IdaCategoria>
{
    public void Configure(EntityTypeBuilder<IdaCategoria> e)
    {
        e.ToTable("ida_categoria");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("ida_categoria_id").ValueGeneratedOnAdd();
        e.Property(x => x.Nome).HasColumnName("ida_categoria_nome").HasMaxLength(100).IsRequired();
        e.HasIndex(x => x.Nome).IsUnique();

        e.HasData(
            new IdaCategoria { Id = 1, Nome = "tecnologia" },
            new IdaCategoria { Id = 2, Nome = "Agro" },
            new IdaCategoria { Id = 3, Nome = "inovacao" },
            new IdaCategoria { Id = 4, Nome = "infraestrutura" },
            new IdaCategoria { Id = 5, Nome = "moda" },
            new IdaCategoria { Id = 6, Nome = "automobilismo" },
            new IdaCategoria { Id = 7, Nome = "sustentabilidade" },
            new IdaCategoria { Id = 8, Nome = "Comodidade" },
            new IdaCategoria { Id = 9, Nome = "lazer" },
            new IdaCategoria { Id = 10, Nome = "uso diario" },
            new IdaCategoria { Id = 11, Nome = "Moradia" },
            new IdaCategoria { Id = 12, Nome = "Energia" },
            new IdaCategoria { Id = 13, Nome = "maritimo" },
            new IdaCategoria { Id = 14, Nome = "aeronáutico" },
            new IdaCategoria { Id = 15, Nome = "outros" }
        );
    }
}

internal sealed class IdaIdeiaMap : IEntityTypeConfiguration<IdaIdeia>
{
    public void Configure(EntityTypeBuilder<IdaIdeia> e)
    {
        e.ToTable("ida_ideia");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("ida_id").ValueGeneratedOnAdd();
        e.Property(x => x.UsuarioId).HasColumnName("ida_usuario_id").IsRequired();
        e.Property(x => x.StatusId).HasColumnName("ida_status_id").IsRequired();
        e.Property(x => x.MotivoStatus).HasColumnName("ida_motivo_status").HasMaxLength(2000);
        e.Property(x => x.CategoriaId).HasColumnName("ida_categoria_id").IsRequired();
        e.Property(x => x.EstagioId).HasColumnName("ida_estagio_id").HasDefaultValue(1).IsRequired();
        e.Property(x => x.Nome).HasColumnName("ida_nome").HasMaxLength(200).IsRequired();
        e.Property(x => x.Regiao).HasColumnName("ida_regiao").HasMaxLength(100);
        e.Property(x => x.CreateDate).HasColumnName("ida_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("ida_update_date").IsRequired();

        e.HasIndex(x => x.Nome).IsUnique();
        e.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_ida_ideia_ida_categoria_id");
        e.HasIndex(x => x.EstagioId).HasDatabaseName("ix_ida_ideia_ida_estagio_id");

        e.HasOne(x => x.Usuario)
            .WithMany(u => u.Ideias)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Status)
            .WithMany(s => s.Ideias)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Estagio)
            .WithMany(est => est.Ideias)
            .HasForeignKey(x => x.EstagioId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Categoria)
            .WithMany(c => c.Ideias)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class IdaInfoMap : IEntityTypeConfiguration<IdaInfo>
{
    public void Configure(EntityTypeBuilder<IdaInfo> e)
    {
        e.ToTable("ida_info");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("ida_info_id").ValueGeneratedOnAdd();
        e.Property(x => x.IdeiaId).HasColumnName("ida_ideia_id").IsRequired();
        e.Property(x => x.Cnpj).HasColumnName("ida_info_cnpj").HasMaxLength(30).IsRequired();
        e.Property(x => x.Descricao).HasColumnName("ida_info_descricao").HasMaxLength(4000);
        e.Property(x => x.LinkVideo).HasColumnName("ida_info_link_video").HasMaxLength(2000);
        e.Property(x => x.Imagem).HasColumnName("ida_info_imagem").HasMaxLength(2000);
        e.Property(x => x.Fatia).HasColumnName("ida_info_fatia").HasPrecision(5, 2).IsRequired();
        e.Property(x => x.ValorCaptacao).HasColumnName("ida_info_valor_captacao").HasPrecision(18, 2).HasDefaultValue(0m).IsRequired();
        e.Property(x => x.CreateDate).HasColumnName("ida_info_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("ida_info_update_date").IsRequired();

        e.HasIndex(x => x.Cnpj).IsUnique();
        e.HasIndex(x => x.IdeiaId).IsUnique();
        e.HasOne(x => x.Ideia)
            .WithOne(i => i.Info)
            .HasForeignKey<IdaInfo>(x => x.IdeiaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class IdaComentarioMap : IEntityTypeConfiguration<IdaComentario>
{
    public void Configure(EntityTypeBuilder<IdaComentario> e)
    {
        e.ToTable("ida_comentario");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("ida_comentario_id").ValueGeneratedOnAdd();
        e.Property(x => x.IdeiaId).HasColumnName("ida_comentario_ideia_id").IsRequired();
        e.Property(x => x.UsuarioId).HasColumnName("ida_comentario_usuario_id").IsRequired();
        e.Property(x => x.ParentId).HasColumnName("ida_comentario_parent_id");
        e.Property(x => x.Texto).HasColumnName("ida_comentario_texto").HasMaxLength(4000).IsRequired();
        e.Property(x => x.CreateDate).HasColumnName("ida_comentario_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("ida_comentario_update_date").IsRequired();

        e.HasOne(x => x.Ideia)
            .WithMany(i => i.Comentarios)
            .HasForeignKey(x => x.IdeiaId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Parent)
            .WithMany(p => p.Replies)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class IdaDocumentoMap : IEntityTypeConfiguration<IdaDocumento>
{
    public void Configure(EntityTypeBuilder<IdaDocumento> e)
    {
        e.ToTable("ida_documento");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("ida_documento_id").ValueGeneratedOnAdd();
        e.Property(x => x.IdeiaId).HasColumnName("ida_ideia_id").IsRequired();
        e.Property(x => x.Arquivo).HasColumnName("ida_documento_arquivo").HasMaxLength(1000).IsRequired();

        e.HasOne(x => x.Ideia)
            .WithMany(i => i.Documentos)
            .HasForeignKey(x => x.IdeiaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ChtConversaMap : IEntityTypeConfiguration<ChtConversa>
{
    public void Configure(EntityTypeBuilder<ChtConversa> e)
    {
        e.ToTable("cht_conversa");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("cht_conversa_id").ValueGeneratedOnAdd();
        e.Property(x => x.Usuario1Id).HasColumnName("cht_conversa_usuario1_id").IsRequired();
        e.Property(x => x.Usuario2Id).HasColumnName("cht_conversa_usuario2_id").IsRequired();
        e.Property(x => x.IdeiaId).HasColumnName("cht_conversa_ideia_id");
        e.Property(x => x.CreateDate).HasColumnName("cht_conversa_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("cht_conversa_update_date").IsRequired();

        e.HasOne(x => x.Usuario1)
            .WithMany()
            .HasForeignKey(x => x.Usuario1Id)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Usuario2)
            .WithMany()
            .HasForeignKey(x => x.Usuario2Id)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Ideia)
            .WithMany()
            .HasForeignKey(x => x.IdeiaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class ChtMensagemMap : IEntityTypeConfiguration<ChtMensagem>
{
    public void Configure(EntityTypeBuilder<ChtMensagem> e)
    {
        e.ToTable("cht_mensagem");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("cht_mensagem_id").ValueGeneratedOnAdd();
        e.Property(x => x.ConversaId).HasColumnName("cht_mensagem_conversa_id").IsRequired();
        e.Property(x => x.RemetenteId).HasColumnName("cht_mensagem_remetente_id").IsRequired();
        e.Property(x => x.Texto).HasColumnName("cht_mensagem_texto").HasMaxLength(4000).IsRequired();
        e.Property(x => x.Lida).HasColumnName("cht_mensagem_lida").IsRequired();
        e.Property(x => x.CreateDate).HasColumnName("cht_mensagem_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("cht_mensagem_update_date").IsRequired();

        e.HasOne(x => x.Conversa)
            .WithMany(c => c.Mensagens)
            .HasForeignKey(x => x.ConversaId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Remetente)
            .WithMany()
            .HasForeignKey(x => x.RemetenteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PgtPagamentoMap : IEntityTypeConfiguration<PgtPagamento>
{
    public void Configure(EntityTypeBuilder<PgtPagamento> e)
    {
        e.ToTable("pgt_pagamento");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("pgt_pagamento_id").ValueGeneratedOnAdd();
        e.Property(x => x.UsuarioId).HasColumnName("usu_id").IsRequired();
        e.Property(x => x.Valor).HasColumnName("pgt_pagamento_valor").HasPrecision(18, 2).IsRequired();
        e.Property(x => x.Descricao).HasColumnName("pgt_pagamento_descricao").HasMaxLength(500).IsRequired();
        e.Property(x => x.Metodo).HasColumnName("pgt_pagamento_metodo").HasMaxLength(100).IsRequired();
        e.Property(x => x.Status).HasColumnName("pgt_pagamento_status").HasMaxLength(50).IsRequired();
        e.Property(x => x.CreateDate).HasColumnName("pgt_pagamento_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("pgt_pagamento_update_date").IsRequired();

        e.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class GovDenunciaMap : IEntityTypeConfiguration<GovDenuncia>
{
    public void Configure(EntityTypeBuilder<GovDenuncia> e)
    {
        e.ToTable("gov_denuncia");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("gov_denuncia_id").ValueGeneratedOnAdd();
        e.Property(x => x.DenuncianteId).HasColumnName("denunciante_id").IsRequired();
        e.Property(x => x.TipoAlvo).HasColumnName("gov_denuncia_tipo_alvo").HasMaxLength(50).IsRequired();
        e.Property(x => x.AlvoId).HasColumnName("gov_denuncia_alvo_id").IsRequired();
        e.Property(x => x.Motivo).HasColumnName("gov_denuncia_motivo").HasMaxLength(1000).IsRequired();
        e.Property(x => x.Status).HasColumnName("gov_denuncia_status").HasMaxLength(50).IsRequired();
        e.Property(x => x.ObservacaoAdm).HasColumnName("gov_denuncia_obs_adm").HasMaxLength(2000);
        e.Property(x => x.CreateDate).HasColumnName("gov_denuncia_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("gov_denuncia_update_date").IsRequired();

        e.HasOne(x => x.Denunciante)
            .WithMany()
            .HasForeignKey(x => x.DenuncianteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PrpAceiteMap : IEntityTypeConfiguration<PrpAceite>
{
    public void Configure(EntityTypeBuilder<PrpAceite> e)
    {
        e.ToTable("prp_aceite");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("prp_aceite_id").ValueGeneratedOnAdd();
        e.Property(x => x.Nome).HasColumnName("prp_aceite_nome").HasMaxLength(50).IsRequired();
        e.HasIndex(x => x.Nome).IsUnique();

        e.HasData(
            new PrpAceite { Id = 1, Nome = "aceita" },
            new PrpAceite { Id = 2, Nome = "recusada" },
            new PrpAceite { Id = 3, Nome = "pendente" },
            new PrpAceite { Id = 4, Nome = "contraproposta" }
        );
    }
}

internal sealed class PrpPropostaMap : IEntityTypeConfiguration<PrpProposta>
{
    public void Configure(EntityTypeBuilder<PrpProposta> e)
    {
        e.ToTable("prp_proposta");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("prp_id").ValueGeneratedOnAdd();
        e.Property(x => x.IdeiaId).HasColumnName("prp_ideia_id").IsRequired();
        e.Property(x => x.UsuarioId).HasColumnName("prp_usuario_id").IsRequired();
        e.Property(x => x.Status).HasColumnName("prp_status").IsRequired();
        e.Property(x => x.CreateDate).HasColumnName("prp_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("prp_update_date").IsRequired();

        e.HasOne(x => x.Ideia)
            .WithMany(i => i.Propostas)
            .HasForeignKey(x => x.IdeiaId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Usuario)
            .WithMany(u => u.Propostas)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PrpInfoMap : IEntityTypeConfiguration<PrpInfo>
{
    public void Configure(EntityTypeBuilder<PrpInfo> e)
    {
        e.ToTable("prp_info");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("prp_info_id").ValueGeneratedOnAdd();
        e.Property(x => x.PropostaId).HasColumnName("prp_proposta_id").IsRequired();
        e.Property(x => x.Mensagem).HasColumnName("prp_info_mensagem").HasMaxLength(4000);
        e.Property(x => x.Valor).HasColumnName("prp_info_valor").HasPrecision(18, 2).IsRequired();
        e.Property(x => x.FatiaPret).HasColumnName("prp_info_fatia_pret").HasPrecision(5, 2).IsRequired();
        e.Property(x => x.AceiteId).HasColumnName("prp_aceite_id").IsRequired();
        e.Property(x => x.Retorno).HasColumnName("prp_info_retorno").HasMaxLength(4000);
        e.Property(x => x.CreateDate).HasColumnName("prp_info_create_date").IsRequired();
        e.Property(x => x.UpdateDate).HasColumnName("prp_info_update_date").IsRequired();

        e.HasOne(x => x.Proposta)
            .WithMany(p => p.Infos)
            .HasForeignKey(x => x.PropostaId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Aceite)
            .WithMany(a => a.Infos)
            .HasForeignKey(x => x.AceiteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TrnTipoMap : IEntityTypeConfiguration<TrnTipo>
{
    public void Configure(EntityTypeBuilder<TrnTipo> e)
    {
        e.ToTable("trn_tipo");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("trn_tipo_id").ValueGeneratedOnAdd();
        e.Property(x => x.Nome).HasColumnName("trn_tipo_nome").HasMaxLength(50).IsRequired();
        e.HasIndex(x => x.Nome).IsUnique();

        e.HasData(
            new TrnTipo { Id = 1, Nome = "cadastro" },
            new TrnTipo { Id = 2, Nome = "edição" },
            new TrnTipo { Id = 3, Nome = "proposta" },
            new TrnTipo { Id = 4, Nome = "login" }
        );
    }
}

internal sealed class TrnLogMap : IEntityTypeConfiguration<TrnLog>
{
    public void Configure(EntityTypeBuilder<TrnLog> e)
    {
        e.ToTable("trn_log");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("trn_id").ValueGeneratedOnAdd();
        e.Property(x => x.TipoId).HasColumnName("trn_tipo_id").IsRequired();
        e.Property(x => x.UsuarioId).HasColumnName("trn_usuario_id");
        e.Property(x => x.IdeiaId).HasColumnName("trn_ideia_id");
        e.Property(x => x.PropostaId).HasColumnName("trn_proposta_id");
        e.Property(x => x.CreateDate).HasColumnName("trn_create_date").IsRequired();
        e.Property(x => x.Descricao).HasColumnName("trn_descricao").HasMaxLength(4000).IsRequired();

        e.HasOne(x => x.Tipo)
            .WithMany(t => t.Logs)
            .HasForeignKey(x => x.TipoId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Usuario)
            .WithMany(u => u.Logs)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        e.HasOne(x => x.Ideia)
            .WithMany()
            .HasForeignKey(x => x.IdeiaId)
            .OnDelete(DeleteBehavior.SetNull);

        e.HasOne(x => x.Proposta)
            .WithMany()
            .HasForeignKey(x => x.PropostaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class NtfTipoMap : IEntityTypeConfiguration<NtfTipo>
{
    public void Configure(EntityTypeBuilder<NtfTipo> e)
    {
        e.ToTable("ntf_tipo");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("ntf_tipo_id").ValueGeneratedOnAdd();
        e.Property(x => x.Nome).HasColumnName("ntf_tipo_nome").HasMaxLength(100).IsRequired();
        e.HasIndex(x => x.Nome).IsUnique();

        e.HasData(
            new NtfTipo { Id = 1, Nome = "prp aceita" },
            new NtfTipo { Id = 2, Nome = "prp recusada" },
            new NtfTipo { Id = 3, Nome = "alerta" },
            new NtfTipo { Id = 4, Nome = "n" },
            new NtfTipo { Id = 5, Nome = "prp recebida" },
            new NtfTipo { Id = 6, Nome = "prp contraproposta" }
        );
    }
}

internal sealed class NtfNotificacaoMap : IEntityTypeConfiguration<NtfNotificacao>
{
    public void Configure(EntityTypeBuilder<NtfNotificacao> e)
    {
        e.ToTable("ntf_notificacao");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("ntf_id").ValueGeneratedOnAdd();
        e.Property(x => x.UsuarioId).HasColumnName("ntf_usuario_id").IsRequired();
        e.Property(x => x.TipoId).HasColumnName("ntf_tipo_id").IsRequired();
        e.Property(x => x.Mensagem).HasColumnName("ntf_mensagem").HasMaxLength(4000).IsRequired();
        e.Property(x => x.Lida).HasColumnName("ntf_lida").IsRequired();
        e.Property(x => x.CreateDate).HasColumnName("ntf_create_date").IsRequired();

        e.HasOne(x => x.Usuario)
            .WithMany(u => u.NotificacoesRecebidas)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Tipo)
            .WithMany(t => t.Notificacoes)
            .HasForeignKey(x => x.TipoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
