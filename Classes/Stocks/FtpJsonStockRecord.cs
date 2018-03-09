using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Stocks
{
    public class FtpJsonStockRecord
    {
        public string market { get; set; }
        public string website { get; set; }
        public string currency { get; set; }
        public string parse_date { get; set; }
        public string delivery_to_usa { get; set; }
        public bool is_watermark_image { get; set; }
        public bool is_white_background { get; set; }
        public string lang { get; set; }
    }
}
