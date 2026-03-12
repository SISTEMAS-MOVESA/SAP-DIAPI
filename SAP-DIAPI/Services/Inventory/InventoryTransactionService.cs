using Azure.Core;
using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Inventory;
using SAPbobsCOM;
using System.Security.Cryptography.Xml;

namespace IntegracionesSAP.Services.Inventory
{
    public class InventoryTransactionService
    {
        private Company _company;

        public InventoryTransactionService()
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

        public ApiResponse CreateTransferRequest(OWTQ document)
        {
            ApiResponse response = new ApiResponse();
            StockTransfer SAPobj = null;

            try
            {
                //var company = DiApi.GetCOM();
                SAPobj = (StockTransfer)_company.GetBusinessObject(BoObjectTypes.oInventoryTransferRequest);

                SAPobj.DocDate = DateTime.Now;
                SAPobj.TaxDate = DateTime.Now;
                SAPobj.CardCode = document.CardCode;
                SAPobj.FromWarehouse = document.FromWarehouse;
                SAPobj.ToWarehouse = document.ToWarehouse;
                SAPobj.Series = document.Series;
                SAPobj.PriceList = document.PriceList;
                SAPobj.Comments = document.Comments;

                // UserFields Header
                if (document.UserFields != null)
                {
                    foreach (var UF in document.UserFields)
                    {
                        try
                        {
                            SAPobj.UserFields.Fields.Item(UF.Key).Value = UF.Value;
                        }
                        catch { }
                    }
                }

                foreach (var item in document.Lines)
                {
                    SAPobj.Lines.SetCurrentLine(item.LineNum);
                    SAPobj.Lines.ItemCode = item.ItemCode;
                    SAPobj.Lines.Quantity = item.Quantity;

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
                    return response.Error(500, $"Error SAP OWTQ: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult {
                    DocEntry = docEntry, DocNum = SAPConnection.GetObjDocNum("OWTQ", docEntry),
                    DocType = BoObjectTypes.oInventoryTransferRequest
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP OWTQ: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        public ApiResponse CreateStockTransfer(OWTR document)
        {
            ApiResponse response = new ApiResponse();
            StockTransfer SAPobj = null;

            try
            {
                //var company = DiApi.GetCOM();
                SAPobj = (StockTransfer)_company.GetBusinessObject(BoObjectTypes.oStockTransfer);

                SAPobj.DocDate = DateTime.Now;
                SAPobj.TaxDate = DateTime.Now;
                SAPobj.CardCode = document.CardCode;
                SAPobj.FromWarehouse = document.FromWarehouse;
                SAPobj.ToWarehouse = document.ToWarehouse;
                SAPobj.Series = document.Series;
                SAPobj.Comments = document.Comments;
                SAPobj.PriceList = document.PriceList;

                // UserFields Header
                if (document.UserFields != null)
                {
                    foreach (var UF in document.UserFields)
                    {
                        try
                        {
                            SAPobj.UserFields.Fields.Item(UF.Key).Value = UF.Value;
                        }
                        catch { }
                    }
                }

                foreach (var item in document.Lines)
                {
                    SAPobj.Lines.SetCurrentLine(item.LineNum);
                    // si contiene base entry
                    if (item.BaseEntry != null)
                    {
                        SAPobj.Lines.BaseEntry = (int) item.BaseEntry;
                        SAPobj.Lines.BaseLine = (int) item.BaseLine;
                        SAPobj.Lines.BaseType = (InvBaseDocTypeEnum) item.BaseType;
                        SAPobj.Lines.Quantity = item.Quantity;
                    }
                    else
                    {
                        SAPobj.Lines.ItemCode = item.ItemCode;
                        SAPobj.Lines.Quantity = item.Quantity;

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
                    }

                    SAPobj.Lines.Add();

                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OWTR: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());
                return response.Ok(new SAPObjResult {
                    DocEntry = docEntry, DocNum = SAPConnection.GetObjDocNum("OWTR", docEntry),
                    DocType = BoObjectTypes.oStockTransfer
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP OWTR: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        public ApiResponse CreateTransferFlow(OWTQ request)
        {
            ApiResponse response = new ApiResponse();
            //var company = DiApi.GetCOM();

            try
            {
                if (!_company.InTransaction)
                    _company.StartTransaction();

                // -------- crear solicitud traslado

                var OWTQ = CreateTransferRequest(request);

                if (!OWTQ.Success)
                    throw new Exception(OWTQ.Message);

                SAPObjResult requestData = (SAPObjResult)OWTQ.Data;

                // -------- preparar transferencia

                OWTR transfer = new OWTR()
                {
                    CardCode = request.CardCode,
                    FromWarehouse = request.FromWarehouse,
                    ToWarehouse = request.ToWarehouse,
                    Series = (int)request.TargetSeries,
                    Comments = request.Comments,
                    UserFields = request.UserFields,
                    PriceList = request.PriceList
                };

                int BaseLine = 0;
                foreach (var item in request.Lines)
                {
                    transfer.Lines.Add(new WTR1()
                    {
                        BaseEntry = requestData.DocEntry,
                        BaseType = (int)InvBaseDocTypeEnum.InventoryTransferRequest,
                        BaseLine = BaseLine,

                        Quantity = item.Quantity,
                        SysSerial = item.SysSerial,
                        SerialNumber = item.SerialNumber,
                    });
                    BaseLine++;
                }

                // -------- crear transferencia

                var OWTR = CreateStockTransfer(transfer);

                if (!OWTR.Success)
                    throw new Exception(OWTR.Message);

                SAPObjResult transferData = (SAPObjResult)OWTR.Data;

                // -------- commit

                _company.EndTransaction(BoWfTransOpt.wf_Commit);

                return response.Ok(
                    new { OWTQ = requestData, OWTR = transferData }
                );
            }
            catch (Exception ex)
            {
                if (_company.InTransaction)
                    _company.EndTransaction(BoWfTransOpt.wf_RollBack);

                return response.Error(500, $"SAP Error [Transfer Flow]: {ex.Message}");
            }
        }

    }
}
