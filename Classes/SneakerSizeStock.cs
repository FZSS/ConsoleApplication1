using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes
{
    public class SneakerSizeStock
    {
        public string stockName;
        public int quantity;
        public double price;
        public double buy_price;

        public SneakerSizeStock()
        {

        }

        public SneakerSizeStock(string stockName, int quantity, double price, double buy_price)
        {
            this.stockName = stockName;
            this.quantity = quantity;
            this.price = price;
            this.buy_price = buy_price;
        }
    }
}
