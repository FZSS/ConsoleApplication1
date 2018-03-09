using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.AllStock
{
    public class AllStockRoot
    {
        public DateTime update_time { get; set; }
        public List<AllStockSneaker> sneakers { get; set; }

        public AllStockRoot()
        {
            sneakers = new List<AllStockSneaker>();
        }

        public int GetCountSizes() {
            int count = 0;
            foreach (var sneaker in sneakers)
	        {
                count += sneaker.sizes.Count;		 
	        }
            return count;
        }

        public int GetCountOffers()
        {
            int count = 0;
            foreach (var sneaker in sneakers)
            {
                foreach (var size in sneaker.sizes)
                {
                    count += size.offers.Count;
                }
            }
            return count;
        }
    }
}
