using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Exporters
{
    public class AllStockRecord
    {
        public string sku { get; set; }
        public string size { get; set; }
        public string sku2 {
            get
            {
                return sku + "-" + size;
            }
        }
        public string brand { get; set; }
        public string title { get; set; }
        public int nash_quantity { get; set; }
        public double nash_price { get; set; }
        public double nashSellPrice { get; set; }     
        public int discont_quantity { get; set; }
        public double discont_price { get; set; }
        //public int kuzminki_quantity { get; set; }
        //public double kuzminki_price { get; set; }
        public double queens_price { get; set; }
        public double streetbeat_price { get; set; }
        public double basketshop_price { get; set; }
        public double einhalb_price { get; set; }
        public double sivas_price { get; set; }
        public double titolo_price { get; set; }
        public double sneakerstuff_price { get; set; }
        public double overkill_price { get; set; }
        public double solehavenPrice { get; set; }
        public double afewPrice { get; set; }
        public double suppaStorePrice { get; set; }
        public double chmielnaPrice { get; set; }
        public double bdgastorePrice { get; set; }
        public double asfaltgoldPrice { get; set; }

        //links
        public string queensLink { get; set; }
        public string strBeatLink { get; set; }
        public string basketShopLink { get; set; }
        public string einhalbLink { get; set; }
        public string sivasLink { get; set; }
        public string titoloLink { get; set; }
        public string sneakerstuffLink { get; set; }
        public string overkilllink { get; set; }
        public string solehavenLink { get; set; }
        public string afewLink { get; set; }
        public string suppaLink { get; set; }
        public string chmielnaLink { get; set; }
        public string bdgastoreLink { get; set; }
        public string asfaltgoldLink { get; set; }

        public AllStockRecord()
        {

        }

        //public double getMinPrice()
        //{
        //    double minPrice = 1000000;
        //    int quantity = -1;
        //    string stockName = String.Empty;
        //    if (nash_quantity > 0)
        //    {
        //        minPrice = nash_price;
        //        quantity = nash_quantity;
        //        stockName = "nashstock";
        //    }
        //    if (queens_quantity > 0 && queens_price < minPrice)
        //    {
        //        minPrice = queens_price;
        //        quantity = queens_quantity;
        //        stockName = "queens";
        //    }
        //    if (discont_quantity > 0 && discont_price < minPrice)
        //    {
        //        minPrice = discont_price;
        //        quantity = discont_quantity;
        //        stockName = "discont";
        //    }
        //    if (streetbeat_quantity > 0 && streetbeat_price < minPrice)
        //    {
        //        minPrice = streetbeat_price;
        //        quantity = streetbeat_quantity;
        //        stockName = "streetbeat";
        //    }
        //    if (quantity == -1)
        //    {
        //        throw new Exception("Артикул " + sku + " размер " + sizeUS + " не найдена мин цена");
        //    }
        //    return minPrice;
        //}



    }
}
