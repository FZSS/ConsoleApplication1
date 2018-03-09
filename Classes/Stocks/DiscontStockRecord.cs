using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Stocks
{
    public class DiscontStockRecord
    {
        public string sku { get; set; }
        public string size { get; set; }
        public string upc { get; set; }
        public int quantity { get; set; }
        public string title { get; set; }
        /// <summary>
        /// Себестоимость
        /// </summary>
        public double price { get; set; }
        /// <summary>
        /// Старая цена закупки без учета скидки (для интернет магазинов для товаров по сейлу)
        /// </summary>
        public double oldPrice { get; set; }
        public bool isBackWall { get; set; }
        public string category { get; set; }
    }
}
