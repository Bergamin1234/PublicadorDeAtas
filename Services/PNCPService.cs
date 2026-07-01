using PublicadorARP.Models.Dtos;
using PublicadorARP.Models.ViewModels;
using PublicadorARP.Services.Interfaces;
using RestSharp;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using WebApp.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PublicadorDeAtas.Context; // Traz o banco de dados da SUPEL para o arquivo

namespace PublicadorARP.Services
{
    public class PNCPService : IPNCPService
    {
        private readonly RestClient _client; // Cliente do RestSharp configurado
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // O construtor recebe o HttpClient do .NET (com a URL base do governo) e o Contexto da SUPEL
        public PNCPService(HttpClient httpClient, AppDbContext context, IConfiguration configuration)
        {
            _client = new RestClient(httpClient);
            _context = context;
            _configuration = configuration;
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
                if (contratacao == null) return null;

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
                    throw new ArgumentException("O arquivo digital em PDF é obrigatório.");
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
                    throw new FormatException("O ID do PNCP informado não corresponde ao padrão esperado.");
                }

                var request = new RestRequest($"orgaos/{dto.cnpjOrgao}/compras/{dto.anoCompra}/{dto.sequencialCompra}/atas", Method.Post);

                string login = _configuration["AuthPNCP:Login"] ?? throw new InvalidOperationException("Login não configurado.");
                string senha = _configuration["AuthPNCP:Senha"] ?? throw new InvalidOperationException("Senha não configurada.");
                
                var token = await LoginAndGetTokenAsync(login, senha);
                request.AddHeader("Authorization", $"Bearer {token}");

                string nomeArquivo = $"ATA_REGISTRO_PRECO_No_{dto.numeroAta}_{dto.anoAta}";
                request.AddHeader("Titulo-Documento", nomeArquivo);
                request.AddHeader("Tipo-Documento-Id", "11"); 

                var requestData = new
                {
                    numeroAtaRegistroPreco = dto.numeroAta,
                    anoAta = Convert.ToInt32(dto.anoAta),
                    dataAssinatura = dto.dataAssinatura,
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

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                string jsonString = JsonSerializer.Serialize(requestData, jsonOptions);
                
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                request.AddFile("ata", jsonBytes, "ata.json", "application/json");

                var arquivoBytes = ConvertIFormFileToByteArray(dto.arquivo);
                request.AddFile("documento", arquivoBytes, dto.arquivo.FileName, "application/pdf");

                var response = await _client.ExecuteAsync(request);
                string location = "";

                if (response.IsSuccessful)
                {
                    var locationHeader = response.Headers?.FirstOrDefault(h => h.Name.Equals("Location", StringComparison.OrdinalIgnoreCase));
                    location = locationHeader?.Value?.ToString() ?? string.Empty;
                    Console.WriteLine($"Ata publicada com sucesso em lote único. Status: {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine($"Failed to submit form. HTTP Status: {(int)response.StatusCode}");
                    Console.WriteLine($"Status de Resposta do RestSharp: {response.ResponseStatus}");
                    Console.WriteLine($"Mensagem de Erro de Rede: {response.ErrorMessage ?? "Nenhuma mensagem de rede"}");
                    Console.WriteLine($"Exceção de Conexão: {response.ErrorException?.Message ?? "Nenhuma exceção lançada"}");
                    Console.WriteLine($"Retorno bruto do PNCP: {response.Content ?? string.Empty}");
                }
                
                return (response, location);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting form: {ex.Message}");
                throw;
            }
        }

        public async Task<RestResponse> AlterarAtaRegistroPreco(PublicadorDeAtas.Models.AlterarAtaRegistroPrecoDto dto)
        {
            try
            {
                var request = new RestRequest($"orgaos/{dto.cnpjOrgao}/compras/{dto.anoCompra}/{dto.sequencialCompra}/atas/{dto.sequencialAta}", Method.Put);

                string login = _configuration["AuthPNCP:Login"] ?? throw new InvalidOperationException("Login não configurado.");
                string senha = _configuration["AuthPNCP:Senha"] ?? throw new InvalidOperationException("Senha não configurada.");
                
                var token = await LoginAndGetTokenAsync(login, senha);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("Content-Type", "application/json");

                var payloadAlteracao = new
                {
                    numeroAta = dto.NumeroAta,
                    anoAta = dto.AnoAta,
                    dataAssinatura = dto.DataAssinatura,
                    dataVigenciaInicio = dto.DataVigenciaInicio,
                    dataVigenciaFim = dto.DataVigenciaFim,
                    objeto = dto.Objeto,
                    justificativaAlteracao = dto.JustificativaAlteracao
                };

                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                string jsonString = JsonSerializer.Serialize(payloadAlteracao, jsonOptions);
                request.AddJsonBody(jsonString);

                var response = await _client.ExecuteAsync(request);
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
                    var authHeader = response.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase));
                    if (authHeader?.Value != null)
                    {
                        string tokenStr = authHeader.Value.ToString() ?? string.Empty;
                        if (tokenStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            tokenStr = tokenStr.Substring(7);
                        }
                        return tokenStr.Trim();
                    }
                }

                // LOG DE PROTEÇÃO: Registra se a falha de autenticação ocorreu por bloqueio de rede local
                Console.WriteLine($"Falha na autenticação do PNCP. Código HTTP: {(int)response.StatusCode}. Erro: {response.ErrorMessage ?? "Sem mensagem de rede"}");
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