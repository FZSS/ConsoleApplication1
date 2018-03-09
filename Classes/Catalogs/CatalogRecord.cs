using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes
{
    public class CatalogRecord
    {
        public String style { get; set; }
        public String colorcode { get; set; }
        public String sku { get; set; }
        public String title { get; set; }
        public String color { get; set; }
        public String brand { get; set; }
        public String collection { get; set; }
        public String type { get; set; }
        public string height { get; set; }
        public String destination { get; set; }
        public String description { get; set; }
        public String category { get; set; }
        public String sex { get; set; }
        public String images { get; set; }
        public String link { get; set; }
        public String price { get; set; }
        public String oldPrice { get; set; }

        public CatalogRecord()
        {

        }
        public CatalogRecord(Sneaker sneaker)
        {
            this.style = sneaker.style;
            this.colorcode = sneaker.colorcode;
            this.sku = sneaker.sku;
            this.title = sneaker.title;
            this.color = sneaker.color;
            this.brand = sneaker.brand;
            this.collection = sneaker.collection;
            this.type = sneaker.type;
            this.height = sneaker.height;
            this.destination = sneaker.destination;
            if (sneaker.description != null)
            {
                this.description = sneaker.description.Replace("\n", "");
            }
            this.category = sneaker.category;
            this.sex = sneaker.sex;
            this.images = String.Join("|", sneaker.images);
            this.link = sneaker.link;
            this.price = sneaker.price.ToString();
            this.oldPrice = sneaker.oldPrice.ToString();
        }

        public Sneaker ReadSneakerFromCatalogRecord()
        {
            Sneaker sneaker = new Sneaker();
            sneaker.style = this.style;
            sneaker.colorcode = this.colorcode;
            sneaker.sku = this.sku;
            sneaker.title = this.title;
            sneaker.color = this.color;
            sneaker.brand = this.brand;
            sneaker.collection = this.collection;
            sneaker.type = this.type;
            sneaker.height = this.height;
            sneaker.destination = this.destination;
            sneaker.description = this.description;
            sneaker.category = this.category;
            sneaker.sex = this.sex;
            sneaker.images = this.images.Split('|').ToList();
            sneaker.link = this.link;
            sneaker.price = Double.Parse(this.price);
            sneaker.oldPrice = Double.Parse(this.oldPrice);
            return sneaker;
        }
    }
}
