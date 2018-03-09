using AngleSharp.Parser.Html;
using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Catalogs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing
{
    public class BrandShopParser : Parser
    {
        const string SITEURL = "https://brandshop.ru";
        private static readonly string FOLDER = DIRECTORY_PATH + @"brandshop\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "stock.csv";
        public const string CURRENCY = "RUB";
        public const string NAME = "brandshop";
        public Encoding SITE_ENCODING = Encoding.UTF8;
        
        string brand { get; set; }

        public void Run()
        {
            catalog = ParseSneakersFromAllPages();
            ParseAllSneakers();

            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);
            
            //ReadCatalogFromCSV(FOLDER + "CatalogBasketshop.ru.csv");
            DownloadAllSneakersImages(FOLDER + @"Images\");
        }

        public void Update()
        {
            UpdateCatalogAndStock();
            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);
        }

        /// <summary>
        /// Обновляем информацию о каталоге и стоке 
        /// </summary>
        /// <returns>возвращает true если были обновления</returns>
        public bool UpdateCatalogAndStock() //не обновил
        {        
            //читаем старый каталог и сток
            ReadCatalogFromCSV(CATALOG_FILENAME);
            stock.ReadStockFromCSV(STOCK_FILENAME);
            MergeCatalogAndStock();

            
            bool isUpdate = false;

            //парсим новый список кроссовок и размеров
            Catalog newCatalog = ParseSneakersFromAllPages();

            foreach (var newSneaker in newCatalog.sneakers)
            {
                Sneaker oldSneaker = catalog.GetSneakerFromSKU(newSneaker.sku);
                if (oldSneaker == null)
                {
                    catalog.sneakers.Add(newSneaker);
                    ParseOneSneaker(catalog.sneakers.Count - 1);
                    DownloadSneakerImages(newSneaker, Config.GetConfig().BasketshopImageFolger);
                    string message = "Новые кроссовки: " + newSneaker.sku + " " + newSneaker.title + " " + newSneaker.link;
                    Program.Logger.Info(message);
                    //Console.WriteLine(message);
                }
                else
                {
                    //проверяем цену
                    if (oldSneaker.price != newSneaker.price)
                    {
                        isUpdate = true;
                        string message = "Price change. sku:" + newSneaker.sku + " old price:" + oldSneaker.price + " new price:" + newSneaker.price;
                        oldSneaker.price = newSneaker.price;
                        Program.Logger.Info(message);
                        //Console.WriteLine(message);
                    }
                    if (oldSneaker.oldPrice != newSneaker.oldPrice)
                    {
                        isUpdate = true;
                        oldSneaker.oldPrice = newSneaker.oldPrice;
                    }
                    //проверяем размеры
                    bool isSizeUpdated = false;
                    foreach (var size in newSneaker.sizes)
                    {
                        SneakerSize oldSize = oldSneaker.GetSize(size.sizeUS);
                        if (oldSize == null)
                        {
                            isSizeUpdated = true;
                            Console.WriteLine("new size. sku:" + newSneaker.sku + " size:" + size.sizeUS);
                        }
                    }
                    //теперь проверяем старые размеры. Если какого-то из этих размеров нет в новых размерах, значит его продали и его надо убрать
                    foreach (var size in oldSneaker.sizes)
                    {
                        SneakerSize newSize = newSneaker.GetSize(size.sizeUS);
                        if (newSize == null)
                        {
                            isSizeUpdated = true;
                            
                            Console.WriteLine("size soldout. sku:" + newSneaker.sku + " size:" + size.sizeUS);
                        }
                    }
                    if (isSizeUpdated)
                    {
                        isUpdate = true;
                        oldSneaker.sizes = newSneaker.sizes;
                    }      
                }
            }
            //проверяем старый каталог. Если в старом каталоге есть кроссовки, которых нет в новом, значит эти кроссы уже не продают и надо их удалить
            List<Sneaker> newSneakers = new List<Sneaker>();
            foreach (var sneaker in catalog.sneakers)
            {
                Sneaker newSneaker = newCatalog.GetSneakerFromSKU(sneaker.sku);
                if (newSneaker == null)
                {
                    isUpdate = true;
                    string message = "Sneaker deleted. sku:" + sneaker.sku;
                    Program.Logger.Info(message);
                    //Console.WriteLine(message);
                }
                else
                {
                    newSneakers.Add(sneaker);
                }
            }
            catalog.sneakers = newSneakers;

            return isUpdate;
        }

        /// <summary>
        /// составляем список всех кроссовок и собираем первичные данные (title цену и другие которые можем отсюда выцепить)
        /// </summary>
        public Catalog ParseSneakersFromAllPages()
        {
            Catalog catalog = new Catalog();

            ParseSneakersFromPage(catalog, "https://brandshop.ru/nike/?mfp=31-kategoriya[%D0%9A%D1%80%D0%BE%D1%81%D1%81%D0%BE%D0%B2%D0%BA%D0%B8]&limit=30");           
            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string link)
        {
            Console.WriteLine("Парсим " + link);
            Uri uri = new Uri(link);
            WebClient webClient = new WebClient();
            webClient.Encoding = SITE_ENCODING;
            string source = webClient.DownloadString(uri);
            var parser = new HtmlParser();
            var document = parser.Parse(source);

            //как определить кроссовок
            var items = document.QuerySelectorAll("div.product");
            
            foreach (var item in items)
            {
                var sneaker = new Sneaker();
                
                sneaker.link = item.QuerySelector("a").GetAttribute("href");
                sneaker.brand = "Nike";
                //fullCatalogSneaker.sku = item.QuerySelector("div.art").InnerHtml;

                //price
                var priceDiv = item.QuerySelector("div.price");
                string priceStr;
                if (priceDiv.QuerySelector("del") != null) {
                    priceStr = priceDiv.QuerySelector("del").InnerHtml;
                    priceStr = priceStr.Substring(0, priceStr.IndexOf("<em")).Replace(" ", "");
                    sneaker.oldPrice = Int32.Parse(priceStr);

                    //oldprice
                    priceStr = priceDiv.InnerHtml;
                    priceStr = priceStr.Substring(0, priceStr.IndexOf("<em"));
                    priceStr = priceStr.Replace(" ", "");
                    sneaker.price = Int32.Parse(priceStr);
                }
                else {
                    priceStr = priceDiv.InnerHtml;
                    priceStr = priceStr.Substring(0, priceStr.IndexOf("<em"));
                    priceStr = priceStr.Replace(" ", "");
                    sneaker.price = Int32.Parse(priceStr);
                }
                
                //title
                sneaker.title = item.QuerySelector("a").GetAttribute("title");
                sneaker.title = sneaker.title.Replace("Мужские", "").Replace("Женские", "").Replace("Детские", "").Replace("Подростковые", "").Trim();
                sneaker.title = sneaker.title.Replace("Кроссовки", "").Replace("кроссовки", "").Trim();
                sneaker.ParseTitle();

                
                //fullCatalogSneaker.brand = this.brand;

                //sizes
                //string[] sizesStrArr = item.QuerySelector("div.item_sizes").InnerHtml.Split(',');
                //foreach (var sizeUS in sizesStrArr)
                //{

                //    SneakerSize sneakerSize = new SneakerSize(fullCatalogSneaker, sizeUS.Trim());
                //    fullCatalogSneaker.sizes.Add(sneakerSize);
                //}

                //catalog.sneakers.Add(stockSneaker);
                catalog.AddUniqueSneaker(sneaker);
            }

            //next page
            var nextlinks = document.QuerySelector("div.pn").QuerySelectorAll("a.textlink");
            foreach (var nextlink in nextlinks)
            {
                if (nextlink != null)
                {
                    if (nextlink.InnerHtml == "далее")
                    {
                        string nextPageLink = nextlink.GetAttribute("href");
                        Thread.Sleep(100);
                        ParseSneakersFromPage(catalog, nextPageLink);
                    }

                }
            }

        }

        /// <summary>
        /// парсим каждый кроссовок. на выходе получаем заполненный объект sneakers со всеми данными (размеры, изображения и т.д.)
        /// </summary>
        public void ParseAllSneakers()
        {
            int count = catalog.sneakers.Count;
            List<Sneaker> newSneakers = new List<Sneaker>();
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(i + " " + catalog.sneakers[i].link);
                
                Sneaker sneaker = ParseOneSneaker(i);
                if (sneaker != null) newSneakers.Add(sneaker);
                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                Thread.Sleep(100);
                //sneakerParser.DownloadSneakerImages(i);
            }
            catalog.sneakers = newSneakers;


        }

        //public Sneaker ParseOneSneaker(int index) { }

        public Sneaker ParseOneSneaker(int index)
        {
            Sneaker sneaker = @catalog.sneakers[index];
            string url = sneaker.link;
            //url = "https://brandshop.ru/goods/20463/muzhskie-krossovki-nike-zoom-talaria-mid-flyknit-baroque-brown/";
            Uri uri = new Uri(url);
            WebClient webClient = new WebClient();
            webClient.Encoding = SITE_ENCODING;
            string source = String.Empty;
            try
            {
                source = webClient.DownloadString(uri);
            }
            catch (WebException e)
            {
                //catalog.sneakers.RemoveAt(index);
                Program.Logger.Warn("Ошибка загрузки страницы. ссылка: " + sneaker.link);
                return null;
            }

            var parser = new HtmlParser();
            var document = parser.Parse(source);

            //sku
            sneaker.sku = document.QuerySelector("div.row.description").QuerySelector("div.row").QuerySelector("div.col-60").InnerHtml;

            //images
            var imagesHTML = document.QuerySelector("div.product-image-big").QuerySelectorAll("img");
            foreach (var image in imagesHTML)
            {
                string imageString = image.GetAttribute("data-lazy");
                sneaker.images.Add(imageString);
            }

            //sex
            string sex = document.QuerySelector("div.breadcrumb").QuerySelectorAll("a")[1].InnerHtml;
            switch (sex)
            {
                case "Мужское":
                    sneaker.category = Settings.CATEGORY_MEN;
                    sneaker.sex = Settings.GENDER_MAN;
                    break;
                case "Женское":
                    sneaker.category = Settings.CATEGORY_WOMEN;
                    sneaker.sex = Settings.GENDER_WOMAN;
                    break;
                case "Детское":
                    sneaker.category = Settings.CATEGORY_KIDS;
                    break;
                case "Подарки":
                    sneaker.category = Settings.CATEGORY_MEN;
                    sneaker.sex = Settings.GENDER_MAN;
                    break;
                default:
                    Console.WriteLine("Категория не определена. sku: " + sneaker.sku);
                    Program.Logger.Warn("Категория не определена. sku: " + sneaker.sku);
                    //throw new Exception("Категория не определена.");
                    break;
            }

            //sizes


            return sneaker;
        }
    }
}
