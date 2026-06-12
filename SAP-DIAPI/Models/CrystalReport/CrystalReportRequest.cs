namespace IntegracionesSAP.Models.CrystalReport
{
    public class CrystalReportRequest
    {
        public string TemplateName { get; set; }
        public List<CrystalReportParameter> Parameters { get; set; }
    }

    public class CrystalReportParameter
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }
}
