using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SneakerIcon.Classes.UPCDB
{
    public class SneakerJson
    {       
        public string brand { get; set; }
        public string title { get; set; }
        public string sku { get; set; }
        public string category { get; set; }
        public string sex { get; set; }
        public DateTime add_time { get; set; }
        public List<JsonItem> sizes { get; set; }

        public SneakerJson()
        {
            sizes = new List<JsonItem>();
        }
    }
}
