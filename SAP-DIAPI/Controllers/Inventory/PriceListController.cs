using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Inventory;
using IntegracionesSAP.Services.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers.Inventory
{
    [Route("v1/PriceList/")]
    [ApiController]
    public class PriceListController : ControllerBase
    {
        private readonly PriceListService service;

        public PriceListController()
        {
            service = new PriceListService();
        }

        /// <summary>
        /// Actualiza el precio de múltiples artículos en una lista de precios específica.
        /// Body: { ListNum: int, Items: [{ ItemCode, Price }] }
        /// </summary>
        [HttpPost("Update")]
        public IActionResult UpdatePriceList([FromBody] OPLN document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document.ListNum == null)
                    return Ok(response.Error(400, "Campo [ListNum] es requerido"));

                if (document.Items == null || !document.Items.Any())
                    return Ok(response.Error(400, "Campo [Items] es requerido y no puede estar vacío"));

                foreach (var item in document.Items)
                {
                    if (string.IsNullOrEmpty(item.ItemCode))
                        return Ok(response.Error(400, "Campo [ItemCode] debe ir relleno en [Items]"));

                    if (item.Price == null)
                        return Ok(response.Error(400, $"Precio no definido para {item.ItemCode}"));
                }

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.UpdatePriceList(document);
                }
                finally
                {
                    service.Disconnect();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        /// <summary>
        /// Upsert masivo de precios especiales: misma lista de ítems aplicada a todos los socios indicados.
        /// Body: { CardCodes: [{ CardCode }], Items: [{ ItemCode, Price }], Currency?: "LPS" }
        /// </summary>
        [HttpPost("SpecialPrice")]
        public IActionResult UpdateSpecialPrices([FromBody] OSPP document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document.CardCodes == null || !document.CardCodes.Any())
                    return Ok(response.Error(400, "Campo [CardCodes] es requerido y no puede estar vacío"));

                if (document.Items == null || !document.Items.Any())
                    return Ok(response.Error(400, "Campo [Items] es requerido y no puede estar vacío"));

                foreach (var customer in document.CardCodes)
                {
                    if (string.IsNullOrWhiteSpace(customer.CardCode))
                        return Ok(response.Error(400, "Cada entrada en [CardCodes] debe tener [CardCode]"));
                }

                foreach (var item in document.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.ItemCode))
                        return Ok(response.Error(400, "Campo [ItemCode] debe ir relleno en [Items]"));

                    if (item.Price == null)
                        return Ok(response.Error(400, $"Precio no definido para {item.ItemCode}"));
                }

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.UploadSpecialPrices(document);
                }
                finally
                {
                    service.Disconnect();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        /// <summary>
        /// Upsert de precios especiales para un socio de negocio específico.
        /// Body: { Items: [{ ItemCode, Price }] }
        /// </summary>
        [HttpPost("SpecialPrice/{cardCode}")]
        public IActionResult UpdateSpecialPricesByCardCode(string cardCode, [FromBody] OSPP document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (string.IsNullOrWhiteSpace(cardCode))
                    return Ok(response.Error(400, "CardCode es requerido en la URL"));

                if (document.Items == null || !document.Items.Any())
                    return Ok(response.Error(400, "Campo [Items] es requerido y no puede estar vacío"));

                foreach (var item in document.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.ItemCode))
                        return Ok(response.Error(400, "Campo [ItemCode] debe ir relleno en [Items]"));

                    if (item.Price == null)
                        return Ok(response.Error(400, $"Precio no definido para {item.ItemCode}"));
                }

                document.CardCode = cardCode;

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.UpdateSpecialPrices(document);
                }
                finally
                {
                    service.Disconnect();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }
    }
}
