using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Stocks
{
    public class NashStockRecord
    {
        public string brand { get; set; }
        public string condition { get; set; }
        public string sku { get; set; }
        public string size { get; set; }
        public string sku2 { get; set; }
        public string upc { get; set; }
        public double price { get; set; }
        public double sellPrice { get; set; }
        public int quantity { get; set; }
        public double capacity { get; set; }
        public string category { get; set; }
        public string title { get; set; }
        public string comment { get; set; }

        public string GetSKU2()
        {
            return this.sku + "-" + this.size;
        }

        public bool Validate()
        {
            NashStockRecord stockRecord = this;
            //brand
            if (String.IsNullOrWhiteSpace(stockRecord.brand))
            {
                Program.Logger.Warn("Empty brand sku:" + stockRecord.sku2);
                return false;
            }

            //condition
            if (!SneakerSize.ValidateCondition(stockRecord.condition))
            {
                Program.Logger.Warn("Wrong condition sku:" + stockRecord.sku2);
                return false;
            }

            //sku
            if (String.IsNullOrWhiteSpace(stockRecord.sku))
            {
                Program.Logger.Warn("Empty sku sku:" + stockRecord.sku2);
                return false;
            }

            //sizeUS
            if (String.IsNullOrWhiteSpace(stockRecord.size))
            {
                Program.Logger.Warn("Empty size sku:" + stockRecord.sku2);
                return false;
            }

            //sku2
            if (String.IsNullOrWhiteSpace(stockRecord.sku2))
            {
                Program.Logger.Warn("Empty sku2 sku:" + stockRecord.sku2);
                return false;
            }

            //upc
            string upc = stockRecord.upc;
            if (!String.IsNullOrEmpty(upc))
            {
                if (upc.Count() == 11)
                {
                    if (stockRecord.brand == "Nike" || stockRecord.brand == "Jordan")
                        stockRecord.upc = "0" + upc;
                }
            }

            //price
            if (stockRecord.price <= 0)
                return false;

            //quantity
            if (stockRecord.quantity == 0)
                return false;

            //categorySneakerFullCatalog
            if (stockRecord.category != "men" && stockRecord.category != "women" && stockRecord.category != "kids")
            {
                Program.Logger.Warn("Wrong category. SKU2:" + stockRecord.sku2);
                return false;
            }
            if (stockRecord.category == "men") stockRecord.category = Settings.CATEGORY_MEN;
            if (stockRecord.category == "women") stockRecord.category = Settings.CATEGORY_WOMEN;
            if (stockRecord.category == "kids") stockRecord.category = Settings.CATEGORY_KIDS;

            //title
            if (String.IsNullOrWhiteSpace(stockRecord.title))
            {
                Program.Logger.Warn("Empty title SKU2:" + stockRecord.sku2);
                return false;
            }

            return true;
            //throw new NotImplementedException();
        }
    }
}
