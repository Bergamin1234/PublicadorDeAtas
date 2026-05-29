using PublicadorARP.Models.Dtos;
using PublicadorARP.Models.ViewModels;
using PublicadorARP.Services.Interfaces;
using RestSharp;
using System.Net;
using System.Text.RegularExpressions;
using WebApp.Models.Dtos;

namespace PublicadorARP.Services
{
    public class PNCPService : IPNCPService
    {
        private readonly RestClient _client;
        private readonly IConfiguration _configuration;

        public PNCPService(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new RestClient(_configuration["ApiPNCP:Route"]);
        }

        public async Task<ContratacaoViewModel?> ConsultarContratacao(ConsultarContratacaoDto dto)
        {
            try
            {
                var request = new RestRequest(resource: $"orgaos/04801221000110/compras/{dto.anoCompra}/{dto.sequencialCompra}", method: Method.Get);
                var response = await _client.ExecuteAsync<ContratacaoViewModel>(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var ataListViewModel = response.Data;
                    return ataListViewModel;
                }
                else
                {
                    Console.WriteLine($"Erro na requisição: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na requisição: {ex.Message}");
                return null;
            }
        }

        public async Task<IList<AtaRegistroPrecoViewModel>?> ConsultarAtasPorContratacao(ConsultarContratacaoDto dto)
        {
            try
            {
                var request = new RestRequest(resource: $"orgaos/04801221000110/compras/{dto.anoCompra}/{dto.sequencialCompra}/atas", method: Method.Get);
                var response = await _client.ExecuteAsync<AtaRegistroPrecoListViewModel>(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var ataListViewModel = response.Data.Data;
                    return ataListViewModel;
                }
                else
                {
                    Console.WriteLine($"Erro na requisição: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na requisição: {ex.Message}");
                return null;
            }
        }

        public async Task<ContratacaoViewModel?> ConsultarContratacaoComAtasDeRegistroDePreco(ConsultarContratacaoDto dto)
        {
            try
            {
                var contratacao = await ConsultarContratacao(dto);
                var ataListViewModel = await ConsultarAtasPorContratacao(dto);

                if (contratacao != null)
                {
                    contratacao.AtasRegistroPrecoList = ataListViewModel;
                    return contratacao;
                }
                else
                {
                    Console.WriteLine($"Erro");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return null;
            }
        }

        public async Task<AtaRegistroPrecoViewModel?> ConsultarAtaRegistroPreco(ConsultarAtaRegistroPrecoDto dto)
        {
            try
            {
                var request = new RestRequest(resource: $"orgaos/04801221000110/compras/{dto.anoCompra}/{dto.sequencialCompra}/atas/{dto.sequencialAta}", method: Method.Get);
                var response = await _client.ExecuteAsync<AtaRegistroPrecoViewModel>(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var ataViewModel = response.Data;
                    return ataViewModel;
                }
                else
                {
                    Console.WriteLine($"Erro na requisição: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na requisição: {ex.Message}");
                return null;
            }
        }

        public async Task<(RestResponse, string)> InserirAtaRegistroPreco(InserirAtaRegistroPrecoDto dto)
        {
            try
            {
                string pattern = @"(\d{14})-(\d)-(\d{6})/(\d{4})";

                Match match = Regex.Match(dto.idPNCP, pattern);
                if (match.Success)
                {
                    dto.cnpjOrgao = match.Groups[1].Value;
                    dto.sequencialCompra = (match.Groups[3].Value);
                    dto.anoCompra = match.Groups[4].Value;
                }
                else
                {
                    Console.WriteLine("A string não está no formato esperado.");
                    throw new Exception();
                }

                var request = new RestRequest(resource: $"orgaos/{dto.cnpjOrgao}/compras/{dto.anoCompra}/{dto.sequencialCompra}/atas", method: Method.Post);

                string login = _configuration["AuthPNCP:Login"];
                string senha = _configuration["AuthPNCP:Senha"];
                var token = await LoginAndGetTokenAsync(login, senha);
                request.AddHeader("Authorization", $"Bearer {token}");

                // ==============
                // Modificação (Inclusão de Headers Obrigatórios v2.4)
                // Substituído o Header antigo "Content-Type: application/json". 
                // Agora enviamos os metadados do documento exigidos na validação de porta da API.
                // =============
                string nomeArquivo = $"ATA_REGISTRO_PRECO_No_{dto.numeroAta}/{dto.anoAta}";
                request.AddHeader("Titulo-Documento", $"{nomeArquivo}");
                request.AddHeader("Tipo-Documento-Id", "11"); // Código 11 = Ata de Registro de Preço
                
                string location = "";

                // ==============
                // Modificação (Injeção de campos de negócio obrigatórios no JSON)
                // Incluídas as propriedades "possibilidadeAdesao" e "partesEnvolvidas" dentro do objeto anônimo,
                // que antes causavam o erro 422 por estarem ausentes no modelo da versão antiga.
                // =============
                var requestData = new
                {
                    numeroAtaRegistroPreco = dto.numeroAta,
                    anoAta = dto.anoAta,
                    dataAssinatura = dto.dataAssinatura,
                    dataVigenciaInicio = dto.dataInicioVigencia,
                    dataVigenciaFim = dto.dataFimVigencia,
                    possibilidadeAdesao = dto.PossibilidadeAdesao, // Novo campo v2.4 mapeado da base
                    partesEnvolvidas = dto.PartesEnvolvidas,        // Nova lista v2.4 mapeada da base
                    codigoUnidade = dto.codigoUnidade               // Campo obrigatório de unidade adicionado
                };

                // ==============
                // Modificação (Construção do modelo Multipart/Form-Data no RestSharp)
                // Antiga Estrutura: request.AddJsonBody(requestData);
                // Nova Estrutura: Serializamos o JSON e o anexamos como um parâmetro de formulário nomeado "ata",
                // junto com o arquivo PDF físico nomeado "documento" no mesmo pacote (envelope único).
                // =============
                string jsonString = System.Text.Json.JsonSerializer.Serialize(requestData);
                request.AddParameter("ata", jsonString);

                var arquivoBytes = ConvertIFormFileToByteArray(dto.arquivo);
                request.AddFile("documento", arquivoBytes, dto.arquivo.FileName, "application/pdf");

                // Dispara o lote único para o governo
                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    location = response.Headers.FirstOrDefault(h => h.Name == "Location")?.Value.ToString();
                    
                    // ==============
                    // Modificação (Remoção da Segunda Etapa de Upload)
                    // Antiga Estrutura: O sistema pegava o "location/arquivos" e realizava uma chamada assíncrona 
                    // para o método "UploadFileAsync". Esse bloco foi desativado porque causava o erro 422 
                    // (o governo bloqueia o POST inicial se ele não contiver o arquivo PDF anexado logo de primeira).
                    // =============
                    /* var urlUpload = $"{location}/arquivos";
                    var responseUpload = await UploadFileAsync(urlUpload, dto.arquivo, token, nomeArquivo);
                    if (responseUpload.IsSuccessful) { Console.WriteLine(responseUpload.StatusCode); }
                    else { Console.WriteLine($"Failed to upload file: {responseUpload.StatusCode}"); }
                    */

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
                throw ex;
            }
        }

        public async Task<RestResponse> AlterarAtaRegistroPreco(PublicadorDeAtas.Models.AlterarAtaRegistroPrecoDto dto)
        {
            try
            {
                var request = new RestRequest(resource: $"orgaos/{dto.cnpjOrgao}/compras/{dto.anoCompra}/{dto.sequencialCompra}/atas/{dto.sequencialAta}", method: Method.Put);

                string login = _configuration["AuthPNCP:Login"];
                string senha = _configuration["AuthPNCP:Senha"];
                var token = await LoginAndGetTokenAsync(login, senha);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddHeader("Content-Type", "application/json");

                // ====================
                // Edição feita por ítalo aqui
                // ====================
                var jsonString = System.Text.Json.JsonSerializer.Serialize(dto);
                var jsonDicionario = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                
                if (jsonDicionario != null)
                {
                    jsonDicionario.Remove("anoCompra");
                    jsonDicionario.Remove("AnoCompra");
                    request.AddJsonBody(jsonDicionario);
                }
                // ====================

                var response = await _client.ExecuteAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error altering form: {ex.Message}");
                throw ex;
            }
        }

        private async Task<string?> LoginAndGetTokenAsync(string login, string senha)
        {
            try
            {
                var request = new RestRequest(resource: "usuarios/login", method: Method.Post);
                request.AddHeader("Content-Type", "application/json");

                var requestData = new
                {
                    login = login,
                    senha = senha
                };
                request.AddJsonBody(requestData);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    string token = response.Headers.FirstOrDefault(h => h.Name == "Authorization")?.Value.ToString()?.Replace("Bearer ", "");
                    return token;
                }
                else
                {
                    Console.WriteLine($"Failed to login: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging in: {ex.Message}");
                return null;
            }
        }

        // ==============
        // Modificação (Método Obsoleto / Estrutura Antiga)
        // Este método realizava o upload isolado do PDF na API antiga. Ele foi mantido no arquivo 
// apenas como histórico/legado da estrutura antiga, mas não é mais invocado pelo fluxo principal.
        // =============
        private async Task<RestResponse?> UploadFileAsync(string uploadUrl, IFormFile file, string token, string nomeArquivo)
        {
            var restClient = new RestClient(uploadUrl);
            var arquivoContent = ConvertIFormFileToByteArray(file);

            var request = new RestRequest(resource: "", method: Method.Post);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Titulo-Documento", $"{nomeArquivo}");
            request.AddHeader("Tipo-Documento", "11");
            request.AddFile("arquivo", arquivoContent, file.FileName, "application/pdf");

            var response = await restClient.ExecuteAsync(request);
            return response;
        }

        private byte[] ConvertIFormFileToByteArray(IFormFile file)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}