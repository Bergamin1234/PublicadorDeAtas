namespace PublicadorARP.Models.ViewModels
{
    public class ModoDisputaViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public object Descricao { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public bool StatusAtivo { get; set; }
        public object JustificativaAtualizacao { get; set; }
    }
}
