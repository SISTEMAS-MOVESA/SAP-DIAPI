namespace IntegracionesSAP.Models.Sales
{
    public class CancelBulkRequest
    {
        public List<int> DocEntries { get; set; } = new List<int>();
        public string Comments { get; set; } = string.Empty;
    }

    public class CancelBulkItemResult
    {
        public int DocEntry { get; set; }
        public string? Error { get; set; }
    }

    public class CancelBulkResult
    {
        public List<CancelBulkItemResult> Succeeded { get; set; } = new List<CancelBulkItemResult>();
        public List<CancelBulkItemResult> Failed { get; set; } = new List<CancelBulkItemResult>();
    }
}
