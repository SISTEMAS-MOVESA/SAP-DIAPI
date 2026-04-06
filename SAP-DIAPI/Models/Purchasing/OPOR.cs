using SAPbobsCOM;

namespace IntegracionesSAP.Models.Purchasing
{
    public class OPOR
    {
        public string? CardCode { get; set; }

        public int? Series { get; set; }

        public DateTime DocDate { get; set; } = DateTime.Now;

        public DateTime DocDueDate { get; set; } = DateTime.Now;
        public BoDocumentTypes DocType { get; set; } = BoDocumentTypes.dDocument_Items;

        public string? Comments { get; set; }

        public string? JournalMemo { get; set; }

        public List<UserField> UserFields { get; set; } = new List<UserField>();

        public List<POR1> Lines { get; set; } = new List<POR1>();
    }
}
