using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers.Inventory
{
    [ApiController]
    [Route("v1/GoodsReceipt")]
    public class GoodsReceiptController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GoodsReceiptController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("create")]
        public IActionResult CreateGoodsReceipt([FromBody] OrderHeader order)
        {
            if (order == null)
            return BadRequest(new { message = "No se recibió el objeto de entrada de mercaderías." });

            if (string.IsNullOrWhiteSpace(order.CardCode))
                return BadRequest(new { message = "El campo 'CardCode' es obligatorio." });

            if (order.Lines == null || !order.Lines.Any())
                return BadRequest(new { message = "Debe incluir al menos una línea." });

            try
            {
                SapDiApi.Connect();

                int docEntry = SapDiApi.CreateGoodsReceipt(order);

                // Obtener cadena de conexión desde appsettings.json
                var connString = _configuration.GetConnectionString("SAP");

                if (string.IsNullOrEmpty(connString))
                {
                    throw new Exception("No se encontró ConnectionString 'SAP' en appsettings.json");
                }

                var osrn = new OsrnUpdater(connString);

                if (order.Aduanas != null)
                {
                    foreach (var a in order.Aduanas)
                    {
                        bool ok = osrn.ActualizarOSRN(
                            a.Serie,
                            a.Aduana,
                            a.FechaPago,
                            a.Poliza,
                            a.Item,
                            a.CodigoRepuesto
                        );

                        if (!ok)
                        {
                            return StatusCode(500, new
                            {
                                message = $"Entrada creada (DocEntry {docEntry}), pero falló al actualizar OSRN para serie {a.Serie}"
                            });
                        }
                    }
                }

                return Ok(new
                {
                    message = "Entrada de Mercaderías creada correctamente en SAP B1.",
                    DocEntry = docEntry
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear Entrada de Mercaderías en SAP B1.",
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
