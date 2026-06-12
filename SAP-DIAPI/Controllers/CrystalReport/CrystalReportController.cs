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

        // Endpoint generico: cualquier crystal con parametros custom
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

        // Factura de moto - Copia (PRINTED = Y)
        [HttpGet("MotorcycleInvoice/Copy/{docentry}")]
        public async Task<IActionResult> MotorcycleInvoiceCopy(int docentry)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                service.SetInvoicePrinted(docentry, "Y");
                response = await service.GetPDF(BuildRequest("INV20099", docentry));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        // Factura de moto - Original (PRINTED = N)
        [HttpGet("MotorcycleInvoice/Original/{docentry}")]
        public async Task<IActionResult> MotorcycleInvoiceOriginal(int docentry)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                service.SetInvoicePrinted(docentry, "N");
                response = await service.GetPDF(BuildRequest("INV20099", docentry));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        // Factura de repuestos - Original
        [HttpGet("SparePartsInvoice/Original/{docentry}")]
        public async Task<IActionResult> SparePartsInvoiceOriginal(int docentry)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await service.GetPDF(BuildRequest("INV20101", docentry));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        // Guia de envio
        [HttpGet("ShippingGuide/{docentry}")]
        public async Task<IActionResult> ShippingGuide(int docentry)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await service.GetPDF(BuildRequest("WTR10026", docentry));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        // Nota de entrega de moto
        [HttpGet("MotorcycleDeliveryNote/{docentry}")]
        public async Task<IActionResult> MotorcycleDeliveryNote(int docentry)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await service.GetPDF(BuildRequest("DLN20023", docentry));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        // Nota de despacho
        [HttpGet("DispatchNote/{docentry}")]
        public async Task<IActionResult> DispatchNote(int docentry)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                response = await service.GetPDF(BuildRequest("RDR20021", docentry));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        private static CrystalReportRequest BuildRequest(string templateName, int docEntry)
        {
            return new CrystalReportRequest
            {
                TemplateName = templateName,
                Parameters = new List<CrystalReportParameter>
                {
                    new CrystalReportParameter { Key = "DocKey@", Value = docEntry.ToString() }
                }
            };
        }
    }
}
