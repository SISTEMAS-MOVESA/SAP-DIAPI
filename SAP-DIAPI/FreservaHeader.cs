namespace IntegracionesSAP
{
    public class FreservaHeader
    {
        public string CardCode { get; set; }           // Código de cliente
        public string CardName { get; set; }           // Nombre del cliente
        public int Series { get; set; }                // Serie del documento
        public DateTime DocDate { get; set; }          // Fecha del documento
        public string Reference2 { get; set; }         // Referencia interna
        public string Comments { get; set; }           // Comentarios
        public string NumAtCard { get; set; }          // Número de referencia del cliente
        public int SalesPersonCode { get; set; }       // Código del vendedor
        public int GroupNumber { get; set; }           // Condiciones de pago (Payment Terms)

        // Lista de líneas de la factura de reserva
        public List<FreservaLine> Lines { get; set; } = new();
    }
}
