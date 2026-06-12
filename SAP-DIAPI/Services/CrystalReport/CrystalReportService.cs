using IntegracionesSAP.Models;
using IntegracionesSAP.Models.CrystalReport;
using System.Text;
using System.Text.Json;

namespace IntegracionesSAP.Services.CrystalReport
{
    public class CrystalReportService
    {
        private static readonly string LayoutApiUrl = "http://192.168.1.6/SAP-LAYOUT-API/api/Report/PDF";
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<ApiResponse> GetPDF(CrystalReportRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                string json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage httpResponse = await _http.PostAsync(LayoutApiUrl, content);
                string body = await httpResponse.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ApiResponse>(body, _jsonOpts);
                return result ?? response.Error(500, "Respuesta invalida del servicio de reportes");
            }
            catch (TaskCanceledException)
            {
                return response.Error(504, "El servicio de reportes no respondio en el tiempo esperado");
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
        }
    }
}
