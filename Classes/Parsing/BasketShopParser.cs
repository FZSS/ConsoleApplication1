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
    public class BasketShopParser : Parser
    {
        const string SITEURL = "http://www.basketshop.ru";
        private static readonly string FOLDER = DIRECTORY_PATH + @"basketshop.ru\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "ru";
        public const string CURRENCY = "RUB";
        public const string NAME = "basketshop.ru";
        public const int MARZHA = DELIVERY_TO_USA + 2000; 
        public const int DELIVERY_TO_USA = 1500;
        public const double VAT_VALUE = 0; //Если 15% ват, то пишем 0.15, если вычета нет, то 0
        
        string brand { get; set; }

        public void Run()
        {
            catalog = ParseSneakersFromAllPages();
            ParseAllSneakers();
            SaveCatalogToCSV(Config.GetConfig().BasketshopCatalogFilename);
            SaveStockToCSV(Config.GetConfig().BasketshopStockFilename);
            
            //ReadCatalogFromCSV(FOLDER + "CatalogBasketshop.ru.csv");
            //DownloadAllSneakersImages(FOLDER + @"Images\");
            var market_info = new Parsing.Model.MarketInfo();
            market_info.name = NAME;
            market_info.website = SITEURL;
            market_info.currency = CURRENCY;
            market_info.start_parse_date = DateTime.Now;
            market_info.end_parse_date = DateTime.Now;
            market_info.delivery_to_usa = DELIVERY_TO_USA;
            market_info.photo_parameters.is_watermark_image = false;
            market_info.photo_parameters.background_color = "white";
            market_info.photo_parameters.first_photo_sneaker_direction = "left"; //уточнить
            market_info.currently_language = "en";
            var json = CreateJson(market_info);
            SaveJson(json, JSON_FILENAME, FOLDER, NAME);
        }

        public void Update()
        {
            UpdateCatalogAndStock();
            SaveCatalogToCSV(Config.GetConfig().BasketshopCatalogFilename);
            SaveStockToCSV(Config.GetConfig().BasketshopStockFilename);

            var market_info = new Parsing.Model.MarketInfo();
            market_info.name = NAME;
            market_info.website = SITEURL;
            market_info.currency = CURRENCY;
            market_info.start_parse_date = DateTime.Now;
            market_info.end_parse_date = DateTime.Now;
            market_info.delivery_to_usa = DELIVERY_TO_USA;
            market_info.photo_parameters.is_watermark_image = false;
            market_info.photo_parameters.background_color = "white";
            market_info.photo_parameters.first_photo_sneaker_direction = "left"; //уточнить
            market_info.currently_language = "en";
            var json = CreateJson(market_info);
            SaveJson(json, JSON_FILENAME, FOLDER, NAME);
        }

        /// <summary>
        /// Обновляем информацию о каталоге и стоке 
        /// </summary>
        /// <returns>возвращает true если были обновления</returns>
        public bool UpdateCatalogAndStock()
        {        
            //читаем старый каталог и сток
            ReadCatalogFromCSV(Config.GetConfig().BasketshopCatalogFilename);
            stock.ReadStockFromCSV(Config.GetConfig().BasketshopStockFilename);
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
                    //DownloadSneakerImages(newSneaker, Settings.BASKETSHOP_IMAGE_FOLDER);
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

            //бренд - nike
            this.brand = "Nike";
            ParseSneakersFromPage(catalog, "http://www.basketshop.ru/catalog/shoes/nike/?np=30");
            this.brand = "Nike";
            ParseSneakersFromPage(catalog, "http://www.basketshop.ru/catalog/shoes/nikesb/?np=30");
            this.brand = "Nike";
            ParseSneakersFromPage(catalog, "http://www.basketshop.ru/catalog/shoes/nike-sportswear/?np=30");
            this.brand = "Jordan";
            ParseSneakersFromPage(catalog, "http://www.basketshop.ru/catalog/shoes/jordan/?np=30");
            

            //SaveLinksToCSV(JSON_FILENAME);

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string link)
        {
            Console.WriteLine("Парсим " + link);
            Uri uri = new Uri(link);
            string source = new WebClient().DownloadString(uri);
            var parser = new HtmlParser();
            var document = parser.Parse(source);
            var items = document.QuerySelectorAll("div.item");
            foreach (var item in items)
            {
                var sneaker = new Sneaker();

                sneaker.sku = item.QuerySelector("div.art").InnerHtml;
                var priceDiv = item.QuerySelector("div.price");
                string priceStr;
                if (priceDiv.QuerySelector("span.ssale") != null) {
                    priceStr = priceDiv.QuerySelector("span.ssale").InnerHtml;
                    sneaker.price = Int32.Parse(priceStr.Replace("р.", ""));
                    sneaker.oldPrice = Int32.Parse(priceDiv.QuerySelector("s").InnerHtml);
                }
                else {
                    priceStr = priceDiv.InnerHtml;
                    sneaker.price = Int32.Parse(priceStr.Replace("р.", ""));
                }
                
                //из заголовка еще надо вытащить type и убрать nike
                sneaker.title = item.QuerySelector("span.name").InnerHtml;
                sneaker.ParseTitle();
                sneaker.link = SITEURL + item.QuerySelector("a").GetAttribute("href");
                sneaker.brand = this.brand;

                //sizes
                string[] sizesStrArr = item.QuerySelector("div.item_sizes").InnerHtml.Split(',');
                foreach (var size in sizesStrArr)
                {

                    SneakerSize sneakerSize = new SneakerSize(sneaker, size.Trim());
                    sneaker.sizes.Add(sneakerSize);
                }

                //catalog.sneakers.Add(stockSneaker);
                catalog.AddUniqueSneaker(sneaker);
            }

            //next page
            var nextlinks = document.QuerySelector("div.pages").QuerySelectorAll("a.arrow");
            foreach (var nextlink in nextlinks)
            {
                if (nextlink.ClassName == "arrow r")
                {
                    string nextPageLink = nextlink.GetAttribute("href");
                    Thread.Sleep(100);
                    ParseSneakersFromPage(catalog, SITEURL + nextPageLink);
                }
            }
        }

        /// <summary>
        /// парсим каждый кроссовок. на выходе получаем заполненный объект sneakers со всеми данными (размеры, изображения и т.д.)
        /// </summary>
        public void ParseAllSneakers()
        {
            int count = catalog.sneakers.Count;
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(i + " " + catalog.sneakers[i].link);
                ParseOneSneaker(i);
                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                Thread.Sleep(100);
                //sneakerParser.DownloadSneakerImages(i);

            }
        }

        //public Sneaker ParseOneSneaker(int index) { }

        public Sneaker ParseOneSneaker(int index)
        {
            Sneaker sneaker = @catalog.sneakers[index];
            //string url = "http://street-beat.ru/d/krossovki-nizkie-air-force-1-low-retro-845053-101/";
            string url = sneaker.link;
            Uri uri = new Uri(url);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.GetEncoding(1251);
            string source = webClient.DownloadString(uri);

            var parser = new HtmlParser();
            var document = parser.Parse(source);

            //images
            string imageString = SITEURL + document.QuerySelector("div.zoom").QuerySelector("img").GetAttribute("src");
            sneaker.images.Add(imageString);
            var imagesHTML = document.QuerySelector("div.bottom_bar").QuerySelectorAll("img");
            //List<String> listImage = new List<String>();
            foreach (var image in imagesHTML)
            {
                imageString = SITEURL + image.GetAttribute("src").Replace("bm.JPG", ".JPG").Replace("bm.jpg", ".jpg");
                sneaker.images.Add(imageString);
            }

            //color and sex
            var colorHTML = document.QuerySelector("table.ii").QuerySelectorAll("tr");
            foreach (var tr in colorHTML)
            {
                var td = tr.QuerySelectorAll("td").ToArray();
                //color
                if (td[0].InnerHtml.Contains("цвет"))
                {
                    sneaker.color = td[1].InnerHtml.Trim();
                    TextInfo ti = new CultureInfo("ru-ru", false).TextInfo;
                    string color = sneaker.color.Replace(".", "").Replace(", ", "|").Replace("ё", "е").Trim();
                    color = ti.ToTitleCase(color);
                    sneaker.color = color;
                }

                //sex
                if (td[0].InnerHtml.Contains("пол:"))
                {
                    switch (td[1].InnerHtml.Trim())
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
                        default:
                            Console.WriteLine("Категория не определена. sku: " + sneaker.sku);
                            Program.Logger.Warn("Категория не определена. sku: " + sneaker.sku);
                            //throw new Exception("Категория не определена.");
                            break;
                    }
                }
            }
            return sneaker;
        }
    }
}
