using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Business_Partners;
using IntegracionesSAP.Services.Business_Partners;
using IntegracionesSAP.Services.Inventory;
using Microsoft.AspNetCore.Mvc;
using SAPbobsCOM;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IntegracionesSAP.Controllers
{
    [ApiController]
    [Route("v1/BusinessPartner")]
    public class BusinessPartnerController : ControllerBase
    {
        private readonly BusinessPartnerService service;
        private readonly int GrpCodeMOVESA = 106;
        private readonly int GrpCodeSKG = 116;

        private readonly int PriceListMostrador = 1;
        private readonly string Currency = "LPS";
        private readonly int PaymentContado = 48;
        private readonly int Payment4Meses = 4;

        public BusinessPartnerController()
        {
            service = new BusinessPartnerService();
        }

        // ============================
        //  POST api/BusinessPartner/create
        // ============================
        [HttpPost("createCustomer/MOVESA")]
        public IActionResult CreateCustomerMovesa([FromBody] OCRD obj)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (obj == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));

                // validar campos importantes
                if (string.IsNullOrWhiteSpace(obj.AdditionalID))
                    return Ok(response.Error(500, "El campo [AdditionalID] es obligatorio."));

                if (service.ExistsAdditionalId(obj.AdditionalID, GrpCodeMOVESA))
                    return Ok(response.Error(500, $"Ya existe un cliente con el DNI [{obj.AdditionalID}]"));

                // validar direcciones
                if (obj.Addresses == null || obj.Addresses.Count == 0)
                    return Ok(response.Error(500, "Debe enviar al menos una dirección [SHIP TO][BILL TO]."));

                var bill_to = ValidateAddress(obj.Addresses, "BILL TO");
                if (!bill_to.Success) return Ok(bill_to);

                var ship_to = ValidateAddress(obj.Addresses, "SHIP TO");
                if (!ship_to.Success) return Ok(ship_to);

                // setear propiedades default
                obj.CardCode = service.GetNextCL();
                obj.Currency = Currency;
                obj.GroupCode = GrpCodeMOVESA; // Clts. Moto
                obj.FederalTaxID = "000000000000";
                obj.PayTermsGrpCode = PaymentContado;
                obj.PriceListNum = PriceListMostrador;

                obj.UserFields ??= new List<UserField>();
                obj.UserFields.Add(new UserField() { Key = "U_Canal", Value = "01" });
                obj.UserFields.Add(new UserField() { Key = "U_Empresa", Value = "1" });

                obj.Properties ??= new List<SapProperty>();
                obj.Properties.Add(new SapProperty() { Property = 13, Value = BoYesNoEnum.tYES });

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CreateCustomer(obj);
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


        [HttpPost("createCustomer/SKG")]
        public IActionResult CreateCustomerSKG([FromBody] OCRD obj)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var data = service.GetCustomerSIFCO(obj.CardCode);
                if (data == null)
                    return Ok(response.Error(400, $"Cliente [{obj.CardCode}] no es valido"));

                if (service.ExistsCardCode(obj.CardCode))
                    return Ok(response.Error(500, $"Ya existe el cliente [{obj.CardCode}] en SAP"));

                // setear propiedades default
                obj.CardCode = data["CardCode"].ToString();
                obj.CardName = data["CardName"].ToString();
                obj.CardForeignName = data["AddId"].ToString();
                obj.FatherCard = "SK100000";

                obj.Currency = Currency;
                obj.GroupCode = GrpCodeSKG; // Clts. skg
                obj.FederalTaxID = "000000000000";
                obj.AdditionalID = data["AddId"].ToString();
                obj.UnifiedFederalTaxID = data["UnifiedFederalTaxID"].ToString();
                obj.PayTermsGrpCode = Payment4Meses;
                obj.PriceListNum = PriceListMostrador;

                obj.Phone1 = data["Phone"].ToString();
                obj.Phone2 = data["Phone"].ToString();
                obj.Cellular = data["Phone"].ToString();
                obj.EmailAddress = "NT";

                obj.Valid = false;
                // bloquear 2 dia despues de crearse
                obj.Frozen = true;
                obj.FrozenFrom = DateTime.Now.AddDays(2);

                obj.UserFields ??= new List<UserField>();
                obj.UserFields.Add(new UserField() { Key = "U_Canal", Value = "01" });
                obj.UserFields.Add(new UserField() { Key = "U_Empresa", Value = "2" });
                obj.UserFields.Add(new UserField() { Key = "U_FechaNacimiento", Value = data["BirthDate"].ToString() });
                obj.UserFields.Add(new UserField() { Key = "U_Genero", Value = data["Gender"].ToString() });

                obj.Addresses.Add(new CRD1()
                {
                    AddressName = "BILL TO",
                    Street = "NT",
                    Block = MSSQL.Left(data["Address"].ToString(), 100),
                    AddressName3 = "NT",
                    City = data["City"].ToString(),
                    County = data["State"].ToString(),
                    State = data["StateId"].ToString(),
                    Country = "HN",
                    AddressType = BoAddressType.bo_BillTo
                });

                obj.Addresses.Add(new CRD1()
                {
                    AddressName = "SHIP TO",
                    Street = "NT",
                    Block = MSSQL.Left(data["Address"].ToString(),100),
                    AddressName3 = "NT",
                    City = data["City"].ToString(),
                    County = data["State"].ToString(),
                    State = data["StateId"].ToString(),
                    Country = "HN",
                    AddressType = BoAddressType.bo_ShipTo,
                    TaxCode = "ISV"
                });

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CreateCustomer(obj);
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

        [HttpGet("NextCL")]
        public IActionResult NextCL()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var data = service.GetNextCL();
                return Ok(response.Ok(data));
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }


        [HttpGet("validateAddId")]
        public IActionResult validateAddId([FromQuery] string AddId, int GrpCode)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var data = service.ExistsAdditionalId(AddId, GrpCode);
                return Ok(response.Ok(data));
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        // ---------------- helpers
        private ApiResponse ValidateAddress(List<CRD1> Addresses, string addressId)
        {
            ApiResponse response = new ApiResponse();

            CRD1 Address = Addresses.FirstOrDefault(x => x.AddressName == addressId);

            if (Address == null)
                return response.Error(500, $"Direccion Address.AddressName[{addressId}] es requerida");

            if (string.IsNullOrWhiteSpace(Address.AddressName))
                return response.Error(500, $"Campo Address.AddressName es requerido");

            if (string.IsNullOrWhiteSpace(Address.Street))
                return response.Error(500, $"Campo Address.Street es requerido");

            if (string.IsNullOrWhiteSpace(Address.Block))
                return response.Error(500, $"Campo Address.Block es requerido");

            if (string.IsNullOrWhiteSpace(Address.AddressName3))
                return response.Error(500, $"Campo Address.AddressName3 es requerido");

            if (string.IsNullOrWhiteSpace(Address.City))
                return response.Error(500, $"Campo Address.City es requerido");

            if (string.IsNullOrWhiteSpace(Address.State))
                return response.Error(500, $"Campo Address.State es requerido");

            Address.Country = "HN";

            if (addressId == "SHIP TO")
            {
                Address.AddressType = BoAddressType.bo_ShipTo;
                Address.TaxCode = "ISV";
            }
            else if (addressId == "BILL TO")
            {
                Address.AddressType = BoAddressType.bo_BillTo;
            }

            return response.Ok();
        }
    }
}
