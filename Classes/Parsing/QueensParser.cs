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
    class QueensParser : Parser
    {
        public const string SITEURL = "http://magazinqueens.ru";
        public static readonly string FOLDER = DIRECTORY_PATH + @"queens\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "ru";
        public const string CURRENCY = "RUB";
        public const string NAME = "magazinqueens.ru";
        public const int MARZHA = DELIVERY_TO_USA + 1000;
        public const int DELIVERY_TO_USA = 2000;
        public const double VAT_VALUE = 0; //Если 15% ват, то пишем 0.15, если вычета нет, то 0

        public void Run()
        {
            catalog = ParseSneakersFromAllPages(); //парсим все ссылки и общую инфу
            ParseAllSneakers(catalog); //парсим все кроссы по одному
            catalog.RemoveDuplicate();
            catalog.RemoveDuplicateSizesFromSneakers();

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
            catalog.RemoveDuplicate();
            catalog.RemoveDuplicateSizesFromSneakers();
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
            //ParseAllSneakers(newCatalog);

            int i = 0;
            foreach (var newSneaker in newCatalog.sneakers)
            {
                //Console.WriteLine(i + " " + catalog.sneakers[i].queensLink);
                //ParseOneSneaker(newSneaker);
                //Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                i++;

                Sneaker oldSneaker = catalog.GetSneakerFromLink(newSneaker.link);
                if (oldSneaker == null)
                {
                    catalog.sneakers.Add(newSneaker);
                    ParseOneSneaker(catalog.sneakers[catalog.sneakers.Count - 1]);
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
            Catalog newCatalog2 = new Catalog();
            foreach (var sneaker in catalog.sneakers)
            {
                sneaker.ParseTitle();
                sneaker.title = sneaker.title.Replace("Jordan AIR JORDAN", "AIR JORDAN");
                sneaker.title = sneaker.title.Replace("Jordan JORDAN", "AIR JORDAN");

                //if (stockSneaker.sku == "")
                Sneaker newSneaker = newCatalog.GetSneakerFromLink(sneaker.link);
                if (newSneaker == null)
                {
                    isUpdate = true;
                    string message = "Sneaker deleted. sku:" + sneaker.sku;
                    Program.Logger.Info(message);
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


            ParseSneakersFromPage(catalog, "https://magazinqueens.ru/catalog/obuv/?msoption|gender=%D0%9C%D1%83%D0%B6%D1%81%D0%BA%D0%BE%D0%B5,%D0%A3%D0%BD%D0%B8%D1%81%D0%B5%D0%BA%D1%81&ms|vendor=12,2,3&limit=360", "Мужской", "Мужская");

            ParseSneakersFromPage(catalog, "https://magazinqueens.ru/catalog/obuv/?msoption|gender=%D0%96%D0%B5%D0%BD%D1%81%D0%BA%D0%BE%D0%B5&ms|vendor=12,2,3&limit=360", "Женский", "Женская");

            ParseSneakersFromPage(catalog, "https://magazinqueens.ru/catalog/obuv/?msoption|gender=%D0%94%D0%B5%D1%82%D1%81%D0%BA%D0%BE%D0%B5,%D0%9F%D0%BE%D0%B4%D1%80%D0%BE%D1%81%D1%82%D0%BA%D0%BE%D0%B2%D0%BE%D0%B5&ms|vendor=12,2,3&limit=360", null, "Детская");


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
            var items = document.QuerySelectorAll("div.ms2_product");
            foreach (var item in items)
            {
                var sneaker = new Sneaker();
                sneaker.sex = sex;
                sneaker.category = category;
                sneaker.link = SITEURL +"/" + item.QuerySelector("a").GetAttribute("href");

                //title
                sneaker.title = item.QuerySelector("span.item-txt").InnerHtml;
                sneaker.ParseTitle();
                //stockSneaker.title = stockSneaker.brand + " " + stockSneaker.title.Replace(stockSneaker.brand, "").Trim();
                sneaker.title = sneaker.title.Replace("Jordan AIR JORDAN", "AIR JORDAN");
                sneaker.title = sneaker.title.Replace("Jordan JORDAN", "AIR JORDAN");

                //price
                string priceString = item.QuerySelector("div.item-price").InnerHtml.Replace("<!--h5>RUB</h5-->","").Replace("RUB","").Trim().Replace(" ","");
                sneaker.price = Double.Parse(priceString);

                //sizes
                string sizeString = item.QuerySelector("div.size-box").InnerHtml.Replace("US","");
                sizeString = sizeString.Replace("UK", ""); //есть один артикул у которого вместо US UK
                string[] stringSeparators = new string[] { "\n" };
                string[] sizeArr = sizeString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var size in sizeArr)
                {
                    if (!String.IsNullOrWhiteSpace(size))
                    {
                        SneakerSize snSize = new SneakerSize(size.Trim());
                        sneaker.sizes.Add(snSize);
                    }
                }
                sneaker.DeleteDuplicateSizes();
                catalog.AddUniqueSneaker(sneaker);
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

            sneaker.brand = document.QuerySelector("h1").InnerHtml;
            if (sneaker.brand.ToUpper().Contains("NIKE SB"))
            {
                bool test = true;
                sneaker.brand = sneaker.brand.Replace("Nike SB","Nike");
            }

            //stockSneaker.title = document.QuerySelector("div.good-subtitle").InnerHtml;
            //stockSneaker.ParseTitle();
            //stockSneaker.title = stockSneaker.brand + " " + stockSneaker.title.Replace(stockSneaker.brand, "").Trim();
            //stockSneaker.title = stockSneaker.title.Replace("Jordan AIR JORDAN", "AIR JORDAN");
            //stockSneaker.title = stockSneaker.title.Replace("Jordan JORDAN", "AIR JORDAN");
            

            sneaker.sku = document.QuerySelector("h5.good-articul").InnerHtml.Replace("Артикул: ", "");
            //stockSneaker.sku = artikul[0] + '-' + artikul[1];

            //description
            sneaker.description = document.QuerySelector("div.good-txt").InnerHtml.Trim();

            //price
            //source = document.QuerySelector("span.product__price").InnerHtml;
            //var document2 = parser.Parse(source);
            //int price = Int32.Parse(document2.QuerySelector("span.product__price__num").InnerHtml);
            //stockSneaker.price = price;

            //old price
            //var oldPriceHTML = document.QuerySelector("span.product__price-old");
            //if (oldPriceHTML != null)
            //{
            //    stockSneaker.oldPrice = Int32.Parse(oldPriceHTML.QuerySelector("span.product__price__num").InnerHtml.Trim().Replace("&nbsp;", ""));
            //}

            //images
            var imageHTMLContainer = document.QuerySelector("div.good-thumbs-cnt");
            if (imageHTMLContainer != null)
            {
                var imageHTML = imageHTMLContainer.QuerySelectorAll("a");
                foreach (var image in imageHTML)
                {
                    sneaker.images.Add(SITEURL + image.GetAttribute("data-image"));
                }
            }

            //List<String> listImage = new List<String>();


            //sizes
            //source = document.QuerySelector("div.product__choice_size").InnerHtml;
            //document2 = parser.Parse(source);
            //var sizes2 = document2.QuerySelectorAll("li.sizeUS-list_item").ToArray();

            //foreach (var sizeitem in sizes2)
            //{
            //    Boolean isEnd = false;
            //    foreach (var className in sizeitem.ClassList)
            //    {
            //        if (className == "list__item_disable")
            //        {
            //            isEnd = true;
            //        }
            //    }
            //    if (!isEnd)
            //    {
            //        SneakerSize sizeUS = new SneakerSize();
            //        sizeUS.sizeUS = sizeitem.GetAttribute("data-article").Split('-')[2];
            //        SneakerSizeStock stock = new SneakerSizeStock();
            //        stock.stockName = "StreetBeat";
            //        stock.quantity = 1;
            //        stock.price = price;
            //        sizeUS.stock.Add(stock);
            //        stockSneaker.sizes.Add(sizeUS);
            //    }
            //}

            return sneaker;
        }

        public void ParseAllSneakers(Catalog @catalog)
        {
            //string filenameStock = @"C:\SneakerIcon\CSV\StreetBeat\StockStreetBeat.csv";

            int count = catalog.sneakers.Count;
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(i + " " + catalog.sneakers[i].link);
                ParseOneSneaker(catalog.sneakers[i]);
                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                //sneakerParser.DownloadSneakerImages(i);
            }
            catalog.RemoveDuplicate();
        }
    }
}
