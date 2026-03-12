using Microsoft.AspNetCore.Mvc;
using IntegracionesSAP;

namespace IntegracionesSAP.Controllers
{
    [ApiController]
    [Route("test/[controller]")]
    public class ReserveInvoiceController : ControllerBase
    {
        /// <summary>
        /// Crea una Factura de Reserva en SAP B1
        /// </summary>
        [HttpPost("Create")]
        public IActionResult Create([FromBody] FreservaHeader reserva)
        {
            if (reserva == null)
                return BadRequest(new { message = "No se recibió la información de la factura de reserva." });

            if (string.IsNullOrEmpty(reserva.CardCode))
                return BadRequest(new { message = "El campo 'CardCode' es obligatorio." });

            if (reserva.Lines == null || !reserva.Lines.Any())
                return BadRequest(new { message = "Debe incluir al menos una línea en la factura de reserva." });

            try
            {
                SapDiApi.Connect();
                int docEntry = SapDiApi.CreateReserveInvoice(reserva);

                return Ok(new
                {
                    message = "Factura de Reserva creada correctamente en SAP B1",
                    DocEntry = docEntry
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear Factura de Reserva en SAP B1",
                    error = ex.Message
                });
            }
            finally
            {
                SapDiApi.Disconnect();
            }
        }
    }
}
