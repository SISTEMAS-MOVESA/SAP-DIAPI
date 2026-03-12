using System.Numerics;

namespace IntegracionesSAP
{
    public class OrderHeader
    {
        public int DocEntry { get; set; }
        public int DocSeries { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public string? DocCurrency { get; set; }
        public string? Comments { get; set; }
        public int SalesPersonCode { get; set; }
        public int Series { get; set; }
        public double DocTotal { get; set; }
        public double DiscSum { get; set; }

        // === Campo adicional del encabezado ===
        public string? NumAtCard { get; set; }

        // === User Field para referencia weba api rest===
        public string? U_placaTransporte { get; set; }
        // === User Field para token de moto ===
        public string? U_seriemoto { get; set; }

        public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
        public List<AduanaInfo>? Aduanas { get; set; }

    }
    public class AduanaInfo
    {
        public string? Serie { get; set; }
        public string? Aduana { get; set; }
        public DateTime FechaPago { get; set; }
        public string? Poliza { get; set; }
        public string? Item { get; set; }
        public string? CodigoRepuesto { get; set; }
    }
}
