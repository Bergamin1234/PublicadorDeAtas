namespace PublicadorARP.Models.ViewModels
{
    public class CompraViewModel
    {
        public int Id { get; set; }
        public ModalidadeViewModel Modalidade { get; set; }
        public string NumeroCompra { get; set; }
        public int AnoCompra { get; set; }
        public string Processo { get; set; }
        public TipoInstrumentoConvocatorioViewModel TipoInstrumentoConvocatorio { get; set; }
        public int SituacaoCompra { get; set; }
        public string ObjetoCompra { get; set; }
        public string InformacaoComplementar { get; set; }
        public bool Srp { get; set; }
        public DateTime DataAberturaProposta { get; set; }
        public DateTime DataEncerramentoProposta { get; set; }
        public DateTime DataPublicacaoPncp { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public int SequencialCompra { get; set; }
        public OrgaoEntidadeViewModel OrgaoEntidade { get; set; }
        public UnidadeOrgaoViewModel UnidadeOrgao { get; set; }
        public object OrgaoSubRogado { get; set; }
        public object UnidadeSubRogada { get; set; }
        public AmparoLegalViewModel AmparoLegal { get; set; }
        public ModoDisputaViewModel ModoDisputa { get; set; }
        public string LinkSistemaOrigem { get; set; }
        public bool Excluido { get; set; }
        public int AtributoControle { get; set; }
        public string JustificativaPresencial { get; set; }
        public bool ExisteResultado { get; set; }
        public string NumeroControle { get; set; }
    }
}
