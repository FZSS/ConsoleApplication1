using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Exporters;

namespace SneakerIcon.Model.FullCatalog
{
    public class FullCatalogRoot
    {
        public DateTime update_time { get; set; }
        public List<FullCatalogRecord> records { get; set; }

        public FullCatalogRoot()
        {
            records = new List<FullCatalogRecord>();
        }

        public void SaveToFtp()
        {
            FullCatalog2.SaveFullCatalogToFtp(this);
        }
    }
}
