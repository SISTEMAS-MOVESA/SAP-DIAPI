using IntegracionesSAP.Models;
using IntegracionesSAP.Models.CrystalReport;
using IntegracionesSAP.Services.CrystalReport;
using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers
{
    [ApiController]
    [Route("v1/CrystalReport")]
    public class CrystalReportController : ControllerBase
    {
        private readonly CrystalReportService service;

        public CrystalReportController()
        {
            service = new CrystalReportService();
        }

        [HttpPost("GetPDF")]
        public async Task<IActionResult> GetPDF([FromBody] CrystalReportRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (request == null)
                    return Ok(response.Error(400, "Request no puede ser nulo"));

                if (string.IsNullOrEmpty(request.TemplateName))
                    return Ok(response.Error(400, "Campo [TemplateName] es requerido"));

                response = await service.GetPDF(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }
    }
}
