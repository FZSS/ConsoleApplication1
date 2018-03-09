using SneakerIcon.Classes.Parsing.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Parsing.SivasSale
{
    public class RootObjectSaleSivas
    {
        public MarketInfo market_info { get; set; }
        public List<ListingSaleSivas> listings { get; set; }
    }
}
