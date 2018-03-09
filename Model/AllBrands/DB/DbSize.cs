using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.AllBrands.DB
{
    public class DbSize
    {
        public string Us { get; set; }
        public string Eu { get; set; }
        public string Cm { get; set; }
        public string Uk { get; set; }
        public string Ru { get; set; }
        public string Upc { get; set; }
        public List<DbOffer> Offers { get; set; }

        public DbSize()
        {
            Offers = new List<DbOffer>();
        }
    }
}
