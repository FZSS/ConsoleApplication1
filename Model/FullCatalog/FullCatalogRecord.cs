using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.FullCatalog
{
    public class FullCatalogRecord
    {
        public String brand { get; set; }
        public String sku { get; set; }
        public String title { get; set; }
        public String color { get; set; }
        public String collection { get; set; }
        public String type { get; set; }
        public string height { get; set; }
        public String destination { get; set; }
        public String description { get; set; }
        public String category { get; set; }
        public String sex { get; set; }
        public String link { get; set; }
        public DateTime add_time { get; set; }
        public List<String> images { get; set; }

        public FullCatalogRecord()
        {
            images = new List<string>();
        }
    }
}
