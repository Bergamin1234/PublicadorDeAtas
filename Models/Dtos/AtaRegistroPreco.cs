using System;
using System.Collections.Generic;

namespace PublicadorDeAtas.Models
{
    public class AtaRegistroPreco
    {
        public int Id { get; set; }
        public string IdPncp { get; set; } = string.Empty;
        public string CnpjOrgao { get; set; } = string.Empty;
        public string AnoCompra { get; set; } = string.Empty;
        public string SequencialCompra { get; set; } = string.Empty;
        public string NumeroAta { get; set; } = string.Empty;
        public string AnoAta { get; set; } = string.Empty;
        public DateTime DataAssinatura { get; set; }
        public DateTime DataInicioVigencia { get; set; }
        public DateTime DataFimVigencia { get; set; }
        public bool PossibilidadeAdesao { get; set; }
        public string CodigoUnidade { get; set; } = string.Empty;
        public string UsuarioNome { get; set; } = string.Empty;
        public DateTime DataCadastroLocal { get; set; } = DateTime.UtcNow;

        // Corrigido o namespace interno para localizar a coleção perfeitamente
        public virtual ICollection<ParteEnvolvida> PartesEnvolvidas { get; set; } = new List<ParteEnvolvida>();
    }
}