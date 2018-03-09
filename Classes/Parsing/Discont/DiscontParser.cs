using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Stocks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing.Discont
{
    public class DiscontParser : Parser
    {
        internal Catalogs.Catalog ParseStock(DiscontStock DiscontSamara)
        {
            var newCatalog = new Catalogs.Catalog();

            //тут категория проверяется из фулкаталога, поэтому сначала нужно обновлять фулкаталог и только потом делать markets
            foreach (var item in DiscontSamara.Records2)
            {
                var sneaker = newCatalog.sneakers.Find(x => x.sku == item.sku);
                if (sneaker == null)
                {
                    sneaker = new Sneaker();
                    sneaker.sku = item.sku;

                    //title 
                    sneaker.title = item.title;
                    sneaker.title = sneaker.title.Replace("*", "");
                    sneaker.title = sneaker.title.Replace("JRD", "JORDAN");

                    //brand
                    if (sneaker.title.Contains("JORDAN"))
                    {
                        sneaker.brand = "Jordan";
                    }
                    else
                    {
                        sneaker.brand = "Nike";
                        //добавляем к тайтлу бренд, если его там нет
                        if (!sneaker.title.ToLower().Contains(sneaker.brand.ToLower()))
                            sneaker.title = sneaker.brand + " " + sneaker.title;
                    }



                    sneaker.category = item.category;
                    Helper.GetSexFromCategory(sneaker.category);
                    sneaker.price = item.price;
                    sneaker.oldPrice = item.oldPrice;

                    if (item.quantity > 0)
                    {
                        //sizes
                        sneaker.AddSize(item.size, item.quantity, item.upc);

                        newCatalog.AddUniqueSneaker(sneaker);
                    }
                }
                else //если артикул уже есть в каталоге
                {
                    //проверяем, есть ли уже этот размер у кроссовка
                    var size = sneaker.GetSize(item.size);
                    if (size == null)
                    {
                        if (item.quantity > 0)
                        {
                            sneaker.AddSize(item.size, item.quantity, item.upc);
                        }
                    }
                    else
                    {
                        Program.Logger.Warn("Дубликат размера в нашем стоке: sku:" + item.sku + " size:" + item.size);
                    }
                }
            }

            return newCatalog;
            //throw new NotImplementedException();
        }
    }
}
