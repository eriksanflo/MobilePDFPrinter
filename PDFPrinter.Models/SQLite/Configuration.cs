using SQLite;

namespace PDFPrinter.Models.SQLite
{
    public class Configuration
    {
        [PrimaryKey]
        public int IdConfiguration { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public string PoSName { get; set; }
        public string PrinterName { get; set; }
        public string PrintingType { get; set; }
        public string ServicePath { get; set; }
    }
}
