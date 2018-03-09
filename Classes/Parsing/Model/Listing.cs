using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Parsing.Model
{
    public class Listing
    {
        public object spec_set { get; set; }
        public long id { get; set; }
        public string url { get; set; }
        public string sku { get; set; }
        public string title { get; set; }
        public string brand { get; set; }
        public string colorbrand { get; set; }
        public string collection { get; set; }
        public string category { get; set; }
        public string sex { get; set; }
        public double price { get; set; }
        public double old_price { get; set; }
        public List<string> images { get; set; }
        public List<ListingSize> sizes { get; set; }
        public int position { get; set; }

        public Listing()
        {
            spec_set = new object();
        }

        public string GetCategory() {
            //если категория нул, то пробуем ее определить по другим размерам
            if (String.IsNullOrWhiteSpace(category))
            {
                if (sizes != null)
                {
                    if (sizes.Count > 0)
                    {
                        var sizeitem = sizes[0];
                        //проверяем, может быть размер детский
                        if (sizeitem.us != null)
                        {
                            if (sizeitem.us.Contains("Y") || sizeitem.us.Contains("C"))
                            {
                                category = "kids";
                                return category;
                            }
                        }
                        //если юс размера нет или есть но категория не детская, то пробуем определить размер по разным размерным сеткам (если они есть)
                        category = SizeConverters.SizeConverter.GetCategory(sizeitem.us, sizeitem.eu, sizeitem.uk, sizeitem.cm, brand);
                        return category;
                    }
                }
            }
            else
            {
                return category;
            }
            return null;
        }
    }
}
