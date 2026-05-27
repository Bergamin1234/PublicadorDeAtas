using System.ComponentModel.DataAnnotations;

namespace PublicadorARP.Models.ViewModels
{
    public class AtaRegistroPrecoViewModel
    {
        public string NumeroAtaRegistroPreco { get; set; }
        public int AnoAta { get; set; }
        public DateTime DataAssinatura { get; set; }
        public DateTime DataVigenciaInicio { get; set; }
        public DateTime DataVigenciaFim { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public bool Cancelado { get; set; }
        public DateTime DataPublicacaoPncp { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public int SequencialAta { get; set; }
        public string NumeroControlePNCP { get; set; }
        public AtaViewModel Ata { get; set; }
        public OrgaoEntidadeViewModel OrgaoEntidade { get; set; }
        public object OrgaoSubRogado { get; set; }
        public UnidadeOrgaoViewModel UnidadeOrgao { get; set; }
        public object UnidadeSubRogada { get; set; }
        public string ModalidadeNome { get; set; }
        public string ObjetoCompra { get; set; }
        public string InformacaoComplementarCompra { get; set; }
        public string UsuarioNome { get; set; }
    }

    public class AtaRegistroPrecoListViewModel
    {
        public List<AtaRegistroPrecoViewModel> Data { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public int NumeroPagina { get; set; }
        public int PaginasRestantes { get; set; }
        public bool Empty { get; set; }
    }
}
