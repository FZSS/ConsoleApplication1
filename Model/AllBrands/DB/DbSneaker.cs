using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.AllBrands.DB
{
    public class DbSneaker
    {
        public int Id { get; set; }
        public String Brand { get; set; }
        public String Sku { get; set; }
        public String Title { get; set; }
        public List<String> Titles { get; set; }
        public String Color { get; set; }
        public String Collection { get; set; }
        public String Type { get; set; }
        public string Height { get; set; }
        public String Destination { get; set; }
        public String Description { get; set; }
        public String Category { get; set; }
        public List<String> Links { get; set; }       
        public List<DbImageCollection> ImageCollectionList { get; set; }
        public List<DbSize> Sizes { get; set; }

        public DbSneaker()
        {
            ImageCollectionList = new List<DbImageCollection>();
            Sizes = new List<DbSize>();
            Links = new List<string>();
            Titles = new List<string>();
        }
    }
}
