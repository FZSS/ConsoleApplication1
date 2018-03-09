using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.BonanzaModel
{
    class BonanzaRoot
    {
        public DateTime update_date { get; set; }
        public List<BonanzaRecord> Records { get; set; }

        public BonanzaRoot()
        {
            Records = new List<BonanzaRecord>();
        }
    }
}
