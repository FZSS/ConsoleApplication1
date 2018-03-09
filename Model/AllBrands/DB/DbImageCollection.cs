using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.AllBrands.DB
{
    public class DbImageCollection
    {
        public string ShopName { get; set; }
        public List<DbImage> Images { get; set; }

        public DbImageCollection()
        {
            Images = new List<DbImage>();
        }
    }
}
