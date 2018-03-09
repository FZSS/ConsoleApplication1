using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Exporters.Fancy
{
    public class FancyRecord
    {
        public string title { get; set; }
        public string description { get; set; }
        public string is_active { get; set; }
        public int quantity { get; set; }
        public double price { get; set; }
        public double retail_price { get; set; }
        public string image1_url { get; set; }
        public string image2_url { get; set; }
        public string image3_url { get; set; }
        public string categories { get; set; }
        public string option_name1 { get; set; }
        public int option_quantity1 { get; set; }
        public double option_price1 { get; set; }
        public string sale_start_date { get; set; }
        public string seller_sku { get; set; }
        public string prod_colors { get; set; }
        public string prod_country_of_origin { get; set; }
        public string prod_length { get; set; }
        public string prod_height { get; set; }
        public string prod_width { get; set; }
        public string prod_weight { get; set; }
        public string charge_shipping { get; set; }
        public string charge_international_shipping { get; set; }
        public string international_shipping { get; set; }
        public string us_window_start { get; set; }
        public string us_window_end { get; set; }
        public string intl_start { get; set; }
        public string intl_end { get; set; }

        public FancyRecord()
        {
            is_active = "true";
            quantity = 2;
            option_quantity1 = 2;
            sale_start_date = "2014-01-01 00:00:00";
            prod_country_of_origin = "CN";
            charge_shipping = "true";
            charge_international_shipping = "true";
            international_shipping = "true";
            us_window_start = "10";
            us_window_end = "30";
            intl_start = "10";
            intl_end = "30";
        }
    }
}
