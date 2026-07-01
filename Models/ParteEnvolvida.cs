using System;

namespace PublicadorDeAtas.Models
{
    public class ParteEnvolvida
    {
        public int Id { get; set; }
        public int TipoParteEnvolvidaId { get; set; } 
        public string Cnpj { get; set; } = string.Empty;
        public string CodigoUnidadeCompradora { get; set; } = string.Empty;
        
        public int AtaId { get; set; }
        public virtual AtaRegistroPreco Ata { get; set; } = null!;
    }
}