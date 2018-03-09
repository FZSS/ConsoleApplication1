using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.UPCDB
{
    public class JsonRootObject
    {
        public string code { get; set; }
        public int total { get; set; }
        public int offset { get; set; }
        public List<JsonItem> items { get; set; }
    }
}
