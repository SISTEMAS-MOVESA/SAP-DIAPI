using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Purchasing;
using SAPbobsCOM;
using System.Data;

namespace IntegracionesSAP.Services.Purchasing
{
    public class PurchaseTransactionService
    {
        private Company _company;

        public PurchaseTransactionService()
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

        public ApiResponse CreatePurchaseOrder(OPOR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;
            Recordset rs = null;

            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseOrders);

                SAPobj.CardCode = obj.CardCode;
                SAPobj.Series = obj.Series;

                SAPobj.DocDate = obj.DocDate;
                SAPobj.DocDueDate = obj.DocDueDate;

                SAPobj.Comments = obj.Comments;

                if (!string.IsNullOrEmpty(obj.JournalMemo))
                    SAPobj.JournalMemo = obj.JournalMemo;

                // UserFields header
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

                SAPobj.DocType = BoDocumentTypes.dDocument_Service;

                int lineNum = 0;

                foreach (var item in obj.Lines)
                {
                    SAPobj.Lines.SetCurrentLine(lineNum);

                    SAPobj.Lines.ItemDescription = item.ItemDescription;
                    SAPobj.Lines.AccountCode = item.AccountCode;
                    SAPobj.Lines.Quantity = item.Quantity;
                    SAPobj.Lines.UnitPrice = item.UnitPrice;
                    SAPobj.Lines.TaxCode = item.TaxCode;

                    if (item.UserFields != null)
                    {
                        foreach (var UF in item.UserFields)
                        {
                            try
                            {
                                SAPobj.Lines.UserFields.Fields.Item(UF.Key).Value = UF.Value;
                            }
                            catch { }
                        }
                    }

                    SAPobj.Lines.Add();
                    lineNum++;
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OPOR: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType = BoObjectTypes.oInventoryTransferRequest
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP OPOR: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);

                if (rs != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
            }
        }
    }
}
