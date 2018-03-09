using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Parsing.SivasSale
{
    public class ListingSaleSivas
    {
        public object spec_set { get; set; }
        public long id { get; set; }
        public string url { get; set; }
        public string product_code { get; set; }
        public string title { get; set; }
        public string brand { get; set; }
        public string colorbrand { get; set; }
        public string collection { get; set; }
        public string category { get; set; }
        public string sex { get; set; }
        public double price { get; set; }
        public double old_price { get; set; }
        public List<string> images { get; set; }
        public List<string> us_sizes { get; set; }
        public List<string> uk_sizes { get; set; }
        public List<string> eu_sizes { get; set; }
        public List<string> cm_sizes { get; set; }
        public int position { get; set; }

        public ListingSaleSivas()
        {
            spec_set = new object();
        }
    }
}
