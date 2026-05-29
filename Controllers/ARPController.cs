using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models.Dtos;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using RestSharp;
using PublicadorARP.Services.Interfaces;
using PublicadorARP.Models.Dtos;
using PublicadorARP.Models.ViewModels;
using System.Text.RegularExpressions;

namespace WebApp.Controllers
{
    [Authorize]
    public class ARPController : Controller
    {
        private readonly IPNCPService _pncpService;

        public ARPController(IPNCPService pncpService)
        {
            _pncpService = pncpService;
        }

        [Authorize]
        public IActionResult InserirAtaRegistroPreco()
        {
            return View();
        }

        public IActionResult AnexarArquivo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InserirAtaRegistroPreco(InserirAtaRegistroPrecoDto dto)
        {
            // 1. Captura o nome/documento do usuário logado no sistema e injeta no DTO
            dto.UsuarioNome = User.Identity?.Name ?? "Usuario do Sistema";

            // 2. Avisa ao C# para ignorar a validação da tela para este campo específico
            ModelState.Remove("UsuarioNome");

            // 3. Valida se existem OUTROS erros nos demais campos
            if (!ModelState.IsValid)
            {
                // Mantemos o BadRequest por enquanto para caçar qualquer outro erro oculto
                return BadRequest(ModelState); 
            }

            var result = await _pncpService.InserirAtaRegistroPreco(dto);

            if (result.Item1.IsSuccessful)
            {
                var url = result.Item2;
                url = url.Replace("/compras", "");
                url = url.Replace("/atas", "");
                string novaUrl = Regex.Replace(url, @"https://pncp.gov.br/pncp-api/v1/orgaos", "https://pncp.gov.br/app/atas");
                ViewBag.UrlPNCP = novaUrl;
                return View("ARPSucess");
            }
            else
            {
                return View("ARPFail");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConsultarAtasPorContratacao(ConsultarContratacaoDto dto)
        {
            var contratacaoViewModel = await _pncpService.ConsultarContratacaoComAtasDeRegistroDePreco(dto);

            if (contratacaoViewModel != null)
            {
                return View("ResultadoConsultaAtasPorContratacao", contratacaoViewModel);
            }
            else
            {
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GerenciarAtaRegistroPreco(string sequencialAta, string sequencialCompra, string anoCompra)
        {
            var consultaAtaDto = new ConsultarAtaRegistroPrecoDto()
            {
                anoCompra = anoCompra,
                sequencialAta = sequencialAta,
                sequencialCompra = sequencialCompra
            };

            var ataRegistroPrecoViewModel = await _pncpService.ConsultarAtaRegistroPreco(consultaAtaDto);

            return View("GerenciarAtaRegistroPreco", ataRegistroPrecoViewModel);
        }
    }
}