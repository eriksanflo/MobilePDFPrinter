using Newtonsoft.Json;

namespace PDFPrinter.Model
{
    public class TicketObjectDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("port")]
        public string Device { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }
    }
}
