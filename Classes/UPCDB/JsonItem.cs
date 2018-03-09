using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SneakerIcon.Classes.UPCDB
{
    public class JsonItem
    {
        public string ean { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string upc { get; set; }
        public string gtin { get; set; }
        public string asin { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public string color { get; set; }
        public string size { get; set; }
        public string dimension { get; set; }
        public string weight { get; set; }
        public string currency { get; set; }
        public double lowest_recorded_price { get; set; }
        public List<string> images { get; set; }
        public List<JsonOffer> offers { get; set; }
    }
}
