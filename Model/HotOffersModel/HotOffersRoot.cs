using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.HotOffersModel
{
    public class HotOffersRoot
    {
        public DateTime update_time { get; set; }
        public List<HotOffer> offers { get; set; }

        public HotOffersRoot()
        {
            offers = new List<HotOffer>();
        }
    }
}
