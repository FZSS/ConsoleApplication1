using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Stocks
{
    public class StockRecord : IStockRecord
    {
        //public string stockName { get; set; }
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
        /// <summary>
        /// Цена продажи
        /// </summary>
        public double sellPrice { get; set; }
        /// <summary>
        /// Ссылка на товар (для интернет магазинов)
        /// </summary>
        public string link { get; set; }

        public string GetSKU2()
        {
            return this.sku + "-" + this.size;
        }
    }
}
