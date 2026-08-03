using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Dtos
{
    public class ParteEnvolvidaDto
    {
        [Required(ErrorMessage = "O tipo da parte envolvida é obrigatório.")]
        public int TipoParteEnvolvidaId { get; set; } // 1 para Gerenciador, 2 para Participante
        
        [Required(ErrorMessage = "O CNPJ da parte envolvida é obrigatório.")]
        public string Cnpj { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O código da unidade compradora é obrigatório.")]
        public string CodigoUnidadeCompradora { get; set; } = string.Empty;
    }

    public class InserirAtaRegistroPrecoDto
    {
        [Required(ErrorMessage = "O ID de contratação no PNCP é obrigatório.")]
        [Display(Name = "ID de contratação no PNCP")]
        public string? idPNCP { get; set; }

        [Display(Name = "CNPJ do orgão")]
        public string? cnpjOrgao { get; set; }

        [Display(Name = "Ano da contratação")]
        public string? anoCompra { get; set; }

        [Display(Name = "Sequencial da contratação")]
        public string? sequencialCompra { get; set; }

        [Required(ErrorMessage = "O número da ata é obrigatório.")]
        [Display(Name = "Numero da ata")]
        public string? numeroAta { get; set; }

        [Required(ErrorMessage = "O ano da ata é obrigatório.")]
        [Display(Name = "Ano da ata")]
        public string? anoAta { get; set; }

        // ALTERADO PARA DATETIME E TORNADO OBRIGATÓRIO PARA EVITAR O ERRO 500
        [Required(ErrorMessage = "A data da assinatura é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data da assinatura")]
        public DateTime? dataAssinatura { get; set; }

        [Required(ErrorMessage = "A data inicial de vigência é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data da classificação inicial da vigência")]
        public DateTime dataInicioVigencia { get; set; }

        [Required(ErrorMessage = "A data final de vigência é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data final da vigência")]
        public DateTime dataFimVigencia { get; set; }
        
        [Display(Name = "Possibilidade de Adesão")]
        public bool PossibilidadeAdesao { get; set; }
        
        [Display(Name = "Partes Envolvidas")]
        public List<ParteEnvolvidaDto>? PartesEnvolvidas { get; set; } 

        [Required(ErrorMessage = "O código da unidade do órgão é obrigatório.")]
        [Display(Name = "Código da Unidade do Órgão")]
        public string? codigoUnidade { get; set; } 
        
        public string? UsuarioNome { get; set; }

        [Required(ErrorMessage = "O arquivo PDF físico da ata é obrigatório.")]
        [Display(Name = "Anexo da ata de registro de preço")]
        public IFormFile? arquivo { get; set; } 
    }
}