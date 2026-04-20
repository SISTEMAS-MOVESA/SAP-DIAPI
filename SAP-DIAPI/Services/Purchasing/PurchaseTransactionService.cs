using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Purchasing;
using SAPbobsCOM;

namespace IntegracionesSAP.Services.Purchasing
{
    /// <summary>
    /// Servicio de transacciones de compras SAP B1.
    ///
    /// Mapa de tipos de documento (BaseType / BoObjectTypes):
    ///   OPRQ  = 1470000113  oPurchaseRequest
    ///   OPQT  = 540000006   oPurchaseQuotations
    ///   OPOR  = 22          oPurchaseOrders
    ///   OPDN  = 20          oPurchaseDeliveryNotes
    ///   OPCH  = 18          oPurchaseInvoices
    /// </summary>
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

        #region CREATE

        /// <summary>
        /// Crea una Solicitud de Compra (OPRQ — object type 1470000113).
        /// </summary>
        public ApiResponse CreatePurchaseRequest(OPRQ obj)
        {
            ApiResponse response = new ApiResponse();
            Documents doc = null;

            try
            {
                doc = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseRequest);

                if (!string.IsNullOrEmpty(obj.CardCode))
                    doc.CardCode = obj.CardCode;

                doc.DocDate      = obj.DocDate;
                doc.DocDueDate   = obj.DocDueDate;
                doc.RequriedDate = obj.DocDueDate;
                doc.Comments     = obj.Comments;

                if (obj.Series.HasValue)
                    doc.Series = obj.Series.Value;

                CopyUserFields(obj.UserFields, doc.UserFields);

                int i = 0;
                foreach (var line in obj.Lines)
                {
                    doc.Lines.SetCurrentLine(i);
                    doc.Lines.ItemCode     = line.ItemCode;
                    doc.Lines.Quantity     = line.Quantity;
                    doc.Lines.UnitPrice    = line.UnitPrice;
                    doc.Lines.RequiredDate = obj.DocDueDate;
                    if (!string.IsNullOrEmpty(line.AccountCode))
                        doc.Lines.AccountCode = line.AccountCode;
                    if (!string.IsNullOrEmpty(line.TaxCode))
                        doc.Lines.TaxCode = line.TaxCode;
                    CopyUserFields(line.UserFields, doc.Lines.UserFields);
                    doc.Lines.Add();
                    i++;
                }

                int ret = doc.Add();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OPRQ: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());
                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType  = BoObjectTypes.oPurchaseRequest
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (doc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
            }
        }

        /// <summary>
        /// Crea una Oferta de Compra (OPQT — object type 540000006).
        /// DocType controlado desde el endpoint: CreateItems fuerza dDocument_Items,
        /// CreateServices fuerza dDocument_Service.
        /// </summary>
        public ApiResponse CreatePurchaseQuotation(OPQT obj)
        {
            ApiResponse response = new ApiResponse();
            Documents doc = null;

            try
            {
                doc = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseQuotations);

                doc.CardCode    = obj.CardCode;
                doc.DocDate     = obj.DocDate;
                doc.DocDueDate  = obj.DocDueDate;
                doc.Comments    = obj.Comments;
                doc.DocType     = obj.DocType;
                doc.Series      = (int)obj.Series;
                doc.DocCurrency = obj.DocCurrency;

                CopyUserFields(obj.UserFields, doc.UserFields);

                foreach (var line in obj.Lines)
                {
                    doc.Lines.SetCurrentLine((int)line.LineNum);

                    if (obj.DocType == BoDocumentTypes.dDocument_Items)
                        doc.Lines.ItemCode = line.ItemCode;
                    else
                    {
                        doc.Lines.ItemDescription = line.ItemDescription;
                        if (!string.IsNullOrEmpty(line.AccountCode))
                            doc.Lines.AccountCode = line.AccountCode;
                    }

                    doc.Lines.Quantity  = line.Quantity;
                    doc.Lines.UnitPrice = line.UnitPrice;
                    if (!string.IsNullOrEmpty(line.TaxCode))
                        doc.Lines.TaxCode = line.TaxCode;

                    CopyUserFields(line.UserFields, doc.Lines.UserFields);
                    doc.Lines.Add();
                }

                int ret = doc.Add();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OPQT: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());
                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType  = BoObjectTypes.oPurchaseQuotations
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (doc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
            }
        }

        /// <summary>
        /// Crea un Pedido de Compra (OPOR — object type 22).
        /// DocType controlado desde el endpoint: CreateItems / CreateServices.
        ///
        /// Cada línea puede incluir BaseEntry + BaseLine + BaseType para referenciar
        /// un documento origen (ej. OPQT BaseType=540000006). En ese caso SAP copia
        /// ItemCode y datos del origen; Quantity=0 toma la cantidad abierta restante.
        /// Líneas sin BaseEntry se crean como standalone con ItemCode/AccountCode explícito.
        /// </summary>
        public ApiResponse CreatePurchaseOrder(OPOR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents doc = null;

            try
            {
                doc = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseOrders);

                doc.CardCode    = obj.CardCode;
                doc.DocDate     = obj.DocDate;
                doc.DocDueDate  = obj.DocDueDate;
                doc.Comments    = obj.Comments;
                doc.DocType     = obj.DocType;
                doc.Series      = (int)obj.Series;
                doc.DocCurrency = obj.DocCurrency;

                CopyUserFields(obj.UserFields, doc.UserFields);

                foreach (var line in obj.Lines)
                {
                    doc.Lines.SetCurrentLine((int)line.LineNum);

                    if (line.BaseEntry.HasValue && line.BaseEntry > 0)
                    {
                        // ── Copia desde documento base ───────────────────────────────
                        // BaseType: 540000006=OPQT, 22=OPOR, 20=OPDN, 1470000113=OPRQ
                        doc.Lines.BaseEntry = line.BaseEntry.Value;
                        doc.Lines.BaseLine  = line.BaseLine ?? 0;
                        doc.Lines.BaseType  = line.BaseType ?? (int)BoObjectTypes.oPurchaseQuotations;

                        // Quantity=0 → SAP usa cantidad abierta restante automáticamente
                        if (line.Quantity > 0)
                            doc.Lines.Quantity = line.Quantity;

                        if (line.UnitPrice > 0)
                            doc.Lines.UnitPrice = line.UnitPrice;

                        if (!string.IsNullOrEmpty(line.WhsCode))
                            doc.Lines.WarehouseCode = line.WhsCode;
                    }
                    else if (obj.DocType == BoDocumentTypes.dDocument_Items)
                    {
                        // ── Línea standalone tipo artículo ───────────────────────────
                        doc.Lines.ItemCode  = line.ItemCode;
                        doc.Lines.Quantity  = line.Quantity;
                        doc.Lines.UnitPrice = line.UnitPrice;
                        if (!string.IsNullOrEmpty(line.WhsCode))
                            doc.Lines.WarehouseCode = line.WhsCode;
                    }
                    else
                    {
                        // ── Línea standalone tipo servicio ───────────────────────────
                        doc.Lines.ItemDescription = line.ItemDescription;
                        doc.Lines.Quantity        = line.Quantity;
                        doc.Lines.UnitPrice       = line.UnitPrice;
                        if (!string.IsNullOrEmpty(line.AccountCode))
                            doc.Lines.AccountCode = line.AccountCode;
                    }

                    if (!string.IsNullOrEmpty(line.TaxCode))
                        doc.Lines.TaxCode = line.TaxCode;

                    CopyUserFields(line.UserFields, doc.Lines.UserFields);
                    doc.Lines.Add();
                }

                int ret = doc.Add();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OPOR: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());
                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType  = BoObjectTypes.oPurchaseOrders
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (doc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
            }
        }

        /// <summary>
        /// Crea una Entrada de Mercancía de Compras (OPDN — object type 20).
        /// Soporta vinculación a OPOR (BaseType=22) y números de serie por línea.
        /// Adaptado de SapDiApi.CreateGoodsReceipt con modelo propio OPDN/PDN1.
        /// </summary>
        public ApiResponse CreateGoodsReceipt(OPDN obj)
        {
            ApiResponse response = new ApiResponse();
            Documents doc = null;

            try
            {
                doc = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseDeliveryNotes);

                doc.CardCode    = obj.CardCode;
                doc.DocDate     = obj.DocDate ?? DateTime.Now;
                doc.DocDueDate  = obj.DocDueDate ?? DateTime.Now;
                doc.Comments    = obj.Comments;
                doc.Series      = (int)obj.Series;
                doc.DocCurrency = obj.DocCurrency;
                doc.DocTotal    = obj.DocTotal;
                doc.NumAtCard   = obj.NumAtCard;
                doc.Reference2  = "SAP-DIAPI";

                CopyUserFields(obj.UserFields, doc.UserFields);

                int i = 0;
                foreach (var line in obj.Lines)
                {
                    doc.Lines.SetCurrentLine(i);

                    doc.Lines.ItemCode      = line.ItemCode;
                    doc.Lines.Quantity      = line.Quantity;
                    doc.Lines.UnitPrice     = line.UnitPrice;
                    doc.Lines.WarehouseCode = line.WhsCode;
                    if (!string.IsNullOrEmpty(line.TaxCode))
                        doc.Lines.TaxCode = line.TaxCode;

                    if (line.BaseEntry.HasValue && line.BaseEntry > 0)
                    {
                        doc.Lines.BaseEntry = line.BaseEntry.Value;
                        doc.Lines.BaseLine  = line.BaseLine ?? 0;
                        doc.Lines.BaseType  = line.BaseType ?? (int)BoObjectTypes.oPurchaseOrders; // 22
                    }

                    if (line.Serials != null && line.Serials.Any())
                    {
                        foreach (var s in line.Serials)
                        {
                            doc.Lines.SerialNumbers.InternalSerialNumber     = s.InternalSerialNumber;
                            doc.Lines.SerialNumbers.ManufacturerSerialNumber = s.ManufacturerSerialNumber;
                            doc.Lines.SerialNumbers.ManufactureDate          = s.ManufactureDate ?? DateTime.Now;
                            doc.Lines.SerialNumbers.Location                 = s.Location;
                            doc.Lines.SerialNumbers.Notes                    = s.Notes;
                            doc.Lines.SerialNumbers.Add();
                        }
                    }

                    CopyUserFields(line.UserFields, doc.Lines.UserFields);
                    doc.Lines.Add();
                    i++;
                }

                int ret = doc.Add();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OPDN: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());
                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType  = BoObjectTypes.oPurchaseDeliveryNotes
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (doc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
            }
        }

        /// <summary>
        /// Crea una Factura de Proveedores (OPCH — object type 18).
        /// Puede generarse desde OPDN (BaseType=20), OPOR (BaseType=22),
        /// o como documento independiente sin base.
        /// DocType controlado desde el endpoint: CreateItems / CreateServices.
        /// </summary>
        public ApiResponse CreateAPInvoice(OPCH obj)
        {
            ApiResponse response = new ApiResponse();
            Documents doc = null;

            try
            {
                doc = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);

                doc.CardCode    = obj.CardCode;
                doc.DocDate     = obj.DocDate;
                doc.DocDueDate  = obj.DocDueDate;
                doc.Comments    = obj.Comments;
                doc.DocType     = obj.DocType;
                doc.Series      = (int)obj.Series;
                doc.DocCurrency = obj.DocCurrency;
                if (!string.IsNullOrEmpty(obj.NumAtCard))
                    doc.NumAtCard = obj.NumAtCard;

                CopyUserFields(obj.UserFields, doc.UserFields);

                foreach (var line in obj.Lines)
                {
                    doc.Lines.SetCurrentLine((int)line.LineNum);

                    if (obj.DocType == BoDocumentTypes.dDocument_Items)
                        doc.Lines.ItemCode = line.ItemCode;
                    else
                    {
                        doc.Lines.ItemDescription = line.ItemDescription;
                        if (!string.IsNullOrEmpty(line.AccountCode))
                            doc.Lines.AccountCode = line.AccountCode;
                    }

                    doc.Lines.Quantity  = line.Quantity;
                    doc.Lines.UnitPrice = line.UnitPrice;
                    if (!string.IsNullOrEmpty(line.TaxCode))
                        doc.Lines.TaxCode = line.TaxCode;

                    // BaseType: 20 = OPDN (Entrada Mercancía), 22 = OPOR (Pedido de Compra)
                    if (line.BaseEntry.HasValue && line.BaseEntry > 0)
                    {
                        doc.Lines.BaseEntry = line.BaseEntry.Value;
                        doc.Lines.BaseLine  = line.BaseLine ?? 0;
                        doc.Lines.BaseType  = line.BaseType ?? (int)BoObjectTypes.oPurchaseDeliveryNotes; // 20
                    }

                    CopyUserFields(line.UserFields, doc.Lines.UserFields);
                    doc.Lines.Add();
                }

                int ret = doc.Add();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OPCH: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());
                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType  = BoObjectTypes.oPurchaseInvoices
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (doc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
            }
        }

        #endregion

        #region COPY (1-to-1, documento completo con cantidades abiertas)

        /// <summary>
        /// Copia una OPRQ completa a una OPQT usando cantidades abiertas restantes.
        /// Para copias parciales o multi-origen usar CreateOrderFromQuotations.
        /// </summary>
        public ApiResponse CopyRequestToQuotation(int docEntry, int series)
        {
            return CopyDocument(
                docEntry, series,
                BoObjectTypes.oPurchaseRequest,
                BoObjectTypes.oPurchaseQuotations,
                "OPRQ -> OPQT");
        }

        /// <summary>
        /// Copia una OPQT completa a una OPOR usando cantidades abiertas restantes.
        /// Para copias parciales o multi-origen usar CreateOrderFromQuotations.
        /// </summary>
        public ApiResponse CopyQuotationToOrder(int docEntry, int series)
        {
            return CopyDocument(
                docEntry, series,
                BoObjectTypes.oPurchaseQuotations,
                BoObjectTypes.oPurchaseOrders,
                "OPQT -> OPOR");
        }

        private ApiResponse CopyDocument(
            int baseEntry, int targetSeries,
            BoObjectTypes baseType, BoObjectTypes targetType,
            string label)
        {
            ApiResponse response = new ApiResponse();
            Documents baseDoc = null;
            Documents newDoc  = null;

            try
            {
                baseDoc = (Documents)_company.GetBusinessObject(baseType);
                if (!baseDoc.GetByKey(baseEntry))
                    return response.Error(404, $"Documento base [{baseEntry}] no encontrado");

                newDoc = (Documents)_company.GetBusinessObject(targetType);

                newDoc.CardCode   = baseDoc.CardCode;
                newDoc.DocDate    = DateTime.Now;
                newDoc.DocDueDate = baseDoc.DocDueDate;
                newDoc.Series     = targetSeries;

                CopyUserFieldsFromSource(baseDoc.UserFields, newDoc.UserFields);

                for (int i = 0; i < baseDoc.Lines.Count; i++)
                {
                    baseDoc.Lines.SetCurrentLine(i);

                    if (baseDoc.Lines.LineStatus == BoStatus.bost_Open)
                    {
                        newDoc.Lines.BaseEntry = baseEntry;
                        newDoc.Lines.BaseType  = (int)baseType;
                        newDoc.Lines.BaseLine  = i;
                        newDoc.Lines.Quantity  = baseDoc.Lines.RemainingOpenQuantity;

                        CopyUserFieldsFromSource(baseDoc.Lines.UserFields, newDoc.Lines.UserFields);
                        newDoc.Lines.Add();
                    }
                }

                int ret = newDoc.Add();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP {label}: {errCode} - {errMsg}");
                }

                int newDocEntry = int.Parse(_company.GetNewObjectKey());
                return response.Ok(new SAPObjResult { DocEntry = newDocEntry, DocType = targetType });
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (baseDoc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(baseDoc);
                if (newDoc  != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(newDoc);
            }
        }

        #endregion

        #region HELPERS

        private void CopyUserFields(List<UserField>? fields, UserFields target)
        {
            if (fields == null) return;
            foreach (var uf in fields)
            {
                try { target.Fields.Item(uf.Key).Value = uf.Value; }
                catch { }
            }
        }

        private void CopyUserFieldsFromSource(UserFields source, UserFields target)
        {
            for (int i = 0; i < source.Fields.Count; i++)
            {
                try
                {
                    string name = source.Fields.Item(i).Name;
                    target.Fields.Item(name).Value = source.Fields.Item(i).Value;
                }
                catch { }
            }
        }

        public static bool UpdateOSRN(
            DBConnection db,
            string mnfSerial,
            string nAduana,
            DateTime fPago,
            string nPoliza,
            string nItem,
            string codigoRep)
        {
            try
            {
                string estadoMoto = "01";        // en caja
                string ubicacion  = "100000080"; // bodega

                string sql = @"
                UPDATE OSRN SET
                    U_NADUANA       = @NADUANA,
                    U_FPAGO         = @FPAGO,
                    U_NPOLIZA       = @NPOLIZA,
                    U_NITEM         = @NITEM,
                    U_CODIGOREP     = @CODIGOREP,
                    U_Estado_Moto   = @ESTADO_MOTO,
                    U_UBICACION_DCM = @UBICACION_DCM
                WHERE MNFSERIAL = @MNFSERIAL";

                var parameters = new Dictionary<string, object>
                {
                    { "@NADUANA",       nAduana },
                    { "@FPAGO",         fPago },
                    { "@NPOLIZA",       nPoliza },
                    { "@NITEM",         nItem },
                    { "@CODIGOREP",     codigoRep },
                    { "@ESTADO_MOTO",   estadoMoto },
                    { "@UBICACION_DCM", ubicacion },
                    { "@MNFSERIAL",     mnfSerial }
                };

                int rowsAffected = MSSQL.ExecuteNonQuery(db, sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Update OSRN: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
