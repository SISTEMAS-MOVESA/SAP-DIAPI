using Microsoft.Data.SqlClient;
using SAPbobsCOM;

namespace IntegracionesSAP
{
    public static class SapDiApi
    {
        private static Company _company;
        /// <summary>
        /// Conecta a SAP Business One usando DI API. Empresa VEHICO
        /// </summary>
        public static void ConnectVehico()
        {

            if (_company != null && _company.Connected)
                return;

            _company = new Company
            {
                Server = "192.168.1.3",
                DbServerType = BoDataServerTypes.dst_MSSQL2012,
                CompanyDB = "VEHICO",
                UserName = "itpro",
                Password = "eeal96",
                language = BoSuppLangs.ln_Spanish_La,
                UseTrusted = false,
                DbUserName = "SA",
                DbPassword = "M*l!n3r0s2k12",
                SLDServer = "192.168.1.9:40000"
            };

            int errorCode = _company.Connect();
            if (errorCode != 0)
            {
                _company.GetLastError(out int errCode, out string errMsg);
                throw new Exception($"Error al conectar a SAP B1: {errCode} - {errMsg}");
            }
        }

        /// <summary>
        /// Metodo para desconectar de SAP Business One. Empresa VEHICO
        /// </sumary>
        public static void DisconnectVehico()
        {
            if (_company != null)
            {
                try
                {
                    if (_company.Connected)
                    {
                        _company.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al desconectar de SAP B1: {ex.Message}");
                }
                finally
                {
                    _company = null;
                }
            }
        }

        /// <summary>
        /// Conecta a SAP Business One usando DI API.
        /// </summary>
        public static void Connect()
        {
            if (_company != null && _company.Connected)
                return;

            _company = new Company
            {
                Server = "192.168.1.3",
                DbServerType = BoDataServerTypes.dst_MSSQL2012,
                CompanyDB = "MOVESA",
                UserName = "it",
                Password = "polar",
                language = BoSuppLangs.ln_Spanish_La,
                UseTrusted = false,
                DbUserName = "SA",
                DbPassword = "M*l!n3r0s2k12",
                SLDServer = "192.168.1.9:40000"
            };

            int errorCode = _company.Connect();
            if (errorCode != 0)
            {
                _company.GetLastError(out int errCode, out string errMsg);
                throw new Exception($"Error al conectar a SAP B1: {errCode} - {errMsg}");
            }
        }

        /// <summary>
        /// Metodo para desconectar de SAP Business One.
        /// </sumary>
        public static void Disconnect()
        {
            if (_company != null)
            {
                try
                {
                    if (_company.Connected)
                    {
                        _company.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al desconectar de SAP B1: {ex.Message}");
                }
                finally
                {
                    _company = null;
                }
            }
        }

        /// <summary>
        /// Crea una orden de venta en SAP B1.
        /// </summary>
        public static int CreateOrder(OrderHeader order)
        {
            if (_company == null || !_company.Connected)
                throw new Exception("No hay conexión con SAP B1");

            Documents salesOrder = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

            // HEADER
            salesOrder.CardCode = order.CardCode;
            salesOrder.DocDate = order.DocDate;
            salesOrder.DocDueDate = order.DocDueDate;
            salesOrder.Comments = order.Comments;
            salesOrder.SalesPersonCode = order.SalesPersonCode;
            salesOrder.Series = order.Series;
            salesOrder.Reference2 = "APIREST";

            // LINES
            foreach (var line in order.Lines)
            {
                salesOrder.Lines.ItemCode = line.ItemCode;
                salesOrder.Lines.Quantity = line.Quantity;
                salesOrder.Lines.Price = line.Price;
                salesOrder.Lines.UnitPrice = line.Price;
                salesOrder.Lines.DiscountPercent = line.DiscountPercent;
                salesOrder.Lines.WarehouseCode = line.WhsCode;
                salesOrder.Lines.TaxCode = line.TaxCode;

                if (line.Serialized)
                {
                    //datos de la moto
                    salesOrder.Lines.UserFields.Fields.Item("U_MSERIE").Value = line.U_MSERIE?.Length > 32 ? line.U_MSERIE.Substring(0, 32) : line.U_MSERIE;
                    salesOrder.Lines.UserFields.Fields.Item("U_MColor").Value = line.U_MColor?.Length > 25 ? line.U_MColor.Substring(0, 25) : line.U_MColor;
                    salesOrder.Lines.UserFields.Fields.Item("U_MAno").Value = line.U_MAno?.Length > 10 ? line.U_MAno.Substring(0, 20) : line.U_MAno;
                    salesOrder.Lines.UserFields.Fields.Item("U_MModelo").Value = line.U_MModelo?.Length > 25 ? line.U_MModelo.Substring(0, 25) : line.U_MModelo;
                    salesOrder.Lines.UserFields.Fields.Item("U_DatosAduana").Value = line.U_DatosAduana?.Length > 30 ? line.U_DatosAduana.Substring(0, 30) : line.U_DatosAduana;
                    salesOrder.Lines.UserFields.Fields.Item("U_MMarca").Value = line.U_MMarca?.Length > 25 ? line.U_MMarca.Substring(0, 25) : line.U_MMarca;
                    salesOrder.Lines.UserFields.Fields.Item("U_MItem").Value = line.U_MItem?.Length > 30 ? line.U_MItem.Substring(0, 30) : line.U_MItem;
                }

                salesOrder.Lines.Add();

            }

            int result = salesOrder.Add();
            if (result != 0)
            {
                _company.GetLastError(out int errCode, out string errMsg);
                throw new Exception($"Error al crear orden: {errCode} - {errMsg}");
            }

            // Obtener el DocEntry generado
            _company.GetNewObjectCode(out string newDocEntry);
            return int.Parse(newDocEntry);
        }

        /// <summary>
        /// actualizar precios en lineas de ordenes de venta en SAP B1.
        /// </summary>
        public static int UpdateLinePrice(OrderHeader order)
        {
            if (_company == null || !_company.Connected)
                throw new Exception("No hay conexión con SAP B1");

            Documents salesOrder = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

            // Buscar la orden por DocEntry
            if (!salesOrder.GetByKey(order.DocEntry))
                throw new Exception($"No se encontró la orden con DocEntry {order.DocEntry}");

            salesOrder.DocTotal = order.DocTotal;
            salesOrder.Comments = salesOrder.Comments + " #" + order.Comments;
            salesOrder.Reference2 = "APIREST";


            int i = 0;
            // Actualizar precios de líneas enviadas
            foreach (var lineUpdate in order.Lines)
            {
                // Validar que el LineNum esté dentro del rango de líneas en SAP
                //if (lineUpdate.LineNum < 0 || lineUpdate.LineNum >= salesOrder.Lines.Count)
                //{
                //    throw new Exception($"El número de línea {lineUpdate.LineNum} no existe en la orden.");
                //}

                // Mover el puntero directamente a la línea enviada en el JSON
                salesOrder.Lines.SetCurrentLine(i);

                // Validar que sea el mismo ítem si se envía ItemCode
                if (!string.IsNullOrEmpty(lineUpdate.ItemCode) &&
                    !salesOrder.Lines.ItemCode.Equals(lineUpdate.ItemCode, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"El ItemCode '{lineUpdate.ItemCode}' no coincide con el de la línea {lineUpdate.LineNum} en SAP.");
                }

                // Asignar el nuevo precio
                salesOrder.Lines.Price = lineUpdate.Price;
                salesOrder.Lines.UnitPrice = lineUpdate.Price;
                salesOrder.Lines.DiscountPercent = lineUpdate.DiscountPercent;

                i++;
            }

            // Guardar cambios
            int result = salesOrder.Update();
            if (result != 0)
            {
                _company.GetLastError(out int errCode, out string errMsg);
                throw new Exception($"Error al actualizar la orden: {errCode} - {errMsg}");
            }

            return order.DocEntry;
        }

        /// <summary>
        /// crear Facturas de Reserva en SAP B1.
        /// </summary>
        public static int CreateReserveInvoice(FreservaHeader reserva)
        {
            if (_company == null || !_company.Connected)
                throw new Exception("No hay conexión con SAP B1");

            Documents ReserveInvoice = (Documents)_company.GetBusinessObject(BoObjectTypes.oInvoices);

            // Indicamos que es factura de reserva
            ReserveInvoice.ReserveInvoice = BoYesNoEnum.tYES;

            // HEADER
            ReserveInvoice.CardCode = reserva.CardCode?.Length > 15 ? reserva.CardCode.Substring(0, 15) : reserva.CardCode;
            ReserveInvoice.CardName = reserva.CardName?.Length > 100 ? reserva.CardName.Substring(0, 100) : reserva.CardName;
            ReserveInvoice.Series = reserva.Series;
            ReserveInvoice.DocDate = reserva.DocDate;
            ReserveInvoice.DocDueDate= reserva.DocDate;
            ReserveInvoice.Reference2 = reserva.Reference2 ?? "FRPP";
            ReserveInvoice.Comments = reserva.Comments ?? $"Usuario API";
            ReserveInvoice.NumAtCard = reserva.NumAtCard;
            ReserveInvoice.SalesPersonCode = reserva.SalesPersonCode;
            ReserveInvoice.GroupNumber = reserva.GroupNumber;
            ReserveInvoice.PaymentGroupCode = reserva.GroupNumber;

            // LINES
            foreach (var line in reserva.Lines)
            {
                ReserveInvoice.Lines.ItemCode = line.ItemCode;
                ReserveInvoice.Lines.WarehouseCode = line.WarehouseCode;
                ReserveInvoice.Lines.SerialNum = line.SerialNum;
                ReserveInvoice.Lines.Quantity = line.Quantity;
                ReserveInvoice.Lines.Price = line.Price;
                ReserveInvoice.Lines.UnitPrice = line.Price; // Para respetar precio manual
                ReserveInvoice.Lines.TaxCode = line.TaxCode ?? "EXE";

                // Campos de usuario
                ReserveInvoice.Lines.UserFields.Fields.Item("U_N_Factura").Value =
                    line.U_N_Factura?.Length > 20 ? line.U_N_Factura.Substring(0, 20) : line.U_N_Factura;
                ReserveInvoice.Lines.UserFields.Fields.Item("U_Cardcode").Value =
                    line.U_Cardcode?.Length > 10 ? line.U_Cardcode.Substring(0, 10) : line.U_Cardcode;
                ReserveInvoice.Lines.UserFields.Fields.Item("U_Cardname").Value =
                    line.U_Cardname?.Length > 100 ? line.U_Cardname.Substring(0, 100) : line.U_Cardname;

                ReserveInvoice.Lines.Add();
            }

            // Guardar en SAP
            int lRetCode = ReserveInvoice.Add();
            if (lRetCode != 0)
            {
                _company.GetLastError(out int errCode, out string errMsg);
                throw new Exception($"Error al crear Factura de Reserva: {errCode} - {errMsg}");
            }

            _company.GetNewObjectCode(out string newDocEntry);
            return int.Parse(newDocEntry);
        }

        /// <summary>
        /// crear Entregas (Delivery) en SAP B1 a partir de Ordenes de Venta.
        /// </summary>
        /// 
        public static int CreateDeliveryFromOrder(OrderHeader order)
        {
            // Validar conexión
            if (_company == null || !_company.Connected)
                throw new Exception("No hay conexión activa a SAP Business One.");

            // Obtener la orden de venta desde SAP
            Documents salesOrder = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

            if (!salesOrder.GetByKey(order.DocEntry))
                throw new Exception($"No se encontró la Orden de Venta con DocEntry {order.DocEntry}.");

            // Crear nuevo documento de entrega
            Documents delivery = (Documents)_company.GetBusinessObject(BoObjectTypes.oDeliveryNotes);

            // Copiar datos principales de la orden
            delivery.Series = order.DocSeries;
            delivery.CardCode = salesOrder.CardCode;
            delivery.CardName = salesOrder.CardName;
            delivery.NumAtCard = salesOrder.NumAtCard;
            delivery.DocDate = DateTime.Now;
            delivery.DocDueDate = salesOrder.DocDueDate;
            delivery.TaxDate = DateTime.Now;
            delivery.SalesPersonCode = salesOrder.SalesPersonCode;
            delivery.Reference2 = "APIREST";

            // Copiar líneas desde la orden
            for (int i = 0; i < salesOrder.Lines.Count; i++)
            {
                salesOrder.Lines.SetCurrentLine(i);
                //informacion del origen del documento
                delivery.Lines.BaseEntry = order.DocEntry;
                delivery.Lines.BaseLine = salesOrder.Lines.LineNum;
                delivery.Lines.BaseType = (int)BoObjectTypes.oOrders;
                //lineas de la entrega
                delivery.Lines.Quantity = salesOrder.Lines.Quantity;
                delivery.Lines.UnitPrice = salesOrder.Lines.UnitPrice;
                delivery.Lines.Price = salesOrder.Lines.Price;
                delivery.Lines.TaxCode = salesOrder.Lines.TaxCode;
                delivery.Lines.WarehouseCode = salesOrder.Lines.WarehouseCode;

                delivery.Lines.Add();
            }

            // Guardar el documento en SAP
            int result = delivery.Add();
            if (result != 0)
            {
                string errMsg = _company.GetLastErrorDescription();
                throw new Exception($"Error al crear la entrega: {errMsg}");
            }

            // Obtener el DocEntry de la entrega recién creada
            _company.GetNewObjectCode(out string newDocEntryStr);
            int newDocEntry = int.Parse(newDocEntryStr);

            return newDocEntry;
        }
        
        /// <summary>
        /// crear Entrada de Mercaderias de Compras.
        /// </summary>
        public static int CreateGoodsReceipt(OrderHeader order)
        {
            if (_company == null || !_company.Connected)
                throw new Exception("No hay conexión con SAP B1");

            // Documento: Entrada de mercaderías (Purchase Delivery Notes)
            Documents oEntry =
                (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseDeliveryNotes);

            // ============================
            // === HEADER =================
            // ============================
            oEntry.DocDate = order.DocDate;
            oEntry.DocDueDate = order.DocDueDate;
            oEntry.CardCode = order.CardCode;
            oEntry.DocCurrency = order.DocCurrency;
            oEntry.DocTotal = order.DocTotal;
            oEntry.NumAtCard = order.NumAtCard;
            oEntry.Reference2 = "WEBAPI";
            oEntry.Series = order.Series;

            // UserField del header
            if (!string.IsNullOrEmpty(order.U_seriemoto))
                oEntry.UserFields.Fields.Item("U_placaTransporte").Value = "WEBAPIREST";
                oEntry.UserFields.Fields.Item("U_seriemoto").Value = order.U_seriemoto;

            // ============================
            // === LÍNEAS =================
            // ============================
            foreach (var line in order.Lines)
            {
                oEntry.Lines.ItemCode = line.ItemCode;
                oEntry.Lines.Quantity = line.Quantity;
                oEntry.Lines.UnitPrice = line.Price;
                oEntry.Lines.WarehouseCode = line.WhsCode;
                oEntry.Lines.TaxCode = line.TaxCode;

                // === Base Document Linking ===
                if (line.BaseEntry.HasValue && line.BaseEntry > 0)
                {
                    oEntry.Lines.BaseEntry = line.BaseEntry.Value;
                    oEntry.Lines.BaseLine = line.BaseLine ?? 0;
                    oEntry.Lines.BaseType = line.BaseType ?? 0;
                }

                // ============================
                // === SERIAL NUMBERS (FIX) ===
                // ============================
                var serialsForThisLine = line.Serials
                    .Where(s => s.DocLineNum == line.LineNum)
                    .ToList();

                foreach (var s in serialsForThisLine)
                {
                    oEntry.Lines.SerialNumbers.InternalSerialNumber = s.InternalSerialNumber;
                    oEntry.Lines.SerialNumbers.ManufacturerSerialNumber = s.ManufacturerSerialNumber;
                    oEntry.Lines.SerialNumbers.ManufactureDate = s.ManufactureDate ?? DateTime.Now;
                    oEntry.Lines.SerialNumbers.Location = s.Location;
                    oEntry.Lines.SerialNumbers.Notes = s.Notes;
                    oEntry.Lines.SerialNumbers.BatchID = s.BatchID;

                    oEntry.Lines.SerialNumbers.Add();
                }

                oEntry.Lines.Add();
            }

            // ============================
            // === CREAR DOCUMENTO =======
            // ============================
            int result = oEntry.Add();

            if (result != 0)
            {
                _company.GetLastError(out int errCode, out string errMsg);
                throw new Exception($"Error al crear Entrada de Mercaderías: {errCode} - {errMsg}");
            }

            // Obtener DocEntry generado
            _company.GetNewObjectCode(out string newDocEntry);
            return int.Parse(newDocEntry);
        }

        /// <summary>
        /// Crear Socio de Negocios (Business Partner) en SAP B1.
        /// </summary>
        public static int CreateBusinessPartner(BusinessPartnerCustomer bp)
        {
            if (_company == null || !_company.Connected)
                throw new Exception("No hay conexión con SAP B1.");

            BusinessPartners oBP =
                (BusinessPartners)_company.GetBusinessObject(BoObjectTypes.oBusinessPartners);

            // ================================
            // === DATOS PRINCIPALES ==========
            // ================================
            oBP.CardCode = bp.CardCode;
            oBP.CardName = bp.CardName;
            oBP.CardForeignName = bp.CardForeignName;
            oBP.Currency = bp.Currency;
            oBP.GroupCode = bp.GroupCode;
            oBP.FederalTaxID = bp.FederalTaxID;
            oBP.AdditionalID = bp.AdditionalID;
            oBP.UnifiedFederalTaxID = bp.UnifiedFederalTaxID;
            oBP.Phone1 = bp.Phone1;
            oBP.Phone2 = bp.Phone2;
            oBP.Cellular = bp.Cellular;
            oBP.EmailAddress = bp.EmailAddress;
            oBP.Notes = bp.Notes;

            oBP.CardType = BoCardTypes.cCustomer;

            // ================================
            // === CAMPOS PERSONALIZADOS (UDFs)
            // ================================
            oBP.SalesPersonCode = bp.SalesPersonCode;
            oBP.UserFields.Fields.Item("U_Canal").Value = bp.Canal;
            oBP.PayTermsGrpCode = bp.PayTermsGrpCode;
            oBP.PriceListNum = bp.PriceListNum;

            // Properties(13) = YES/NO
            oBP.Properties[13] = bp.Propiedad13
                ? BoYesNoEnum.tYES
                : BoYesNoEnum.tNO;

            oBP.UserFields.Fields.Item("U_Empresa").Value = bp.Empresa;

            // ================================
            // === DIRECCIONES ================
            // ================================
            if (bp.BillTo != null)
            {
                oBP.Addresses.AddressName = bp.BillTo.AddressName;
                oBP.Addresses.Street = bp.BillTo.Street;
                oBP.Addresses.StreetNo = bp.BillTo.StreetNo;
                oBP.Addresses.GlobalLocationNumber = bp.BillTo.GlobalLocationNumber;
                oBP.Addresses.Block = bp.BillTo.Block;
                oBP.Addresses.City = bp.BillTo.City;
                oBP.Addresses.County = bp.BillTo.County;
                oBP.Addresses.State = bp.BillTo.State;
                oBP.Addresses.Country = bp.BillTo.Country;
                oBP.Addresses.AddressType = BoAddressType.bo_BillTo;
                oBP.Addresses.AddressName3 = bp.BillTo.AddressName3;
                oBP.Addresses.Add();
            }

            if (bp.ShipTo != null)
            {
                oBP.Addresses.AddressName = bp.ShipTo.AddressName;
                oBP.Addresses.Street = bp.ShipTo.Street;
                oBP.Addresses.StreetNo = bp.ShipTo.StreetNo;
                oBP.Addresses.GlobalLocationNumber = bp.ShipTo.GlobalLocationNumber;
                oBP.Addresses.Block = bp.ShipTo.Block;
                oBP.Addresses.City = bp.ShipTo.City;
                oBP.Addresses.County = bp.ShipTo.County;
                oBP.Addresses.State = bp.ShipTo.State;
                oBP.Addresses.Country = bp.ShipTo.Country;
                oBP.Addresses.AddressType = BoAddressType.bo_ShipTo;
                oBP.Addresses.AddressName3 = bp.ShipTo.AddressName3;
                oBP.Addresses.TaxCode = bp.ShipTo.TaxCode;
                oBP.Addresses.Add();
            }

            // ================================
            // =========== CREAR ==============
            // ================================
            int result = oBP.Add();

            if (result != 0)
            {
                _company.GetLastError(out int code, out string msg);
                throw new Exception($"SAP Error {code}: {msg}");
            }

            //_company.GetNewObjectCode(out string newCardCode);
            return 1;
        }

        /// <summary>
        /// Crear Socio de Negocios (Business Partner) en SAP B1.
        /// </summary>
        public static int CreateBusinessPartnerVehico(BusinessPartnerCustomer bp)
        {
            if (_company == null || !_company.Connected)
                throw new Exception("No hay conexión con SAP B1.");

            BusinessPartners oBP =
                (BusinessPartners)_company.GetBusinessObject(BoObjectTypes.oBusinessPartners);

            // ================================
            // === DATOS PRINCIPALES ==========
            // ================================
            oBP.CardCode = bp.CardCode;
            oBP.CardName = bp.CardName;
            oBP.CardForeignName = bp.CardForeignName;
            oBP.Currency = bp.Currency;
            oBP.GroupCode = bp.GroupCode;
            oBP.FederalTaxID = bp.FederalTaxID;
            oBP.AdditionalID = bp.AdditionalID;
            oBP.UnifiedFederalTaxID = bp.UnifiedFederalTaxID;
            oBP.Phone1 = bp.Phone1;
            oBP.Phone2 = bp.Phone2;
            oBP.Cellular = bp.Cellular;
            oBP.EmailAddress = bp.EmailAddress;
            oBP.Notes = bp.Notes;

            oBP.CardType = BoCardTypes.cCustomer;

            // ================================
            // === CAMPOS PERSONALIZADOS (UDFs)
            // ================================
            oBP.SalesPersonCode = bp.SalesPersonCode;
            oBP.UserFields.Fields.Item("U_Canal").Value = bp.Canal;
            oBP.PayTermsGrpCode = bp.PayTermsGrpCode;
            oBP.PriceListNum = bp.PriceListNum;

            // Properties(13) = YES/NO
            oBP.Properties[13] = bp.Propiedad13
                ? BoYesNoEnum.tYES
                : BoYesNoEnum.tNO;

            oBP.UserFields.Fields.Item("U_Empresa").Value = bp.Empresa;

            // ================================
            // === DIRECCIONES ================
            // ================================
            if (bp.BillTo != null)
            {
                oBP.Addresses.AddressName = bp.BillTo.AddressName;
                oBP.Addresses.Street = bp.BillTo.Street;
                oBP.Addresses.StreetNo = bp.BillTo.StreetNo;
                oBP.Addresses.GlobalLocationNumber = bp.BillTo.GlobalLocationNumber;
                oBP.Addresses.Block = bp.BillTo.Block;
                oBP.Addresses.City = bp.BillTo.City;
                oBP.Addresses.County = bp.BillTo.County;
                oBP.Addresses.State = bp.BillTo.State;
                oBP.Addresses.Country = bp.BillTo.Country;
                oBP.Addresses.AddressType = BoAddressType.bo_BillTo;
                oBP.Addresses.AddressName3 = bp.BillTo.AddressName3;
                oBP.Addresses.Add();
            }

            if (bp.ShipTo != null)
            {
                oBP.Addresses.AddressName = bp.ShipTo.AddressName;
                oBP.Addresses.Street = bp.ShipTo.Street;
                oBP.Addresses.StreetNo = bp.ShipTo.StreetNo;
                oBP.Addresses.GlobalLocationNumber = bp.ShipTo.GlobalLocationNumber;
                oBP.Addresses.Block = bp.ShipTo.Block;
                oBP.Addresses.City = bp.ShipTo.City;
                oBP.Addresses.County = bp.ShipTo.County;
                oBP.Addresses.State = bp.ShipTo.State;
                oBP.Addresses.Country = bp.ShipTo.Country;
                oBP.Addresses.AddressType = BoAddressType.bo_ShipTo;
                oBP.Addresses.AddressName3 = bp.ShipTo.AddressName3;
                oBP.Addresses.TaxCode = bp.ShipTo.TaxCode;
                oBP.Addresses.Add();
            }

            // ================================
            // =========== CREAR ==============
            // ================================
            int result = oBP.Add();

            if (result != 0)
            {
                _company.GetLastError(out int code, out string msg);
                throw new Exception($"SAP Error {code}: {msg}");
            }

            //_company.GetNewObjectCode(out string newCardCode);
            return 1;
        }

        /// <summary>
        /// Actualizar Listas de Precios en SAP B1.
        /// </summary> 
        public static void UpdateItemPriceBatch(ItemPriceUpdateBatch batch)
        {
            if (_company == null || !_company.Connected)
                throw new Exception("No hay conexión con SAP Business One.");

            // === 1. AGRUPAR TODOS LOS UPDATE POR ITEMCODE ===
            var itemsPorCodigo = batch.Items
                .GroupBy(i => i.ItemCode.Trim())
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            foreach (var kv in itemsPorCodigo)
            {
                string itemCode = kv.Key;
                var priceUpdates = kv.Value; // todas las listas de precios de 1 ítem

                // Validación básica
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    foreach (var p in priceUpdates)
                    {
                        p.SapResultCode = -2;
                        p.ResultMessage = "ItemCode vacío o inválido.";
                    }
                    continue;
                }

                try
                {
                    Items oItem = (Items)_company.GetBusinessObject(BoObjectTypes.oItems);

                    bool exists = oItem.GetByKey(itemCode);
                    if (!exists)
                    {
                        foreach (var p in priceUpdates)
                        {
                            p.SapResultCode = -1;
                            p.ResultMessage = "El artículo no existe.";
                        }
                        continue;
                    }

                    Items_Prices oPriceList = oItem.PriceList;

                    // === 2. ACTUALIZAR TODAS LAS LISTAS DE PRECIO PARA ESTE ITEM ===
                    foreach (var p in priceUpdates)
                    {
                        if (p.PriceList <= 0)
                        {
                            p.SapResultCode = -3;
                            p.ResultMessage = "PriceList debe ser ≥ 1.";
                            continue;
                        }

                        // Index = priceList - 1
                        int index = p.PriceList - 1;

                        oPriceList.SetCurrentLine(index);
                        oPriceList.Price = (double)p.Price;
                        oPriceList.Currency = p.Currency;
                    }

                    // === 3. UN SOLO UPDATE PARA EL ITEM COMPLETO ===
                    int result = oItem.Update();

                    if (result != 0)
                    {
                        _company.GetLastError(out int errCode, out string errMsg);

                        foreach (var p in priceUpdates)
                        {
                            p.SapResultCode = errCode;
                            p.ResultMessage = errMsg;
                        }
                    }
                    else
                    {
                        foreach (var p in priceUpdates)
                        {
                            p.SapResultCode = 0;
                            p.ResultMessage = "Precio actualizado correctamente.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    foreach (var p in priceUpdates)
                    {
                        p.SapResultCode = -500;
                        p.ResultMessage = "Error inesperado: " + ex.Message;
                    }
                }
            }
        }

    }

}
