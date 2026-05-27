namespace PublicadorARP.Models.ViewModels
{
    public class TipoInstrumentoConvocatorioViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public bool StatusAtivo { get; set; }
        public object JustificativaAtualizacao { get; set; }
        public ObrigatoriedadeViewModel ObrigatoriedadeDataAberturaProposta { get; set; }
        public ObrigatoriedadeViewModel ObrigatoriedadeDataEncerramentoProposta { get; set; }
    }
}
