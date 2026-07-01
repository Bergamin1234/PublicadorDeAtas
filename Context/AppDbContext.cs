using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PublicadorDeAtas.Models;

namespace PublicadorDeAtas.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tabelas de negócio explicitadas para o compilador do PostgreSQL
        public DbSet<AtaRegistroPreco> AtasRegistroPreco { get; set; }
        public DbSet<ParteEnvolvida> PartesEnvolvidas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Inicializa as tabelas nativas de usuários e acessos do Identity
            base.OnModelCreating(builder);
            
            // Isola as tabelas do publicador dentro de um Schema próprio no PostgreSQL
            builder.HasDefaultSchema("PublicadorARP");

            // Configuração Fluente da Ata de Registro de Preço
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
                entity.Property(e => e.UsuarioNome).HasMaxLength(150);

                entity.HasIndex(e => new { e.CnpjOrgao, e.AnoCompra, e.SequencialCompra })
                      .HasDatabaseName("IX_AtaRegistroPreco_FiltroPNCP");
            });

            // Configuração Fluente das Partes Envolvidas (Órgãos Gerenciadores/Participantes)
            builder.Entity<ParteEnvolvida>(entity =>
            {
                entity.ToTable("ParteEnvolvida");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Cnpj).HasMaxLength(14).IsRequired();
                entity.Property(e => e.CodigoUnidadeCompradora).HasMaxLength(30).IsRequired();

                // Relacionamento 1:N com exclusão lógica em cascata
                entity.HasOne(p => p.Ata)
                      .WithMany(a => a.PartesEnvolvidas)
                      .HasForeignKey(p => p.AtaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}