namespace IntegracionesSAP
{
    public class FreservaLine
    {
        public string ItemCode { get; set; }          // Código del artículo
        public string WarehouseCode { get; set; }     // Código del almacén
        public string SerialNum { get; set; }         // Número de serie
        public double Quantity { get; set; }          // Cantidad
        public double Price { get; set; }             // Precio unitario
        public string TaxCode { get; set; }           // Código de impuesto

        // Campos de usuario
        public string U_N_Factura { get; set; }       // Número de factura personalizada
        public string U_Cardcode { get; set; }        // Código de cliente final
        public string U_Cardname { get; set; }        // Nombre de cliente final
    }
}
