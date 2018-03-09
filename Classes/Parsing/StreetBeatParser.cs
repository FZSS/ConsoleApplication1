using AngleSharp.Parser.Html;
using SneakerIcon.Classes.Catalogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing
{
    class StreetBeatParser : Parser
    {
        public const string SITEURL = "http://street-beat.ru/";
        public static readonly string FOLDER = DIRECTORY_PATH + @"streetbeat\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "ru";
        public const string CURRENCY = "RUB";
        public const string NAME = "street-beat.ru";
        public const int MARZHA = DELIVERY_TO_USA + 3000;
        public const int DELIVERY_TO_USA = 2000;
        public const double VAT_VALUE = 0;

        public void Run()
        {
            catalog = ParseSneakersFromAllPages(); //парсим все ссылки и общую инфу
            ParseAllSneakers(catalog); //парсим все кроссы по одному

            //UpdateCatalogAndStock();

            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);

            //ReadCatalogFromCSV(CATALOG_FILENAME);
            //DownloadAllSneakersImages(IMAGE_FOLDER);

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
            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);

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

        public bool UpdateCatalogAndStock()
        {
            //читаем старый каталог и сток
            ReadCatalogFromCSV(CATALOG_FILENAME);
            stock.ReadStockFromCSV(STOCK_FILENAME);
            MergeCatalogAndStock();

            bool isUpdate = false;

            //парсим новый список кроссовок и размеров
            Catalog newCatalog = ParseSneakersFromAllPages();
            ParseAllSneakers(newCatalog);
            //ParseAllSneakers(newCatalog);

            for (int i = 0; i < newCatalog.sneakers.Count; i++)
            {
                var newSneaker = newCatalog.sneakers[i];
                //Console.WriteLine(i + " " + catalog.sneakers[i].queensLink);
                //newSneaker = ParseOneSneaker(newSneaker);

                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);

                Sneaker oldSneaker = catalog.GetSneakerFromLink(newSneaker.link);
                if (oldSneaker == null)
                {
                    catalog.sneakers.Add(newSneaker);
                    //ParseOneSneaker(catalog.sneakers[catalog.sneakers.Count - 1]);
                    //DownloadSneakerImages(newSneaker, IMAGE_FOLDER);
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
                            //Console.WriteLine("new sizeUS. sku:" + newSneaker.sku + " sizeUS:" + sizeUS.sizeUS);
                        }
                    }
                    //теперь проверяем старые размеры. Если какого-то из этих размеров нет в новых размерах, значит его продали и его надо убрать
                    foreach (var size in oldSneaker.sizes)
                    {
                        SneakerSize newSize = newSneaker.GetSize(size.sizeUS);
                        if (newSize == null)
                        {
                            isSizeUpdated = true;
                            //Console.WriteLine("sizeUS soldout. sku:" + newSneaker.sku + " sizeUS:" + sizeUS.sizeUS);
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
            Catalog newCatalog2 = new Catalog();
            foreach (var sneaker in catalog.sneakers)
            {
                sneaker.title = sneaker.title.Replace("низкая", "низкие");
                sneaker.ParseTitle();

                if (sneaker.title.Contains("Nike AIR JORDAN"))
                {
                    sneaker.title = sneaker.title.Replace("Nike AIR JORDAN", "AIR JORDAN");
                    sneaker.brand = "Jordan";
                }
                
                //if (stockSneaker.sku == "")
                Sneaker newSneaker = newCatalog.GetSneakerFromLink(sneaker.link);
                if (newSneaker == null)
                {
                    isUpdate = true;
                    string message = "Sneaker deleted. sku:" + sneaker.sku;
                    //Program.logger.Info(message);
                    //Console.WriteLine(message);
                }
                else
                {
                    if (!newCatalog2.isExistSneakerInCatalog(sneaker))
                        newCatalog2.sneakers.Add(sneaker);
                }
            }

            catalog = newCatalog2;

            //SaveCatalogToCSV(CATALOG_FILENAME);
            //SaveStockToCSV(STOCK_FILENAME);

            return isUpdate;
        }

        public Catalog ParseSneakersFromAllPages()
        {
            Catalog catalog = new Catalog();       

            ParseSneakersFromPage(catalog, "http://street-beat.ru/cat/man/krossovki;kedy;sandalii/nike/?page_size=all", "Мужской", "Мужская");
            ParseSneakersFromPage(catalog, "http://street-beat.ru/cat/woman/krossovki;sandalii/nike/?page_size=all", "Женский", "Женская");

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string link, string sex, string category)
        {
            Console.WriteLine("Парсим " + link);
            Uri uri = new Uri(link);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            string source = webClient.DownloadString(uri);
            webClient.Dispose();
            var parser = new HtmlParser();
            var document = parser.Parse(source);
            var items = document.QuerySelectorAll("a.product-item__title_a");
            foreach (var item in items)
            {
                var sneaker = new Sneaker();
                sneaker.sex = sex;
                sneaker.category = category;
                sneaker.link = item.GetAttribute("href");

                if (!catalog.isExistSneakerInCatalog(sneaker))
                    catalog.sneakers.Add(sneaker);
            }
        }

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            string url = sneaker.link;
            Uri uri = new Uri(url);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            string source = webClient.DownloadString(uri);

            // Create a new parser front-end (can be re-used)
            var parser = new HtmlParser();
            //Just get the DOM representation
            var document = parser.Parse(source);

            sneaker.brand = "Nike";

            sneaker.title = document.QuerySelector("h1.product__title").InnerHtml;
            sneaker.ParseTitle();
            sneaker.title = sneaker.brand + " " + sneaker.title.Replace(sneaker.brand, "").Trim();
            string[] artikul = document.QuerySelector("span.product__articul__num").InnerHtml.Split('-');                  
            if (artikul.Count() < 2) return null;
            sneaker.sku = artikul[0] + '-' + artikul[1];

            //description
            string sourceDescription = document.QuerySelector("div.product__descr__right").InnerHtml;
            var documentDescription = parser.Parse(sourceDescription);
            try
            {
                sneaker.description = documentDescription.QuerySelectorAll("p").ToArray()[1].InnerHtml;
            }
            catch (IndexOutOfRangeException e)
            {
                sneaker.description = string.Empty;
                //нет описания
            }

            //price
            source = document.QuerySelector("span.product__price").InnerHtml;
            var document2 = parser.Parse(source);           
            var priceString = document2.QuerySelector("span.product__price__num").InnerHtml;
            priceString = priceString.Replace("<!--'start_frame_cache_price'-->", "").Replace("<!--'end_frame_cache_price'-->", "");
            int price = Int32.Parse(priceString);
            sneaker.price = price;

            //old price
            var oldPriceHTML = document.QuerySelector("span.product__price-old");
            if (oldPriceHTML != null)
            {
                var oldPriceString = oldPriceHTML.QuerySelector("span.product__price__num").InnerHtml.Trim().Replace("&nbsp;", "");
                oldPriceString = oldPriceString.Replace("<!--'start_frame_cache_old_price'-->", "").Replace("<!--'end_frame_cache_old_price'-->", "");
                sneaker.oldPrice = Int32.Parse(oldPriceString);
            }

            //images
            source = document.QuerySelector("div.gallery-preview").InnerHtml;
            document2 = parser.Parse(source);
            var images = document2.QuerySelectorAll("a");
            //List<String> listImage = new List<String>();
            foreach (var image in images)
            {
                string imageStr = image.GetAttribute("href");
                if (imageStr.IndexOf("?") > 0)
                {
                    imageStr = imageStr.Substring(0, imageStr.IndexOf("?"));
                }
                if (!imageStr.Contains("http://") && !imageStr.Contains("https://"))
                {
                    imageStr = "http://street-beat.ru" + imageStr;
                }
                sneaker.images.Add(imageStr);
            }

            //sizes
            source = document.QuerySelector("div.product__choice_size").InnerHtml;
            document2 = parser.Parse(source);
            var sizes2 = document2.QuerySelectorAll("li.size-list_item").ToArray();

            foreach (var sizeitem in sizes2)
            {
                Boolean isEnd = false;
                foreach (var className in sizeitem.ClassList)
                {
                    if (className == "list__item_disable")
                    {
                        isEnd = true;
                    }
                }
                if (!isEnd)
                {
                    SneakerSize size = new SneakerSize();
                    size.sizeUS = sizeitem.GetAttribute("data-article").Split('-')[2];
                    SneakerSizeStock stock = new SneakerSizeStock();
                    stock.stockName = "StreetBeat";
                    stock.quantity = 1;
                    stock.price = price;
                    size.stock.Add(stock);
                    sneaker.sizes.Add(size);
                }
            }

            return sneaker;
        }

        public void ParseAllSneakers(Catalog @catalog)
        {
            //string filenameStock = @"C:\SneakerIcon\CSV\StreetBeat\StockStreetBeat.csv";
            var newCatalog = new Catalog();
            int count = catalog.sneakers.Count;
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(i + " " + catalog.sneakers[i].link);
                var sneaker = ParseOneSneaker(catalog.sneakers[i]);
                if (sneaker != null) newCatalog.AddUniqueSneaker(sneaker);
                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                //sneakerParser.DownloadSneakerImages(i);
            }
            catalog.sneakers = newCatalog.sneakers;
        }

        public Catalog ParseAllSneakers2(Catalog catalog)
        {
            //string filenameStock = @"C:\SneakerIcon\CSV\StreetBeat\StockStreetBeat.csv";
            var newCatalog = new Catalog();
            int count = catalog.sneakers.Count;
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(i + " " + catalog.sneakers[i].link);
                var sneaker = ParseOneSneaker(catalog.sneakers[i]);
                if (sneaker != null) newCatalog.AddUniqueSneaker(sneaker);
                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                //sneakerParser.DownloadSneakerImages(i);
            }
            return newCatalog;
        }

    }
}
