using System.ComponentModel.DataAnnotations;

namespace PublicadorARP.Models.ViewModels
{
    public class ContratacaoViewModel
    {
        public decimal ValorTotalEstimado { get; set; }
        public decimal? ValorTotalHomologado { get; set; }
        public int ModoDisputaId { get; set; }
        public int ModalidadeId { get; set; }
        public bool Srp { get; set; }
        public int AnoCompra { get; set; }
        public int SequencialCompra { get; set; }
        public OrgaoEntidadeViewModel OrgaoEntidade { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public DateTime DataPublicacaoPncp { get; set; }
        public object OrgaoSubRogado { get; set; }
        public UnidadeOrgaoViewModel UnidadeOrgao { get; set; }
        public object UnidadeSubRogada { get; set; }
        public DateTime DataInclusao { get; set; }
        public string NumeroCompra { get; set; }
        public AmparoLegalViewModel AmparoLegal { get; set; }
        public DateTime DataAberturaProposta { get; set; }
        public DateTime DataEncerramentoProposta { get; set; }
        public string InformacaoComplementar { get; set; }
        public string Processo { get; set; }
        public string ObjetoCompra { get; set; }
        public string LinkSistemaOrigem { get; set; }
        public string JustificativaPresencial { get; set; }
        public bool ExisteResultado { get; set; }
        public string NumeroControlePNCP { get; set; }
        public string SituacaoCompraNome { get; set; }
        public int TipoInstrumentoConvocatorioCodigo { get; set; }
        public string TipoInstrumentoConvocatorioNome { get; set; }
        public string ModoDisputaNome { get; set; }
        public string UsuarioNome { get; set; }
        public string ModalidadeNome { get; set; }
        public int OrcamentoSigilosoCodigo { get; set; }
        public string OrcamentoSigilosoDescricao { get; set; }
        public int SituacaoCompraId { get; set; }
        public IList<AtaRegistroPrecoViewModel> AtasRegistroPrecoList { get; set; }
    }
}
