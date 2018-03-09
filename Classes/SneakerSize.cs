using SneakerIcon.Classes;
using SneakerIcon.Classes.SizeConverters;
using SneakerIcon.Classes.SizeConverters.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes
{
    public class SneakerSize
    {
        public string sizeUS { get; set; }
        public string sizeUK { get; set; }
        public string sizeEU { get; set; }
        public string sizeCM { get; set; }
        public string sizeRU { get; set; }
        public string upc { get; set; }
        public int quantity { get; set; }
        public string sku { get; set; }
        public string condition { get; set; }
        public string allSize { get; set; }
        public double price { get; set; }
        public double oldPrice { get; set; }
        public List<SneakerSizeStock> stock;



        public SneakerSize()
        {
            stock = new List<SneakerSizeStock>();
        }

        public SneakerSize(Sneaker sneaker, string sizeUS)
        {
            sizeUS = sizeUS.Replace(',', '.');
            stock = new List<SneakerSizeStock>();
            this.sku = sneaker.sku + "-" + sizeUS;
            this.sizeUS = sizeUS;
            //GetAllSizesFromUS(stockSneaker, sizeUS);
        }

        public SneakerSize(string sizeUS)
        {
            sizeUS = sizeUS.Replace(',', '.');
            this.sizeUS = sizeUS;
        }

        public bool GetAllSizesFromUS (Sneaker sneaker, string sizeUS) {
            try
            {
                if (String.IsNullOrEmpty(sneaker.category)) throw new Exception("Не указана категория размерной сетки. Артикул: " + sneaker.sku);
                var converter = new SizeConverter(sizeUS, sneaker.category);
                this.sizeUS = sizeUS;
                this.sizeEU = converter.sizeEUR;
                this.sizeUK = converter.sizeUK;
                this.sizeRU = converter.sizeRUS;
                this.sizeCM = converter.sizeCM;
                this.allSize = converter.allSize;
                return true;
            }
            catch (Exception e)
            {
                string message = "Размер и категория не совпадают. sku:" + sneaker.sku + " size:" + sizeUS + " категория: " + sneaker.category;
                Program.Logger.Warn(message);
                return false;
                //Console.WriteLine(message);               
            }
        }

        public SneakerSizeStock GetMinPriceStock()
        {
            double minPrice = 1000000;
            int index = -1;

            for (int i = 0; i < this.stock.Count; i++)
            {
                if (stock[i].price < minPrice)
                {
                    minPrice = stock[i].price;
                    index = i;
                }
            }

            return stock[index];
        }

        public static bool ValidateCondition(string condition) {
            List<string> values = new List<string>();
            values.Add("new with box");
            values.Add("new with cut top box");
            values.Add("new without box");
            values.Add("used");

            foreach (var value in values)
            {
                if (condition.ToLower() == value)
                {
                    condition = condition.ToLower();
                    return true;
                }
                    
            }
            return false;
        }
    }
}
