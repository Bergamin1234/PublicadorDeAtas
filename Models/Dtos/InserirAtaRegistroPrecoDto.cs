using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Dtos
{

    public class ParteEnvolvidaDto
    {
        public int TipoParteEnvolvidaId { get; set; } // 1 para Gerenciador, 2 para Participante
        public string Cnpj { get; set; } = string.Empty;
        public string CodigoUnidadeCompradora { get; set; } = string.Empty;
    }

    public class InserirAtaRegistroPrecoDto
    {
        [Display(Name = "ID de contratação no PNCP")]
        public string? idPNCP { get; set; }

        [Display(Name = "CNPJ do orgão")]
        public string? cnpjOrgao { get; set; }

        [Display(Name = "Ano da contratação")]
        public string? anoCompra { get; set; }

        [Display(Name = "Sequencial da contratação")]
        public string? sequencialCompra { get; set; }

        [Display(Name = "Numero da ata")]
        public string? numeroAta { get; set; }

        [Display(Name = "Ano da ata")]
        public string? anoAta { get; set; }

        [Display(Name = "Data da assinatura")]
        public string? dataAssinatura { get; set; }

        [Display(Name = "Data inicial da vigência")]
        public DateTime dataInicioVigencia { get; set; }

        [Display(Name = "Data final da vigência")]
        public DateTime dataFimVigencia { get; set; }
        
        // --- Campos novos exigidos pelo PNCP v2.4 ---
        [Display(Name = "Possibilidade de Adesão")]
        public bool PossibilidadeAdesao { get; set; }
        
        [Display(Name = "Partes Envolvidas")]
        public List<ParteEnvolvidaDto>? PartesEnvolvidas { get; set; } 
        // --------------------------------------------

        [Display(Name = "Código da Unidade do Órgão")]
        public string? codigoUnidade { get; set; } 
        public string? UsuarioNome { get; set; }

        [Display(Name = "Anexo da ata de registro de preço")]
        public IFormFile? arquivo { get; set; } 
    }
}