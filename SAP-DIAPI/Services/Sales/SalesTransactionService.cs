using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Sales;
using SAPbobsCOM;

namespace IntegracionesSAP.Services.Sales
{
    public class SalesTransactionService
    {
        private Company _company;

        public SalesTransactionService()
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
                return response.Error(500, ex.Message, ex);
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
                return response.Error(500, ex.Message, ex);
            }
        }

        public ApiResponse CreateSalesOrder(ORDR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;

            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

                SAPobj.CardCode = obj.CardCode;
                SAPobj.DocDate = DateTime.Now;
                SAPobj.TaxDate = DateTime.Now;
                SAPobj.Series = obj.Series;
                SAPobj.Comments = obj.Comments;
                SAPobj.SalesPersonCode = obj.SalesPersonCode;

                if (obj.UserFields != null)
                {
                    foreach (var UF in obj.UserFields)
                    {
                        try
                        {
                            SAPobj.UserFields.Fields.Item(UF.Key).Value = UF.Value;
                        }
                        catch { }
                    }
                }

                int LineNum = 0;
                foreach (var item in obj.Lines)
                {
                    SAPobj.Lines.SetCurrentLine(LineNum);

                    SAPobj.Lines.ItemCode = item.ItemCode;
                    SAPobj.Lines.Quantity = item.Quantity;
                    SAPobj.Lines.Price = item.Price;

                    if (!string.IsNullOrEmpty(item.SerialNumber))
                    {
                        SAPobj.Lines.UserFields.Fields.Item("U_MSERIE").Value = item.SerialNumber;
                        //SAPobj.Lines.SerialNumber = item.SerialNumber;

                        if (item.SysSerial != null)
                        {
                            SAPobj.Lines.SerialNumbers.SystemSerialNumber = (int)item.SysSerial;
                            SAPobj.Lines.SerialNumbers.Add();
                        }
                    }

                    SAPobj.Lines.Add();
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP ORDR: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry, DocType = BoObjectTypes.oOrders
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP ORDR: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

    }
}
