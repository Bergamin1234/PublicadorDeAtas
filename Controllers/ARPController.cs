using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebApp.Models.Dtos;
using PublicadorARP.Services.Interfaces;
using PublicadorARP.Models.Dtos;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
    [Authorize]
    public class ARPController : Controller
    {
        private readonly IPNCPService _pncpService;
        private readonly ILogger<ARPController> _logger;

        public ARPController(IPNCPService pncpService, ILogger<ARPController> logger)
        {
            _pncpService = pncpService;
            _logger = logger;
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
            dto.UsuarioNome = User.Identity?.Name ?? "Usuario do Sistema";
            ModelState.Remove("UsuarioNome");

            if (dto.arquivo == null || dto.arquivo.Length == 0)
            {
                ModelState.AddModelError("arquivo", "O arquivo PDF da Ata de Registro de Preço é obrigatório.");
            }

            if (!ModelState.IsValid)
            {
                return View(dto); 
            }

            try
            {
                var result = await _pncpService.InserirAtaRegistroPreco(dto);

                if (result.Item1.IsSuccessful)
                {
                    var url = result.Item2;
                    if (!string.IsNullOrEmpty(url))
                    {
                        url = url.Replace("/compras", "");
                        url = url.Replace("/atas", "");
                        string novaUrl = Regex.Replace(url, @"https://pncp.gov.br/pncp-api/v1/orgaos", "https://pncp.gov.br/app/atas");
                        ViewBag.UrlPNCP = novaUrl;
                    }
                    return View("ARPSucess");
                }
                else
                {
                    string erroDetalhado = result.Item1.Content ?? "Nenhum conteúdo de erro retornado pela API.";
                    ModelState.AddModelError(string.Empty, $"Rejeição do PNCP: {erroDetalhado}");
                    return View(dto);
                }
            }
            catch (FormatException fEx)
            {
                ModelState.AddModelError(string.Empty, $"Falha de validação dos dados: {fEx.Message}");
                return View(dto);
            }
            catch (WebException wEx)
            {
                ModelState.AddModelError(string.Empty, $"Erro de comunicação/autenticação no PNCP: {wEx.Message}");
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro interno inesperado: {ex.Message}");
                return View(dto);
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
            // Validação defensiva L3 para interceptar parâmetros perdidos pós-login
            if (string.IsNullOrEmpty(sequencialAta) || string.IsNullOrEmpty(sequencialCompra) || string.IsNullOrEmpty(anoCompra))
            {
                _logger.LogWarning("Parâmetros de rota nulos ao acessar GerenciarAtaRegistroPreco.");
                
                var erroViewModel = new PublicadorDeAtas.Models.ErrorViewModel 
                { 
                    RequestId = $"ERRO_PARAMETROS_NULOS - Trace: {HttpContext.TraceIdentifier}" 
                };
                
                ViewBag.MensagemCustomizada = "Os parâmetros de identificação da ata foram perdidos na requisição.";
                return View("Error", erroViewModel);
            }

            var consultaAtaDto = new ConsultarAtaRegistroPrecoDto()
            {
                anoCompra = anoCompra,
                sequencialAta = sequencialAta,
                sequencialCompra = sequencialCompra
            };

            try
            {
                var ataRegistroPrecoViewModel = await _pncpService.ConsultarAtaRegistroPreco(consultaAtaDto);

                if (ataRegistroPrecoViewModel == null)
                {
                    _logger.LogWarning($"Ata não localizada no PNCP.");
                    return View("Error", new PublicadorDeAtas.Models.ErrorViewModel { RequestId = "ATA_NAO_ENCONTRADA_NO_PNCP" });
                }

                return View("GerenciarAtaRegistroPreco", ataRegistroPrecoViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro fatal na Action de Gerenciamento da Ata: {ex.Message}");
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Content($"[Diagnóstico SUPEL] Erro na execução interna: {ex.Message} \n\nDetalhes da StackTrace: {ex.StackTrace}");
            }
        }
    }
}