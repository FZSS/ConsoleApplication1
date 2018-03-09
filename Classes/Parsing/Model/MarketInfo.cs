using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Parsing.Model
{
    public class MarketInfo
    {
        public PhotoParameters photo_parameters { get; set; }
        public string name { get; set; }
        public string site_notes { get; set; }
        public string website { get; set; }
        public string currency { get; set; }
        public DateTime start_parse_date { get; set; }
        public DateTime end_parse_date { get; set; }
        public int total_listings_count { get; set; }
        public int total_sizes_count { get; set; }
        public double delivery_to_usa { get; set; }
        public string currently_language { get; set; } //en, ru, de
        public List<string> lagnuages_list { get; set; } // {"en","ru","de"}
        public string listing_type { get; set; } //sneaker, clothes, accessuare

        public MarketInfo()
        {
            photo_parameters = new PhotoParameters();
        }
    }
}
