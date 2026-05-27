namespace PublicadorARP.Models.ViewModels
{
    public class ObrigatoriedadeViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public object Descricao { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }
}
