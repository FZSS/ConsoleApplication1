using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.AllBrands.DB
{
    public class DbOffer 
    {
        /// <summary>
        /// стоимость товара в долларах с учетом доставки и вычета налога ват
        /// </summary>
        public double price_usd_with_delivery_to_usa_and_minus_vat { get; set; }
        //public double price_rub_with_delivery_to_usa_and_minus_vat { get; set; } //допилю попозже


        public string currency { get; set; }

        /// <summary>
        /// название магазина, откуда подгружен оффер (для магазинов без ссылок, например чтобы понять из дисконта это или из нашего стока
        /// </summary>
        public string stock_name { get; set; }

        /// <summary>
        /// price in original currency
        /// </summary>       
        public double price { get; set; }

        /// <summary>
        /// if sale price exist this var contains regular price, sale price in var price
        /// </summary>
        public double old_price { get; set; }

        public string url { get; set; }
    }
}
