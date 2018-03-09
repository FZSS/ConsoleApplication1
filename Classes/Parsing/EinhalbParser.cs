using AngleSharp.Parser.Html;
using CsvHelper;
using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Catalogs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing
{
    public class EinhalbParser : Parser
    {
        public const string SITEURL = "http://www.43einhalb.com";
        public static readonly string FOLDER = DIRECTORY_PATH + @"43einhalb.com\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        public const string CURRENCY = "EUR";
        public const string NAME = "43einhalb.com";
        public const int MARZHA = DELIVERY_TO_USA + 25;
        public const int DELIVERY_TO_USA = 33;
        public const double VAT_VALUE = 0.16; //Если 15% ват, то пишем 0.15, если вычета нет, то 0

        public void Run()
        {
            catalog = ParseSneakersFromAllPages(); //парсим все ссылки и общую инфу
            ParseAllSneakers(catalog); //парсим все кроссы по одному
            SetSellPrices();
            //UpdateCatalogAndStock();

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

            //ReadCatalogFromCSV(CATALOG_FILENAME);
            
            
            
            //DownloadAllSneakersImages(IMAGE_FOLDER);
            
            
            
            //DownloadAllSneakersImages(DIRECTORY_PATH + @"BigImages\");

           
        }

        public void Update()
        {
            UpdateCatalogAndStock();
            SetSellPrices();
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

            foreach (var newSneaker in newCatalog.sneakers)
            {
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

            //парсим список мужских кроссовок
            ParseSneakersFromPage(catalog, "http://www.43einhalb.com/en/sneaker/filter/__gender_kids__brand_nike/page/1/sort/relevance/perpage/72", null, "Детская");
            ParseSneakersFromPage(catalog, "http://www.43einhalb.com/en/sneaker/filter/__gender_boys__brand_nike/page/1/sort/relevance/perpage/72", "Мужской", "Мужская");
            ParseSneakersFromPage(catalog, "http://www.43einhalb.com/en/sneaker/filter/__gender_girls__brand_nike/page/1/sort/relevance/perpage/72", "Женский", "Женская");
            
            //cохрняем результат в csv файл
            //SaveLinksToCSV(JSON_FILENAME);

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string link, string sex, string category)
        {
            Console.WriteLine("Парсим " + link);
            Uri uri = new Uri(link);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            string source = string.Empty;
            //string source = webClient.DownloadString(uri);
            bool isDownload = false;
            int numberTry = 0;
            while (!isDownload || numberTry > 10)
                try
                {
                    source = webClient.DownloadString(uri);
                    isDownload = true;
                }
                catch (WebException e)
                {
                    Program.Logger.Error("WebException" + e.StackTrace);
                    Thread.Sleep(5000);
                    numberTry++;
                    //return null;
                }
            if (!isDownload) throw new Exception("Ошибка загрузки страницы 43 einhalb");

            webClient.Dispose();
            var parser = new HtmlParser();
            var document = parser.Parse(source);
            var items = document.QuerySelectorAll("li.item");
            foreach (var item in items)
            {
                var sneaker = new Sneaker();
                sneaker.sex = sex;
                sneaker.category = category;
                sneaker.link = "http://www.43einhalb.com" + item.QuerySelector("div.pImages").GetAttribute("data-href");

                //prices
                var document3 = item.QuerySelector("span.pPrice");
                string priceString = String.Empty;
                if (document3.QuerySelector("span.newPrice") != null) //значит товар идет по сейлу
                {
                    priceString = document3.QuerySelector("span.newPrice").InnerHtml.Replace("€", "").Replace('.', ',');
                    sneaker.price = double.Parse( priceString );
                    priceString = document3.QuerySelector("span.oldPrice").QuerySelector("span.priceWithCurrency").InnerHtml.Replace("€", "").Replace('.', ',');
                    sneaker.oldPrice = double.Parse(priceString);
                }
                else 
                {
                    priceString = document3.InnerHtml.Trim().Replace("€", "").Replace('.', ',');
                    priceString = priceString.Replace("<span>from</span>", "").Trim();
                    sneaker.price = double.Parse(priceString);
                }

                //sizes
                var sizesItem = item.QuerySelector("ul.availableVariants");
                var sizesHTML = sizesItem.QuerySelectorAll("li").ToList();
                var sizesHTML_Soldout = sizesItem.QuerySelectorAll("li.soldout").ToList();
                var sizesListInStock = sizesHTML.Except(sizesHTML_Soldout).ToList();
                foreach (var size in sizesListInStock)
                {
                    string sizeUS = size.QuerySelector("a").GetAttribute("US");
                    if (sizeUS == null) //это значит что размер S M L XL и т.п.
                    {
                        sizeUS = size.QuerySelector("a").GetAttribute("DEFAULT");
                    }
                    sneaker.sizes.Add(new SneakerSize(sizeUS));
                }
                
                if (!catalog.isExistSneakerInCatalog(sneaker))
                    catalog.sneakers.Add(sneaker);
            }
            var nextPage = document.QuerySelector("li.next");

            if (nextPage != null) //если только одна страницы то будет null
            {
                if (!nextPage.ClassName.Contains("disabled")) //если последняя страница то в классе будет disabled
                {
                    string nextPageLink = nextPage.QuerySelector("a").GetAttribute("href");
                    ParseSneakersFromPage(catalog, SITEURL + nextPageLink, sex, category);
                }
            }

        }

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            //Sneaker stockSneaker = @catalog.sneakers[index];

            //string url = "http://street-beat.ru/d/krossovki-nizkie-air-force-1-low-retro-845053-101/";
            //string url = "http://www.43einhalb.com/en/nike-air-force-1-low-retro-white-green-100607";
            //string url = sneakerParser.SneakersLinks[0];
            string url = sneaker.link;
            Uri uri = new Uri(url);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            string source = string.Empty;
            //string source = webClient.DownloadString(uri);
            bool isDownload = false;
            int numberTry = 0;
            while (!isDownload || numberTry > 10)
                try
                {
                    source = webClient.DownloadString(uri);
                    isDownload = true;
                }
                catch (WebException e)
                {
                    Program.Logger.Error("WebException" + e.StackTrace);
                    Thread.Sleep(5000);
                    numberTry++;
                    //return null;
                }
            if (!isDownload) throw new Exception("Ошибка загрузки страницы 43 einhalb");

            // Create a new parser front-end (can be re-used)
            var parser = new HtmlParser();
            //Just get the DOM representation
            var document = parser.Parse(source);

            sneaker.brand = "Nike";



            //title
            //stockSneaker.title = document.QuerySelector("h1.product__title").InnerHtml;
            var source2 = document.QuerySelector("div.five").InnerHtml;
            var document2 = parser.Parse(source2);
            var title = document2.QuerySelector("h4").InnerHtml;

            //бренд принудительно присваивается nike,надо проверить, может есть джорданы
            if (title.ToLower().Contains("jordan"))
            {
                bool test5 = true;
            }

            sneaker.title = Parser.CorrectTitle(title,sneaker.brand);
            
            sneaker.ParseTitle();
            
            
            //sku
            //string[] artikul = document.QuerySelector("span.product__articul__num").InnerHtml.Split('-');
            //stockSneaker.sku = artikul[0] + '-' + artikul[1];
            sneaker.sku = document.QuerySelector("li.suppliernumber").QuerySelector("span").InnerHtml;
            sneaker.sku = sneaker.sku.Trim().Replace(" ", "-"); //есть один артикул у которого пробел вместо -

            if (sneaker.sku == "875797-700")
            {
                bool test = true;
            }

            if (sneaker.sku == "881430-029")
                sneaker.category = "Детская"; //вручную меняем неверную категорию

            //description
            string sourceDescription = document.QuerySelector("li.active").InnerHtml;
            var documentDescription = parser.Parse(sourceDescription);
            try
            {
                //stockSneaker.description = documentDescription.QuerySelectorAll("p").ToArray()[0].InnerHtml;
            }
            catch (IndexOutOfRangeException e)
            {
                sneaker.description = string.Empty;
                //нет описания
            }
            //stockSneaker.description = stockSneaker.description.Replace("\n", "");

            ////price
            //source = document.QuerySelector("div.pPrice").InnerHtml;
            //document2 = parser.Parse(source);
            //var priceHTML = document2.QuerySelector("span.price");
            //string priceString = String.Empty;
            //if (priceHTML != null)
            //{
            //    priceString = priceHTML.GetAttribute("content");
            //}
            //else //значит товар идет по сейлу
            //{
            //    priceHTML = document2.QuerySelector("span.newPrice");
            //    priceString = priceHTML.GetAttribute("content");
            //    stockSneaker.oldPrice = double.Parse(document2.QuerySelector("span.oldPrice").InnerHtml.Replace("&nbsp;*","").Replace("€","").Replace('.', ','));
            //}
            
            ////оригинальная цена в евро
            //stockSneaker.price = double.Parse(priceString.Replace('.', ','));
            
            
            //цена на сайте указана в евро с учетом налога VAT, он составляет 17,532%. Вычитаем этот налог и умножаем на стоимость евро
            // получается так, что доставка в европу примерно 10 евро, но налог не вычитается. А если доставка в россию, канаду, европу
            //Для рублевой цены прибавляем еще 33.14 евро за доставку до России
            //double priceRUB = ((stockSneaker.price * 0.82468) + 33.11) * 74; //преобразую цену в евро в рубли
            //int price = (int)Math.Ceiling(priceRUB);
            //stockSneaker.price = price;

            //images
            source = document.QuerySelector("ul.slides").InnerHtml;
            document2 = parser.Parse(source);
            var images = document2.QuerySelectorAll("a.galleriaTrigger");
            //List<String> listImage = new List<String>();
            foreach (var image in images)
            {
                string imageString = image.QuerySelector("img").GetAttribute("src");
                if (imageString.Contains(".jpg"))
                    sneaker.images.Add(imageString);
            }

            ////sizes
            //source = document.QuerySelector("div.selectVariants").InnerHtml;
            //document2 = parser.Parse(source);
            //var sizes2 = document2.QuerySelectorAll("option").ToArray();

            //foreach (var sizeitem in sizes2)
            //{
            //    Boolean isActive = true;
            //    if (sizeitem.InnerHtml.Contains("Please select"))
            //    {
            //        isActive = false;
            //    }
            //    else
            //    {
            //        if (sizeitem.ClassName.Contains("disabled")) isActive = false;
            //    }
                
            //    if (isActive)
            //    {
            //        string sizeString = sizeitem.InnerHtml.Trim();
            //        string sizeUS = sizeString.Substring(sizeString.IndexOf('·')+1).Replace("US","").Trim();

            //        SneakerSize sizeUS = new SneakerSize();
            //        sizeUS.sizeUS = sizeUS.Replace(',','.');
            //        SneakerSizeStock stock = new SneakerSizeStock();
            //        stock.stockName = "43einhalb.com";
            //        stock.quantity = 1;
            //        stock.price = stockSneaker.price;
            //        sizeUS.stock.Add(stock);
            //        stockSneaker.sizes.Add(sizeUS);
            //    }
            //} //sizes

            //color
            var htmlDoc = document.QuerySelector("table.productAttributesTable");
            var attributesName = htmlDoc.QuerySelectorAll("td.productAttributesName");
            var attributesValue = htmlDoc.QuerySelectorAll("td.productAttributesValue");
            for (int i = 0; i < attributesName.Count(); i++)
            {
                if (attributesName[i].InnerHtml == "Color-Producer:")
                    sneaker.color = attributesValue[i].InnerHtml.Trim();
                if (attributesName[i].InnerHtml == "Style:")
                    ParseStyle(attributesValue[i].InnerHtml.Trim(),sneaker);
                    //stockSneaker.destination = attributesValue[i].InnerHtml.Trim(); //с этим полем надо попозже поработать. тут есть высота кросс, назначение и другие параметры
            }

            return sneaker;
        }

        private void ParseStyle(string style, Sneaker @sneaker) {
            string[] styleArr = style.Split(',');
            foreach (var param in styleArr) {
                if (param.Contains("Low"))
                    sneaker.height = "Низкие";
                if (param.Contains("Mid"))
                    sneaker.height = "Средние";
                if (param.Contains("High"))
                    sneaker.height = "Высокие";
                if (param.Contains("Basketball"))
                    sneaker.AddDestination("Баскетбольные");
                if (param.Contains("Casual"))
                    sneaker.AddDestination("Повседневные");
                if (param.Contains("Running"))
                    sneaker.AddDestination("Беговые");
                if (param.Contains("Skateboarding"))
                    sneaker.AddDestination("Скейтбординг");
                if (param.Contains("Tennis"))
                    sneaker.AddDestination("Теннисные");
            }
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

        }

        public void SetSellPrices()
        {
            foreach (var sneaker in catalog.sneakers)
            {
                int marzha = 25; 
                if (sneaker.price > 135) //тогда 10 евро прибавляем
                {
                    sneaker.sellPrice = ((sneaker.price + 10)) + marzha; //10 - доставка по европе
                    
                }

                else // 0.83 + 33евро
                {
                    sneaker.sellPrice = (((sneaker.price * (1 - 0.17)) + 33)) + marzha; //0.17 вычет налога ват, 33 доставка по миру
                }
            }
        }
    }
}
