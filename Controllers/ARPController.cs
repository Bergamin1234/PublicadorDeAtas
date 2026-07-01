using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebApp.Models.Dtos;
using PublicadorARP.Services.Interfaces;
using PublicadorARP.Models.Dtos;
using System.Text.RegularExpressions;
using System.Net;

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

            // 3. TRAVA DE SEGURANÇA LOCAL: Impede o envio sem o PDF anexado
            if (dto.arquivo == null || dto.arquivo.Length == 0)
            {
                ModelState.AddModelError("arquivo", "O arquivo PDF da Ata de Registro de Preço é obrigatório.");
            }

            // 4. Valida se existem erros nos campos básicos do formulário
            if (!ModelState.IsValid)
            {
                return View(dto); 
            }

            // CORREÇÃO: Bloco de tratamento defensivo para interceptar falhas de negócio e da API
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
                    // Erro retornado mapeado pela própria API do PNCP (Ex: Fornecedor Inexistente, Item incorreto)
                    string erroDetalhado = result.Item1.Content ?? "Nenhum conteúdo de erro retornado pela API.";
                    ModelState.AddModelError(string.Empty, $"Rejeição do PNCP: {erroDetalhado}");
                    return View(dto);
                }
            }
            catch (FormatException fEx)
            {
                // Captura erros de digitação incorreta do ID PNCP ou conversão de inteiros
                ModelState.AddModelError(string.Empty, $"Falha de validação dos dados: {fEx.Message}");
                return View(dto);
            }
            catch (WebException wEx)
            {
                // Captura falhas de autenticação de Token ou queda do barramento do Governo
                ModelState.AddModelError(string.Empty, $"Erro de comunicação/autenticação no PNCP: {wEx.Message}");
                return View(dto);
            }
            catch (Exception ex)
            {
                // Fallback para qualquer outro tipo de erro de infraestrutura local
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