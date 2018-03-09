using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.HotOffersModel
{
    public class HotOffer
    {
        public string sku { get; set; }
        public string brand { get; set; }
        public string title { get; set; }
        public string category { get; set; }
        public double regular_price { get; set; }
        
        /// <summary>
        /// цена с учетом доставки и вычета налога ват. максимальная среди всех размеров
        /// </summary>
        public double our_price { get; set; }
        public List<HotOfferSize> sizes { get; set; }
        public List<string> images { get; set; }
        public DateTime add_time { get; set; }

        public HotOffer()
        {
            sizes = new List<HotOfferSize>();
            images = new List<string>();
        }

        public double GetOurPrice() {
            double maxPrice = 0;
            foreach (var size in sizes)
            {
                if (size.price > maxPrice)
                    maxPrice = size.price;
            }
            return maxPrice;
        }

        
    }
}
