namespace PublicadorARP.Models.ViewModels
{
    public class AtaViewModel
    {
        public int Id { get; set; }
        public CompraViewModel Compra { get; set; }
        public string NumeroAtaRegistroPreco { get; set; }
        public int AnoAta { get; set; }
        public DateTime DataPublicacaoPncp { get; set; }
        public DateTime DataAssinatura { get; set; }
        public bool Cancelado { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public DateTime DataVigenciaInicio { get; set; }
        public DateTime DataVigenciaFim { get; set; }
        public int SequencialAta { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public bool Excluido { get; set; }
        public int AtributoControle { get; set; }
    }
}
