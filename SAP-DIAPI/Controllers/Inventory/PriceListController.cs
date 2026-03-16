using Azure;
using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Inventory;
using IntegracionesSAP.Services.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace IntegracionesSAP.Controllers.Inventory
{
    [Route("v1/PriceList/")]
    [ApiController]
    public class PriceListController : ControllerBase
    {
        private readonly ItemMasterDataService service;

        public PriceListController()
        {
            service = new ItemMasterDataService();
        }

        [HttpPost("Update")]
        public IActionResult UpdatePriceList([FromBody] OPLN document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document.ListNum == null)
                    return Ok(response.Error(400, "Campo [ListNum] es requerido"));

                if (document.Items == null || !document.Items.Any())
                    return Ok(response.Error(400, "No se enviaron artículos"));

                foreach (var item in document.Items)
                {
                    if (string.IsNullOrEmpty(item.ItemCode))
                        return Ok(response.Error(400, "Campo [ItemCode] debe ir relleno en [obj.Items]"));

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

        [HttpPost("SpecialPrice")]
        public IActionResult UpdateSpecialPrices([FromBody] OSPP document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document.CardCode == null)
                    return Ok(response.Error(400, "Campo [CardCode] es requerido"));

                if (document.Items == null || !document.Items.Any())
                    return Ok(response.Error(400, "No se enviaron artículos"));

                foreach (var item in document.Items)
                {
                    if (string.IsNullOrEmpty(item.ItemCode))
                        return Ok(response.Error(400, "Campo [ItemCode] debe ir relleno en [obj.Items]"));

                    if (item.Price == null)
                        return Ok(response.Error(400, $"Precio no definido para {item.ItemCode}"));
                }

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
