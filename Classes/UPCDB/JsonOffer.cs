using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SneakerIcon.Classes.UPCDB
{
    public class JsonOffer
    {
        public string merchant { get; set; }
        public string domain { get; set; }
        public string title { get; set; }
        public string currency { get; set; }
        public double? list_price { get; set; }
        public double price { get; set; }
        public string shipping { get; set; }
        public string condition { get; set; }
        public string availability { get; set; }
        public string link { get; set; }
        public int updated_t { get; set; }
    }
}
