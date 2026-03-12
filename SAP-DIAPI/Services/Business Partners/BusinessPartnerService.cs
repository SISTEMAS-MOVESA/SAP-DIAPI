using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Business_Partners;
using SAPbobsCOM;
using System.Data;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace IntegracionesSAP.Services.Business_Partners
{
    public class BusinessPartnerService
    {
        private Company _company;

        public BusinessPartnerService()
        {
            _company = SAPConnection.GetDefaultCOM();
        }

        public ApiResponse Connect()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                SAPConnection.Connect();
                return response.Ok();
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
        }

        public ApiResponse Disconnect()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                SAPConnection.Disconnect();
                return response.Ok();
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
        }


        public ApiResponse CreateCustomer(OCRD customer)
        {
            ApiResponse response = new ApiResponse();
            BusinessPartners SAPobj = null;

            try
            {
               // var company = DiApi.GetCOM();
                SAPobj = (BusinessPartners)_company.GetBusinessObject(BoObjectTypes.oBusinessPartners);

                if (SAPobj.GetByKey(customer.CardCode))
                    return response.Error(400, $"El cliente [{customer.CardCode}] ya existe");

                SAPobj.CardCode = customer.CardCode;
                SAPobj.CardName = customer.CardName;
                SAPobj.CardForeignName = customer.CardForeignName;
                SAPobj.CardType = BoCardTypes.cCustomer;

                SAPobj.Currency = customer.Currency;
                SAPobj.GroupCode = customer.GroupCode;

                SAPobj.FederalTaxID = customer.FederalTaxID;
                SAPobj.AdditionalID = customer.AdditionalID;
                SAPobj.UnifiedFederalTaxID = customer.UnifiedFederalTaxID;

                SAPobj.Phone1 = customer.Phone1;
                SAPobj.Phone2 = customer.Phone2;
                SAPobj.Cellular = customer.Cellular;
                SAPobj.EmailAddress = customer.EmailAddress;
                SAPobj.Notes = customer.Notes;

                SAPobj.PayTermsGrpCode = customer.PayTermsGrpCode;
                SAPobj.PriceListNum = customer.PriceListNum;
                SAPobj.SalesPersonCode = customer.SalesPersonCode;

                foreach (var UP in customer.Properties ?? [])
                {
                    try { SAPobj.Properties[UP.Property] = UP.Value; }
                    catch { }
                }

                foreach (var UF in customer.UserFields ?? [])
                {
                    try { SAPobj.UserFields.Fields.Item(UF.Key).Value = UF.Value; }
                    catch { }
                }


                if (!string.IsNullOrEmpty(customer.FatherCard))
                    SAPobj.FatherCard = customer.FatherCard;

                if (customer.Frozen)
                {
                    SAPobj.Frozen = BoYesNoEnum.tYES;
                    SAPobj.FrozenFrom = customer.FrozenFrom ?? DateTime.Now.AddDays(1);
                }

                if (!customer.Valid)
                    SAPobj.Valid = BoYesNoEnum.tNO;

                // direcciones
                int AddrLinenum = 0;
                foreach (var addr in customer.Addresses)
                {
                    SAPobj.Addresses.SetCurrentLine(AddrLinenum);

                    SAPobj.Addresses.AddressName = addr.AddressName;
                    SAPobj.Addresses.Street = addr.Street;
                    SAPobj.Addresses.Block = addr.Block;
                    SAPobj.Addresses.AddressName3 = addr.AddressName3;

                    SAPobj.Addresses.City = addr.City;
                    SAPobj.Addresses.County = addr.County;
                    SAPobj.Addresses.State = addr.State;
                    SAPobj.Addresses.Country = addr.Country;

                    SAPobj.Addresses.AddressType = addr.AddressType;
                    if (!string.IsNullOrEmpty(addr.TaxCode))
                        SAPobj.Addresses.TaxCode = addr.TaxCode;

                    SAPobj.Addresses.Add();
                   AddrLinenum++;
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OCRD: {errCode} - {errMsg}");
                }

                string cardCode = _company.GetNewObjectKey();

                return response.Ok(cardCode);
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (SAPobj != null)
                    Marshal.ReleaseComObject(SAPobj);
            }
        }

        public bool ExistsAdditionalId(string addId, int groupCode)
        {
            int exists = MSSQL.ExecuteScalar<int>(
                MSSQL.DB_DEFAULT,
                "SELECT COUNT(1) FROM OCRD WHERE AddID = @AddID AND GroupCode = @GroupCode",
                new Dictionary<string, object>
                {
                    { "@AddID", addId },
                    { "@GroupCode", groupCode }
                }
            );

            return exists > 0;
        }

        public bool ExistsCardCode(string CardCode)
        {
            int exists = MSSQL.ExecuteScalar<int>(
                MSSQL.DB_DEFAULT,
                "SELECT COUNT(1) FROM OCRD WHERE CardCode = @CardCode",
                new Dictionary<string, object>
                {
                    { "@CardCode", CardCode }
                }
            );

            return exists > 0;
        }

        public string GetNextCL()
        {
            string CardCode = MSSQL.ExecuteScalar<string>(
                MSSQL.DB_DEFAULT, "SELECT dbo.getNextCL()", null
            );

            return CardCode;
        }

        public string GetCLByAddId(string addId, int groupCode)
        {
            string cardCode = MSSQL.ExecuteScalar<string>(
                MSSQL.DB_DEFAULT,
                "SELECT MAX(CardCode) FROM OCRD WHERE AddID = @AddID AND GroupCode = @GroupCode",
                new Dictionary<string, object>
                {
                    { "@AddID", addId },
                    { "@GroupCode", groupCode }
                }
            );

            return cardCode;
        }

        /// <returns>
        /// retorna un objeto con: { CardCode, RefId, AddId, UnifiedFederalTaxID, CardName, Phone, Address, City, StateId, State, Country, BirthDate, Gender }
        /// </returns>
        public Dictionary<string, object> GetCustomerSIFCO(string cardcode)
        {
            var stmt = MSSQL.ExecuteQuery(
                MSSQL.DB_DEFAULT,
                @"SELECT
                    CardCode, CodCliente AS RefId, Identidad AS AddId, RTN AS UnifiedFederalTaxID, 
                    NombreCliente AS CardName, Telefono AS Phone, 
                    Direccion AS Address, Municipio AS City, DepCodigo AS StateId, DepNombre AS State, Pais AS Country,
                    FechaNacimiento AS BirthDate, Genero AS Gender
                FROM dbo.fn_GetCustomerSIFCO(@cardcode);",
                new Dictionary<string, object>
                {
                    { "@cardcode", cardcode }
                }
            );

            var data = stmt.FirstOrDefault();
            if(data == null) return data;

            data["CardCode"] = MSSQL.Trim(data["CardCode"].ToString());
            data["AddId"] = MSSQL.Trim(data["AddId"].ToString());
            data["UnifiedFederalTaxID"] = MSSQL.Trim(data["UnifiedFederalTaxID"].ToString());
            data["CardName"] = MSSQL.Trim(data["CardName"].ToString());
            data["Address"] = MSSQL.Trim(data["Address"].ToString());

            return data;
        }
    
        
    }
}
