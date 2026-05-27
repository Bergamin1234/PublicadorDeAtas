namespace PublicadorDeAtas.Models
{
    public class AlterarAtaRegistroPrecoDto
    {
        // Propriedades usadas na URL
        public string cnpjOrgao { get; set; }
        public int anoCompra { get; set; }
        public int sequencialCompra { get; set; }
        public int sequencialAta { get; set; } 
        
        // Propriedades do corpo do JSON (pode manter as que já estavam)
        public string NumeroAta { get; set; }
        public int AnoAta { get; set; }
        public string DataAssinatura { get; set; }
        public string DataVigenciaInicio { get; set; }
        public string DataVigenciaFim { get; set; }
        public string Objeto { get; set; }
        public string JustificativaAlteracao { get; set; }
    }
}