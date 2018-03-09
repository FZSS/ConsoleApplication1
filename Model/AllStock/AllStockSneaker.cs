using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.AllStock
{
    public class AllStockSneaker
    {
        public string sku { get; set; }
        public string brand { get; set; }
        public string title { get; set; }
        /// <summary>
        /// men, women, kids
        /// </summary>
        public string category { get; set; }
        public List<AllStockSize> sizes { get; set; }

        public AllStockSneaker()
        {
            sizes = new List<AllStockSize>();
        }

        public double GetMaxPrice()
        {
            if (sizes.Count == 0)
                return 0;

            double maxPrice = 0;
            foreach (var size in sizes)
            {
                if (size.offers == null)
                    return 0;
                if (size.offers.Count == 0)
                    return 0;

                var offer = size.offers[0];
                if (maxPrice < offer.price_usd_with_delivery_to_usa_and_minus_vat)
                    maxPrice = offer.price_usd_with_delivery_to_usa_and_minus_vat;
            }

            return maxPrice;
        }

        public double GetMinPrice()
        {
            if (sizes.Count == 0)
                return 0;

            double minPrice = 10000000;
            foreach (var size in sizes)
            {
                if (size.offers == null)
                    return 0;
                if (size.offers.Count == 0)
                    return 0;

                var offer = size.offers[0];
                if (minPrice > offer.price_usd_with_delivery_to_usa_and_minus_vat)
                    minPrice = offer.price_usd_with_delivery_to_usa_and_minus_vat;
            }

            return minPrice;
        }
    }
}
