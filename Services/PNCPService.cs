using PublicadorARP.Models.Dtos;
using PublicadorARP.Models.ViewModels;
using PublicadorARP.Services.Interfaces;
using RestSharp;
using System.Net;
using System.Text.RegularExpressions;
using System.Text.Json;
using WebApp.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PublicadorDeAtas.Models; // CORREÇÃO: Adicionado para encontrar o AlterarAtaRegistroPrecoDto

namespace PublicadorARP.Services
{
    public class PNCPService : IPNCPService
    {
        private readonly RestClient _client;
        private readonly IConfiguration _configuration;

        public PNCPService(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            
            var options = new RestClientOptions
            {
                BaseUrl = new Uri(_configuration["ApiPNCP:Route"] ?? throw new ArgumentNullException("ApiPNCP:Route não configurada."))
            };
            _client = new RestClient(httpClient, options);
        }

        public async Task<ContratacaoViewModel?> ConsultarContratacao(ConsultarContratacaoDto dto)
        {
            try
            {
                string cnpj = !string.IsNullOrEmpty(dto.cnpjOrgao) ? dto.cnpjOrgao : "04801221000110";

                var request = new RestRequest($"orgaos/{cnpj}/compras/{dto.anoCompra}/{dto.sequencialCompra}", Method.Get);
                var response = await _client.ExecuteAsync<ContratacaoViewModel>(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response.Data;
                }
                
                Console.WriteLine($"Erro na requisição de consulta de contratação: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na requisição de consulta de contratação: {ex.Message}");
                return null;
            }
        }

        public async Task<IList<AtaRegistroPrecoViewModel>?> ConsultarAtasPorContratacao(ConsultarContratacaoDto dto)
        {
            try
            {
                string cnpj = !string.IsNullOrEmpty(dto.cnpjOrgao) ? dto.cnpjOrgao : "04801221000110";

                var request = new RestRequest($"orgaos/{cnpj}/compras/{dto.anoCompra}/{dto.sequencialCompra}/atas", Method.Get);
                var response = await _client.ExecuteAsync<AtaRegistroPrecoListViewModel>(request);

                if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
                {
                    return response.Data.Data;
                }
                
                Console.WriteLine($"Erro na requisição de consulta de atas: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na requisição de consulta de atas: {ex.Message}");
                return null;
            }
        }

        public async Task<ContratacaoViewModel?> ConsultarContratacaoComAtasDeRegistroDePreco(ConsultarContratacaoDto dto)
        {
            try
            {
                var contratacao = await ConsultarContratacao(dto);
                if (contratacao == null)
                {
                    Console.WriteLine($"Erro ao unificar consulta: Contratação não encontrada.");
                    return null;
                }

                var ataListViewModel = await ConsultarAtasPorContratacao(dto);
                contratacao.AtasRegistroPrecoList = ataListViewModel;
                
                return contratacao;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na unificação de consultas: {ex.Message}");
                return null;
            }
        }

        public async Task<AtaRegistroPrecoViewModel?> ConsultarAtaRegistroPreco(ConsultarAtaRegistroPrecoDto dto)
        {
            try
            {
                string cnpj = !string.IsNullOrEmpty(dto.cnpjOrgao) ? dto.cnpjOrgao : "04801221000110";

                var request = new RestRequest($"orgaos/{cnpj}/compras/{dto.anoCompra}/{dto.sequencialCompra}/atas/{dto.sequencialAta}", Method.Get);
                var response = await _client.ExecuteAsync<AtaRegistroPrecoViewModel>(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response.Data;
                }
                
                Console.WriteLine($"Erro na requisição de consulta detalhada da ata: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na requisição de consulta detalhada da ata: {ex.Message}");
                return null;
            }
        }

        public async Task<(RestResponse, string)> InserirAtaRegistroPreco(InserirAtaRegistroPrecoDto dto)
        {
            try
            {
                if (dto.arquivo == null)
                {
                    throw new ArgumentException("O arquivo digital da Ata é obrigatório para envio ao PNCP.");
                }

                string pattern = @"(\d{14})-(\d)-(\d{6})/(\d{4})";
                string idLimpo = dto.idPNCP?.Trim() ?? "";

                Match match = Regex.Match(idLimpo, pattern);
                if (match.Success)
                {
                    dto.cnpjOrgao = match.Groups[1].Value;
                    dto.sequencialCompra = match.Groups[3].Value;
                    dto.anoCompra = match.Groups[4].Value;
                }
                else
                {
                    throw new FormatException("O ID do PNCP informado não corresponde ao padrão esperado (CNPJ-MODALIDADE-SEQUENCIAL/ANO).");
                }

                var request = new RestRequest($"orgaos/{dto.cnpjOrgao}/compras/{dto.anoCompra}/{dto.sequencialCompra}/atas", Method.Post);

                string login = _configuration["AuthPNCP:Login"] ?? throw new InvalidOperationException("Login do PNCP não configurado.");
                string senha = _configuration["AuthPNCP:Senha"] ?? throw new InvalidOperationException("Senha do PNCP não configurada.");
                
                var token = await LoginAndGetTokenAsync(login, senha);
                if (string.IsNullOrEmpty(token))
                {
                    throw new WebException("Falha na autenticação preventiva junto ao portal PNCP. Token nulo.");
                }

                request.AddHeader("Authorization", $"Bearer {token}");

                string nomeArquivo = $"ATA_REGISTRO_PRECO_No_{dto.numeroAta}_{dto.anoAta}";
                request.AddHeader("Titulo-Documento", nomeArquivo);
                request.AddHeader("Tipo-Documento-Id", "11"); 

                var requestData = new
                {
                    numeroAtaRegistroPreco = dto.numeroAta,
                    anoAta = Convert.ToInt32(dto.anoAta),
                    dataAssinatura = DateTime.TryParse(dto.dataAssinatura, out var dtAssinatura) ? dtAssinatura.ToString("yyyy-MM-dd") : dto.dataAssinatura,
                    dataVigenciaInicio = dto.dataInicioVigencia.ToString("yyyy-MM-dd"),
                    dataVigenciaFim = dto.dataFimVigencia.ToString("yyyy-MM-dd"),
                    possibilidadeAdesao = dto.PossibilidadeAdesao, 
                    codigoUnidade = dto.codigoUnidade,              
                    partesEnvolvidas = dto.PartesEnvolvidas?.Select(p => new {
                        tipoParteEnvolvidaId = p.TipoParteEnvolvidaId,
                        cnpj = p.Cnpj,
                        codigoUnidadeCompradora = p.CodigoUnidadeCompradora
                    }).ToList()
                };

                string jsonString = JsonSerializer.Serialize(requestData);
                
                request.AddParameter("ata", jsonString, ParameterType.RequestBody);

                var arquivoBytes = ConvertIFormFileToByteArray(dto.arquivo);
                request.AddFile("documento", arquivoBytes, dto.arquivo.FileName, "application/pdf");

                var response = await _client.ExecuteAsync(request);
                string location = "";

                if (response.IsSuccessful)
                {
                    // CORREÇÃO WARNING (Linha 91): Uso de checagem condicional de nulidade protegida
                    location = response.Headers?.FirstOrDefault(h => h.Name.Equals("Location", StringComparison.OrdinalIgnoreCase))?.Value?.ToString() ?? "";
                    Console.WriteLine($"Ata publicada com sucesso em lote único. Status: {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine($"Failed to submit form: {response.StatusCode}");
                    Console.WriteLine($"Retorno detalhado do PNCP: {response.Content}");
                }
                
                return (response, location);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting form: {ex.Message}");
                throw;
            }
        }

        public async Task<RestResponse> AlterarAtaRegistroPreco(AlterarAtaRegistroPrecoDto dto)
        {
            try
            {
                var request = new RestRequest($"orgaos/{dto.cnpjOrgao}/compras/{dto.anoCompra}/" +
                    $"{dto.sequencialCompra}/atas/{dto.sequencialAta}", Method.Put);

                string login = _configuration["AuthPNCP:Login"] ?? throw new InvalidOperationException("Login do PNCP não configurado.");
                string senha = _configuration["AuthPNCP:Senha"] ?? throw new InvalidOperationException("Senha do PNCP não configurada.");
                
                var token = await LoginAndGetTokenAsync(login, senha);
                if (string.IsNullOrEmpty(token))
                {
                    throw new WebException("Falha na autenticação preventiva junto ao portal PNCP. Token nulo.");
                }

                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("Content-Type", "application/json");

                var payloadAlteracao = new
                {
                    numeroAta = dto.NumeroAta,
                    anoAta = dto.AnoAta,
                    dataAssinatura = DateTime.TryParse(dto.DataAssinatura, out var dtAssinatura) ? dtAssinatura.ToString("yyyy-MM-dd") : dto.DataAssinatura,
                    dataVigenciaInicio = DateTime.TryParse(dto.DataVigenciaInicio, out var dtInicio) ? dtInicio.ToString("yyyy-MM-dd") : dto.DataVigenciaInicio,
                    dataVigenciaFim = DateTime.TryParse(dto.DataVigenciaFim, out var dtFim) ? dtFim.ToString("yyyy-MM-dd") : dto.DataVigenciaFim,
                    objeto = dto.Objeto,
                    justificativaAlteracao = dto.JustificativaAlteracao
                };

                request.AddJsonBody(payloadAlteracao);

                var response = await _client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"Failed to alter form: {response.StatusCode}");
                    // CORREÇÃO WARNING (Linha 198): Fallback para string vazia caso Content venha nulo
                    Console.WriteLine($"Retorno detalhado do PNCP na alteração: {response.Content ?? string.Empty}");
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error altering form: {ex.Message}");
                throw;
            }
        }

        private async Task<string?> LoginAndGetTokenAsync(string login, string senha)
        {
            try
            {
                var request = new RestRequest("usuarios/login", Method.Post);
                request.AddHeader("Content-Type", "application/json");

                var requestData = new { login, senha };
                request.AddJsonBody(requestData);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful && response.Headers != null)
                {
                    var authHeader = response.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        return authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
                    }
                }
                
                Console.WriteLine($"Failed to login: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging in: {ex.Message}");
                return null;
            }
        }

        private byte[] ConvertIFormFileToByteArray(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}