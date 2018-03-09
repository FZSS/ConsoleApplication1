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
    public class OverkillshopParser : Parser
    {
        public const string SITEURL = "https://www.overkillshop.com";
        public static readonly string FOLDER = DIRECTORY_PATH + @"overkillshop\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        public const string CURRENCY = "EUR";
        public const string NAME = "overkillshop.com";
        public const int MARZHA = DELIVERY_TO_USA + 25;
        public const int DELIVERY_TO_USA = 30;
        public const double VAT_VALUE = 0.16; //Если 15% ват, то пишем 0.15, если вычета нет, то 0

        public void Run()
        {
            catalog = ParseSneakersFromAllPages(); //парсим все ссылки и общую инфу
            ParseAllSneakers(catalog); //парсим все кроссы по одному
            //SetSellPrices();
            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);

            var market_info = new Parsing.Model.MarketInfo();
            market_info.name = NAME;
            market_info.website = SITEURL;
            market_info.currency = CURRENCY;
            market_info.start_parse_date = DateTime.Now;
            market_info.end_parse_date = DateTime.Now;
            market_info.delivery_to_usa = DELIVERY_TO_USA;
            market_info.photo_parameters.is_watermark_image = true;
            market_info.photo_parameters.background_color = "white";
            market_info.photo_parameters.first_photo_sneaker_direction = "left"; //уточнить
            market_info.currently_language = "en";
            var json = CreateJson(market_info);
            SaveJson(json, JSON_FILENAME, FOLDER, NAME);
        }

        //public void Update()
        //{
        //    UpdateCatalogAndStock();
        //    SetSellPrices();
        //    SaveCatalogToCSV(CATALOG_FILENAME);
        //    SaveStockToCSV(STOCK_FILENAME);
        //}

        //public bool UpdateCatalogAndStock()
        //{
        //    //читаем старый каталог и сток
        //    ReadCatalogFromCSV(CATALOG_FILENAME);
        //    stock.ReadStockFromCSV(STOCK_FILENAME);
        //    MergeCatalogAndStock();

        //    bool isUpdate = false;

        //    //парсим новый список кроссовок и размеров
        //    Catalog newCatalog = ParseSneakersFromAllPages();
        //    //ParseAllSneakers(newCatalog);

        //    int i = 0;
        //    foreach (var newSneaker in newCatalog.sneakers)
        //    {
        //        //Console.WriteLine(i + " " + catalog.sneakers[i].queensLink);
        //        ParseOneSneaker(newSneaker);
        //        Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
        //        i++;

        //        Sneaker oldSneaker = catalog.GetSneakerFromLink(newSneaker.link);
        //        if (oldSneaker == null)
        //        {
        //            catalog.sneakers.Add(newSneaker);
        //            //ParseOneSneaker(catalog.sneakers[catalog.sneakers.Count - 1]);
        //            //DownloadSneakerImages(newSneaker, IMAGE_FOLDER);
        //            string message = "Новые кроссовки: " + newSneaker.sku + " " + newSneaker.title + " " + newSneaker.link;
        //            Program.logger.Info(message);
        //            //Console.WriteLine(message);
        //        }
        //        else
        //        {
        //            //проверяем цену
        //            if (oldSneaker.price != newSneaker.price)
        //            {
        //                isUpdate = true;
        //                string message = "Price change. sku:" + newSneaker.sku + " old price:" + oldSneaker.price + " new price:" + newSneaker.price;
        //                oldSneaker.price = newSneaker.price;
        //                Program.logger.Info(message);
        //                //Console.WriteLine(message);
        //            }
        //            if (oldSneaker.oldPrice != newSneaker.oldPrice)
        //            {
        //                isUpdate = true;
        //                oldSneaker.oldPrice = newSneaker.oldPrice;
        //            }
        //            //проверяем размеры
        //            bool isSizeUpdated = false;
        //            foreach (var sizeUS in newSneaker.sizes)
        //            {
        //                SneakerSize oldSize = oldSneaker.GetSize(sizeUS.sizeUS);
        //                if (oldSize == null)
        //                {
        //                    isSizeUpdated = true;
        //                    Console.WriteLine("new sizeUS. sku:" + newSneaker.sku + " sizeUS:" + sizeUS.sizeUS);
        //                }
        //            }
        //            //теперь проверяем старые размеры. Если какого-то из этих размеров нет в новых размерах, значит его продали и его надо убрать
        //            foreach (var sizeUS in oldSneaker.sizes)
        //            {
        //                SneakerSize newSize = newSneaker.GetSize(sizeUS.sizeUS);
        //                if (newSize == null)
        //                {
        //                    isSizeUpdated = true;
        //                    Console.WriteLine("sizeUS soldout. sku:" + newSneaker.sku + " sizeUS:" + sizeUS.sizeUS);
        //                }
        //            }
        //            if (isSizeUpdated)
        //            {
        //                isUpdate = true;
        //                oldSneaker.sizes = newSneaker.sizes;
        //            }
        //        }
        //    }
        //    //проверяем старый каталог. Если в старом каталоге есть кроссовки, которых нет в новом, значит эти кроссы уже не продают и надо их удалить
        //    Catalog newCatalog2 = new Catalog();
        //    foreach (var fullCatalogSneaker in catalog.sneakers)
        //    {

        //        //if (stockSneaker.sku == "")
        //        Sneaker newSneaker = newCatalog.GetSneakerFromLink(fullCatalogSneaker.link);
        //        if (newSneaker == null)
        //        {
        //            isUpdate = true;
        //            string message = "Sneaker deleted. sku:" + fullCatalogSneaker.sku;
        //            Program.logger.Info(message);
        //            //Console.WriteLine(message);
        //        }
        //        else
        //        {
        //            if (!newCatalog2.isExistSneakerInCatalog(fullCatalogSneaker))
        //                newCatalog2.sneakers.Add(fullCatalogSneaker);
        //        }
        //    }

        //    catalog = newCatalog2;

        //    //SaveCatalogToCSV(CATALOG_FILENAME);
        //    //SaveStockToCSV(STOCK_FILENAME);

        //    return isUpdate;
        //}

        public Catalog ParseSneakersFromAllPages()
        {
            Catalog catalog = new Catalog();

            ParseSneakersFromPage(catalog, "Nike", "https://www.overkillshop.com/en/sneaker/filter/manufacturer-nike.html?limit=288");

            ParseSneakersFromPage(catalog, "Jordan", "https://www.overkillshop.com/en/sneaker/filter/manufacturer-jordan.html?limit=288");         

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string brand, string link)
        {
            Console.WriteLine("Парсим " + link);
            Uri uri = new Uri(link);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            string source = webClient.DownloadString(uri);
            webClient.Dispose();
            var parser = new HtmlParser();
            var document = parser.Parse(source);

            //items
            var items = document.QuerySelectorAll("li.item");
            
            foreach (var item in items)
            {
                var sneaker = new Sneaker();

                //fullCatalogSneaker.sex = sex;
                //fullCatalogSneaker.categorySneakerFullCatalog = categorySneakerFullCatalog;

                //brand
                sneaker.brand = brand;

                //link
                var linkHTML = item.QuerySelector("a.product-image");
                sneaker.link = linkHTML.GetAttribute("href");

                //prices             
                var salePriceHTML = item.QuerySelector("p.special-price");
                string priceString = String.Empty;
                //если товар идет по сейлу
                if (salePriceHTML != null)
                {
                    //price
                    priceString = item.QuerySelector("p.special-price").QuerySelector("span.price").InnerHtml;
                    priceString = priceString.Replace("€", "");
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.price = double.Parse(priceString);

                    //old price
                    priceString = item.QuerySelector("p.old-price").QuerySelector("span.price").InnerHtml;
                    priceString = priceString.Replace("€", "");
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.oldPrice = double.Parse(priceString);
                }
                else
                {
                    priceString = item.QuerySelector("p.regular-price").QuerySelector("span.price").InnerHtml;
                    priceString = priceString.Replace("€", "");
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.price = double.Parse(priceString);
                }
                if (sneaker.price == 0)
                {
                    bool test = true;
                }
                else
                {
                    //add to catalog
                    if (!catalog.isExistSneakerInCatalog(sneaker))
                        catalog.sneakers.Add(sneaker);
                }
                
            }              

            //next page
            var nextPage = document.QuerySelector("a.next.i-next");
            if (nextPage != null)
            {
                string nextPageLink = nextPage.GetAttribute("href");
                //System.Threading.Thread.Sleep(5000);
                ParseSneakersFromPage(catalog, brand, nextPageLink);
            }
        }

        public void ParseAllSneakers(Catalog @catalog)
        {
            var newCatalog = new Catalog();
            int count = catalog.sneakers.Count;
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(i + " " + catalog.sneakers[i].link);
                var sneaker = ParseOneSneaker(catalog.sneakers[i]);
                if (sneaker != null) newCatalog.AddUniqueSneaker(sneaker);
                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                //System.Threading.Thread.Sleep(5000);
                //sneakerParser.DownloadSneakerImages(i);
            }
            catalog.sneakers = newCatalog.sneakers;
        }

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            string url = sneaker.link;
            //url = "http://www.sneakersnstuff.com/en/product/25912/nike-air-max-plus-gpx-premium-sp";
            Uri uri = new Uri(url);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            string source = string.Empty;
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

            var parser = new HtmlParser();
            var document = parser.Parse(source);


            if (document.QuerySelector("div.row-fluid.add-to-box") == null) //если null значит товар еще не поступил в продажу
            {
                return null;
            }
            else
            {
                //sku
                //var skuHTML = document.QuerySelector("div.text-sku.muted.manufacturer").QuerySelector("strong").InnerHtml;
                var skuHTML = String.Empty;
                try
                {
                    skuHTML = document.QuerySelector("div.text-sku.muted.manufacturer").QuerySelector("strong").InnerHtml;
                }
                catch (NullReferenceException e)
                {
                    Program.Logger.Warn("wrong sku. link:" + sneaker.link);
                    return null;
                }
                sneaker.sku = skuHTML.Trim().Replace(" ","-");
                if (sneaker.sku == "844855-370")
                {
                    bool test = true;
                }

                //color 
                var tableHTML = document.QuerySelector("div.tab-pane.active");
                var trsHTML = tableHTML.QuerySelectorAll("tr");
                foreach (var tr in trsHTML)
                {
                    var th = tr.QuerySelector("th").InnerHtml;
                    if (th == "Manufacturer Color Code")
                    {
                        sneaker.color = tr.QuerySelector("td").InnerHtml;
                        break;
                    }
                }

                //title
                var titleHTML = document.QuerySelector("div.row-fluid.product-name").QuerySelector("h2").InnerHtml;
                titleHTML = titleHTML.Replace("<br>", "").Replace("  ", " ").Trim();
                //fullCatalogSneaker.title = titleHTML.ToUpper() + " " + fullCatalogSneaker.color.ToUpper();
                sneaker.title = titleHTML.ToUpper();
                if (sneaker.color != null)
                {
                    sneaker.title +=  " " + sneaker.color.ToUpper();
                }
                //fullCatalogSneaker.title = fullCatalogSneaker.title.Replace("JORDAN BRAND", "").Replace("BRAND", "").Trim();

                //images
                //var imageContainer = document.All.Where(m => m.LocalName == "div" && m.Id == "thumbnail-wrapper").First();
                var images = document.QuerySelector("div.carousel-inner").QuerySelectorAll("a");
                //List<String> listImage = new List<String>();
                foreach (var image in images)
                {
                    //var imageHTML = image.QuerySelector("img");
                    string imageString = image.GetAttribute("href");
                    if (imageString.Contains(".jpg"))
                    {
                        sneaker.images.Add(imageString);
                    }
                    else
                    {
                        Program.Logger.Warn("wrong image " + sneaker.sku);
                    }
                }
                if (sneaker.images.Count == 0) Program.Logger.Warn("no images " + sneaker.sku);

                //categorySneakerFullCatalog sex
                var category = String.Empty;
                var dd = document.QuerySelector("div.span3.pull-right").QuerySelector("ul").GetAttribute("class");
                
                if (dd.Contains("kids")) //kids
                {
                    category = Settings.CATEGORY_KIDS;
                }
                else if (dd.Contains(" men")) //man
                {
                    category = Settings.CATEGORY_MEN;
                } else if (dd.Contains("women")) {
                    category = Settings.CATEGORY_WOMEN;
                }
                else {
                    Program.Logger.Warn("wrong category sku: " + sneaker.sku);
                    return null;
                }

                //sizes               
                //var script = document.QuerySelector("div.row-fluid.product-option-group").QuerySelectorAll("script")[1].InnerHtml;
                var script = String.Empty;
                try
                {
                    script = document.QuerySelector("div.row-fluid.product-option-group").QuerySelectorAll("script")[1].InnerHtml;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Program.Logger.Warn("wrong sizes. sku:" + sneaker.sku + " link: " + sneaker.link);
                    return null;
                }
                var sizes = script.Split(new String [] {"},{"},StringSplitOptions.None);
                for (int i = 0; i < sizes.Count(); i++)
                {
                    var size = sizes[i];
                    if (size.Contains("label_us") && !size.Contains("Out of Stock"))
                    {
                        size = size.Replace("\\r\\n", "");
                        int index1 = size.IndexOf("label_us\":\"") + 11;
                        int index2 = size.IndexOf("\",\"label_uk");
                        try
                        {
                            size = size.Substring(index1, index2 - index1);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            size = null;
                            Program.Logger.Warn("wrong size: " + sneaker.sku);
                            return null;
                        }
                        if (size != null)
                        {
                            SneakerSize snsize = new SneakerSize(size);
                            sneaker.sizes.Add(snsize);
                        }
                        
                    }
                    //var sizeUS = sizes[i].QuerySelector("span").InnerHtml;
                    //sizeUS = sizeUS.Replace("US Wm", "").Replace("US", "").Trim();
                    ////string sizeUS = sizes[i].InnerHtml.Trim();
                    //SneakerSize sizeUS = new SneakerSize(sizeUS);
                    //fullCatalogSneaker.sizes.Add(sizeUS);
                } //sizes

                //categorySneakerFullCatalog and sex
                if (sneaker.sizes.Count > 0)
                {
                    if (sneaker.sizes[0].sizeUS.Contains("C") || sneaker.sizes[0].sizeUS.Contains("Y"))
                    {
                        sneaker.category = Settings.CATEGORY_KIDS;
                    }
                    else
                    {
                        if (category == Settings.CATEGORY_WOMEN)
                        {
                            sneaker.category = Settings.CATEGORY_WOMEN;
                            sneaker.sex = Settings.GENDER_WOMAN;
                        }
                        else
                        {
                            sneaker.category = Settings.CATEGORY_MEN;
                            sneaker.sex = Settings.GENDER_MAN;
                        }
                    }
                    return sneaker;
                }
                else {
                    return null;
                }              
            }          
        }

        public void SetSellPrices()
        {
            foreach (var sneaker in catalog.sneakers)
            {
                sneaker.sellPrice = sneaker.price + 60; //примерно 35 доставка и 30 маржа
            }
        }
    }
}
