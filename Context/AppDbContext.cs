using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PublicadorDeAtas.Models;
using WebApp.Models;

namespace PublicadorDeAtas.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<AtaRegistroPreco> AtasRegistroPreco { get; set; }
        public DbSet<ParteEnvolvida> PartesEnvolvidas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Executa primeiro os mapeamentos do Identity (Crucial para o Identity funcionar)
            base.OnModelCreating(builder);
            
            // Define o Schema do banco no PostgreSQL
            builder.HasDefaultSchema("PublicadorARP");

            // Mapeamento da Entidade AtaRegistroPreco
            builder.Entity<AtaRegistroPreco>(entity =>
            {
                entity.ToTable("AtaRegistroPreco");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.IdPncp).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CnpjOrgao).HasMaxLength(14).IsRequired();
                entity.Property(e => e.AnoCompra).HasMaxLength(4).IsRequired();
                entity.Property(e => e.SequencialCompra).HasMaxLength(10).IsRequired();
                entity.Property(e => e.NumeroAta).HasMaxLength(20).IsRequired();
                entity.Property(e => e.AnoAta).HasMaxLength(4).IsRequired();
                entity.Property(e => e.CodigoUnidade).HasMaxLength(30).IsRequired();
                entity.Property(e => e.UsuarioNome).HasMaxLength(150).IsRequired();

                entity.Property(e => e.DataAssinatura).IsRequired();
                entity.Property(e => e.DataInicioVigencia).IsRequired();
                entity.Property(e => e.DataFimVigencia).IsRequired();
                entity.Property(e => e.PossibilidadeAdesao).IsRequired();
                entity.Property(e => e.DataCadastroLocal).IsRequired();

                // Índices de performance para otimizar buscas locais idênticas às da API do PNCP
                entity.HasIndex(e => new { e.CnpjOrgao, e.AnoCompra, e.SequencialCompra })
                      .HasDatabaseName("IX_AtaRegistroPreco_BuscaPNCP");
            });

            // Mapeamento da Entidade ParteEnvolvida
            builder.Entity<ParteEnvolvida>(entity =>
            {
                entity.ToTable("ParteEnvolvida");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Cnpj).HasMaxLength(14).IsRequired();
                entity.Property(e => e.CodigoUnidadeCompradora).HasMaxLength(30).IsRequired();
                entity.Property(e => e.TipoParteEnvolvidaId).IsRequired();

                // Configuração da Chave Estrangeira com a Ata (1 para Muitos)
                entity.HasOne(p => p.Ata)
                      .WithMany(a => a.PartesEnvolvidas)
                      .HasForeignKey(p => p.AtaId)
                      .OnDelete(DeleteBehavior.Cascade); // Se deletar a Ata, remove os vínculos de partes envolvidas
            });
        }
    }
}