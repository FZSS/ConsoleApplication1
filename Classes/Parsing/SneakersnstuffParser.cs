using AngleSharp.Dom.Html;
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
    public class SneakersnstuffParser : Parser
    {
        public const string SITEURL = "https://www.sneakersnstuff.com";
        public static readonly string FOLDER = DIRECTORY_PATH + @"sneakersnstuff\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        public const string CURRENCY = "USD";
        public const string NAME = "sneakersnstuff.com";
        public const int MARZHA = DELIVERY_TO_USA + 25;
        public const int DELIVERY_TO_USA = 10;
        public const double VAT_VALUE = 0; //Если 15% ват, то пишем 0.15, если вычета нет, то 0

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
            market_info.photo_parameters.is_watermark_image = false;
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

            ParseSneakersFromPage(catalog, "Nike", "https://www.sneakersnstuff.com/en/2/sneakers?p=1046&orderBy=Published");

            ParseSneakersFromPage(catalog, "Jordan", "https://www.sneakersnstuff.com/en/2/sneakers?p=5954&orderBy=Published");         

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
            var items = document.QuerySelectorAll("li.product");
            
            foreach (var item in items)
            {
                var sneaker = new Sneaker();

                //fullCatalogSneaker.sex = sex;
                //fullCatalogSneaker.categorySneakerFullCatalog = categorySneakerFullCatalog;

                //brand
                sneaker.brand = brand;

                //link
                var linkHTML = item.QuerySelector("a.plink.image");
                sneaker.link = SITEURL + linkHTML.GetAttribute("href");

                //prices             
                var salePriceHTML = item.QuerySelector("span.sale");
                string priceString = String.Empty;
                //если товар идет по сейлу
                if (salePriceHTML != null)
                {
                    //price
                    priceString = salePriceHTML.InnerHtml;
                    priceString = priceString.Replace("Now: $", "");
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.price = double.Parse(priceString);

                    //old price
                    priceString = item.QuerySelector("del.sale").InnerHtml;             
                    priceString = priceString.Replace("($", "").Replace(")", "");
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.oldPrice = double.Parse(priceString);
                }
                else
                {
                    priceString = item.QuerySelector("span.price").InnerHtml;
                    priceString = priceString.Replace("$", "");
                    priceString = priceString.Replace(".", ",").Trim();
                    //если пустая строка, значит солдаут
                    if (!String.IsNullOrWhiteSpace(priceString))
                        sneaker.price = double.Parse(priceString);
                }

                //title
                //fullCatalogSneaker.title = String.Empty;
                //if (fullCatalogSneaker.brand != "Jordan") fullCatalogSneaker.title = fullCatalogSneaker.brand + " "; //у найк и найк сб нет бренда в тайтле, а у джордан уже есть
                //fullCatalogSneaker.title += item.QuerySelector("span.ftpFolder").InnerHtml;
                //fullCatalogSneaker.title = fullCatalogSneaker.title.ToUpper();
                //fullCatalogSneaker.ParseTitle();

                //add to catalog
                if (!String.IsNullOrWhiteSpace(priceString))
                {
                    if (!catalog.isExistSneakerInCatalog(sneaker))
                        catalog.sneakers.Add(sneaker);
                }

            }              

            //next page
            var nextPage = document.QuerySelector("a.control.next");
            if (nextPage != null)
            {
                string nextPageLink = SITEURL + nextPage.GetAttribute("href");
                System.Threading.Thread.Sleep(5000);
                ParseSneakersFromPage(catalog, brand, nextPageLink);
            }
        }

        public void ParseAllSneakers(Catalog @catalog)
        {
            var newCatalog = new Catalog();
            int count = catalog.sneakers.Count;
            for (int i = 0; i < count; i++)
            {
                Program.Logger.Info(i + " " + catalog.sneakers[i].link);
                var sneaker = ParseOneSneaker(catalog.sneakers[i]);
                if (sneaker != null) newCatalog.AddUniqueSneaker(sneaker);
                Program.Logger.Info(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                System.Threading.Thread.Sleep(5000);
                //sneakerParser.DownloadSneakerImages(i);
            }
            catalog.sneakers = newCatalog.sneakers;
        }

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            //string url = sneaker.link;
            ////url = "http://www.sneakersnstuff.com/en/product/25912/nike-air-max-plus-gpx-premium-sp";
            //Uri uri = new Uri(url);
            //WebClient webClient = new WebClient();
            //webClient.Encoding = Encoding.UTF8;
            //string source = string.Empty;
            //source = webClient.DownloadString(uri);
            //var parser = new HtmlParser();
            //var document = parser.Parse(source);
            var document = Parser.GetHtmlPage(sneaker.link);


            if (document.QuerySelector("div.product-buy-container") != null) //если null значит товар еще не поступил в продажу
            {
                //sku
                var skuHTML = document.All.Where(m => m.LocalName == "span" && m.Id == "product-artno").First().InnerHtml.Replace("Article number:&nbsp;","");
                sneaker.sku = skuHTML;

                //color 
                var colorHTML = document.All.Where(m => m.LocalName == "span" && m.Id == "product-color").First().InnerHtml;
                sneaker.color = colorHTML;

                //добавляем цвет к тайтлу
                var titleHTML = document.QuerySelector("h1").InnerHtml;
                titleHTML = titleHTML.Replace("<br>", "").Replace("  ", " ").Trim();
                sneaker.title = titleHTML.ToUpper() + " " + sneaker.color.ToUpper();
                sneaker.title = sneaker.title.Replace("JORDAN BRAND", "").Replace("BRAND", "").Trim();

                //images
                //var imageContainer = document.All.Where(m => m.LocalName == "div" && m.Id == "thumbnail-wrapper").First();
                var images = document.QuerySelectorAll("a.c-1");
                //List<String> listImage = new List<String>();
                foreach (var image in images)
                {
                    var imageHTML = image.QuerySelector("img");
                    string imageString = SITEURL + imageHTML.GetAttribute("data-large-image");
                    if (imageString.Contains(".jpg")) {
                        sneaker.images.Add(imageString);
                    } 
                    else
                    {
                        Program.Logger.Warn("wrong image " + sneaker.sku);
                    }
                }
                if (sneaker.images.Count == 0) Program.Logger.Warn("no images " + sneaker.sku);

                //sizes
                bool iswomen = false;
                var dd = document.QuerySelector("div.size-button.property.available").InnerHtml;     
                if (dd.Contains("US Wm")) //женские
                {
                    iswomen = true;
                }
                else if (dd.Contains("US")) //мужские
                {

                } else {
                    return null;
                }; //детские значит
                
                var sizes = document.QuerySelectorAll("div.size-button.property.available");
                for (int i = 0; i < sizes.Count(); i++)
                {
                    var sizeUS = sizes[i].QuerySelector("span").InnerHtml;
                    sizeUS = sizeUS.Replace("US Wm", "").Replace("US", "").Trim();
                    //string sizeUS = sizes[i].InnerHtml.Trim();
                    SneakerSize size = new SneakerSize(sizeUS);
                    sneaker.sizes.Add(size);
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
                        if (iswomen)
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
            else
            {
                return null;
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
