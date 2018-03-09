using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Exporters
{
    public class SneakerIconRecord
    {
        public string sku { get; set; }
        public string sku2 { get; set; }
        public string upc { get; set; }
        public string brand { get; set; }
        public string title { get; set; }
        public string color { get; set; }
        public string size { get; set; }
        public string sizeUS { get; set; }
        public string sizeUK { get; set; }
        public string sizeEU { get; set; }
        public string sizeCM { get; set; }
        public string sizeRU { get; set; }       
        public string sex { get; set; }
        public string category { get; set; }
        public string collection { get; set; }
        public string type { get; set; }       
        public string destination { get; set; }
        public string quantity { get; set; }
        public string price { get; set; }
        public string description { get; set; }
        public string images { get; set; }

        public bool SetParametersFromSneaker2(Sneaker sneaker)
        {
            quantity = "1";

            return true; 
        }

        public bool SetParametersFromSneaker(Sneaker sneaker)
        {
            //brand
            brand = sneaker.brand;

            //title
            title = sneaker.title + sku;
            if (!String.IsNullOrWhiteSpace(sneaker.type))
            {
                title = sneaker.type + " " + title;

            }

            //description
            string desc = title + "\r\n";
            desc += "Артикул: " + sneaker.sku + "\r\n";
            desc += "По поводу размеров звоните или оставляйте заявку.\r\n";
            desc += "------------------------\r\n";
            desc += "Вся обувь оригинальная, новая, в упаковке\r\n";
            desc += "------------------------\r\n";
            desc += "Доставка по России 2-3 дня компанией СДЭК.\r\n";
            desc += "Стоимость доставки по РФ - 300 рублей.\r\n";
            description = desc;

            //categorySneakerFullCatalog
            category = sneaker.category;
            if (String.IsNullOrWhiteSpace(sneaker.category)) return false;

            //sex
            sex = sneaker.sex;
            if (String.IsNullOrWhiteSpace(sneaker.sex)) return false;

            //images
            var images = sneaker.images;
            if (images == null)
            {
                return false;
            }
            else
            {
                if (images.Count == 1 && String.IsNullOrWhiteSpace(images[0]))
                {
                    return false;
                }
                else
                {
                    this.images = String.Join("|", images);
                }
            }

            //sizes


            return true;
            //throw new NotImplementedException();
        }
    }
}
