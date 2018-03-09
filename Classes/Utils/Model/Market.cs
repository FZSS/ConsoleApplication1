using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SneakerIcon.Classes.Utils.Model
{
    public class Market
    {
        public string name { get; set; }
        public string currency { get; set; }
        public int delivery_to_usa { get; set; }
        public int margin { get; set; }
        public int vat_value { get; set; }
    }
}
