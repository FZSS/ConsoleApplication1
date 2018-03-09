using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.AllBrands.DB
{
    public class DbRoot
    {
        public DateTime UpdateTime { get; set; }
        public List<DbSneaker> Sneakers { get; set; }

        public DbRoot()
        {
            Sneakers = new List<DbSneaker>();
        }
    }
}
