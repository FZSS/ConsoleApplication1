using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.AllStock
{
    public class AllStockSize
    {
        public string us { get; set; }
        public string sku2 { get; set; }
        public string upc { get; set; }
        public List<AllStockOffer> offers { get; set; }

        public AllStockSize()
        {
            offers = new List<AllStockOffer>();
        }
    }
}
