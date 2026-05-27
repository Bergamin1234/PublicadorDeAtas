using System.ComponentModel.DataAnnotations;

namespace PublicadorARP.Models.Dtos
{
    public class ConsultarAtaRegistroPrecoDto
    {
        [Display(Name = "Ano da contratação")]
        public string anoCompra { get; set; }
        [Display(Name = "Sequencial da contratação")]
        public string sequencialCompra { get; set; }
        [Display(Name = "Sequencial da ata de registro de preço")]
        public string sequencialAta { get; set; }
    }
}
