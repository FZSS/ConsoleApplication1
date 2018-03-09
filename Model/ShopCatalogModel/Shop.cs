using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.ShopCatalogModel
{
    public class Shop
    {
        public int number { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
        public int delivery_to_usa { get; set; }
        public int margin { get; set; }
        public double vat_value { get; set; }
        public string language { get; set; }
        public List<ShopCatalogDelivery> delivery { get; set; }
        public List<string> active_marketplace_list { get; set; }
    }
}
