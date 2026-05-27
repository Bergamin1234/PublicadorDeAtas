using System.ComponentModel.DataAnnotations;

namespace PublicadorARP.Models.Dtos
{
    public class ConsultarContratacaoDto
    {
        [Display(Name = "Ano da contratação")]
        public string anoCompra { get; set; }
        [Display(Name = "Sequencial da contratação")]
        public string sequencialCompra { get; set; }
    }
}
