using SAPbobsCOM;

namespace IntegracionesSAP.Models
{
    public class SAPObjResult
    {
        public int? DocEntry { get; set; }
        public int? DocNum { get; set; }
        public BoObjectTypes DocType { get; set; }

        /// <summary>Errores no-fatales de pasos post-creación (e.g. UPDATE OSRN UDFs).</summary>
        public List<string>? SqlErrors { get; set; }

        public SAPObjResult() { }

        public SAPObjResult(int docEntry, BoObjectTypes docType)
        {
            DocEntry = docEntry;
            DocType = docType;
        }

        public SAPObjResult(int docEntry, int docNum, BoObjectTypes docType)
        {
            DocEntry = docEntry;
            DocType = docType;
            DocNum = docNum;
        }
    }
}
