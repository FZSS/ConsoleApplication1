using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing
{
    public class AfewStoreParser : Parser
    {
        public const string SITEURL = "https://www.afew-store.com";
        public static readonly string FOLDER = DIRECTORY_PATH + @"afew\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        public const string CURRENCY = "EUR";
        public const string NAME = "afew-store.com";
        public const int MARZHA = DELIVERY_TO_USA + 25; //доставка 30, 30 маржа
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
            market_info.photo_parameters.is_watermark_image = false;
            market_info.photo_parameters.background_color = "white";
            market_info.photo_parameters.first_photo_sneaker_direction = "left"; //уточнить
            market_info.currently_language = "en";
            var json = CreateJson(market_info);
            SaveJson(json, JSON_FILENAME,FOLDER,NAME);
        }


        public Catalog ParseSneakersFromAllPages()
        {
            Catalog catalog = new Catalog();

            ParseSneakersFromPage(catalog, "Nike", "https://www.afew-store.com/en/sneaker/nike/");

            ParseSneakersFromPage(catalog, "Jordan", "https://www.afew-store.com/en/sneaker/air-jordan/");

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
                var linkHTML = item.QuerySelector("a");
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
                    priceString = item.QuerySelector("span.regular-price").QuerySelector("span.price").InnerHtml;
                    priceString = priceString.Replace("€", "");
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.price = double.Parse(priceString);
                }
                if (sneaker.price == 0)
                {
                    throw new Exception("цена равна нулю");
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

        private void ParseAllSneakers(Catalog catalog)
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
            string source = Parser.GetPage(sneaker.link);
            var parser = new HtmlParser();
            var document = parser.Parse(source);

            //sku //color
            try
            {
                var sep = new string[] {"br"};
                var sku_color = document.QuerySelector("div.std").InnerHtml.Split(sep,StringSplitOptions.None);
                var sku = sku_color[0].Replace("#", "").Replace("<","").Trim();
                if (Helper.isTrueMaskSKUForNike(sku))
                {
                    sneaker.sku = sku;
                }
                else
                {
                    Program.Logger.Warn("wrong sku. sku: " + sku + ". Link: " + sneaker.link);
                    return null;
                }

                //color
                sneaker.color = sku_color[2].Replace(">","").Replace("<","").Trim();
            }
            catch (NullReferenceException e)
            {
                Program.Logger.Warn("wrong sku. link:" + sneaker.link);
                return null;
            }

            //title
            var titleHTML = document.QuerySelector("h1").InnerHtml;
            titleHTML = titleHTML.Replace("\"", " ").Trim();
            //add brand to title
            if (sneaker.brand == "Nike")
            {
                titleHTML = sneaker.brand + " " + titleHTML;
            }
            else if (sneaker.brand == "Jordan")
            {
                titleHTML = "Air Jordan " + titleHTML;
            }
            else
            {
                throw new Exception("wrong brand");
            }
            sneaker.title = titleHTML.ToUpper().Replace("  "," ");
            //if (sneaker.color != null)
            //{
            //    sneaker.title += " " + sneaker.color.ToUpper();
            //}

            //images
            //var imageContainer = document.All.Where(m => m.LocalName == "div" && m.Id == "thumbnail-wrapper").First();
            var images = document.QuerySelectorAll("img.carousel-cell-image");
            //List<String> listImage = new List<String>();
            foreach (var image in images)
            {
                //var imageHTML = image.QuerySelector("img");
                string imageString = image.GetAttribute("src");
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

            //sizes               
            var sizes = document.QuerySelector("div.sh-size").QuerySelectorAll("li");
            //var sizesEU = new List<string>();
            //var sizes = script.Split(new String [] {"},{"},StringSplitOptions.None);
            for (int i = 0; i < sizes.Count(); i++)
            {
                var size = sizes[i];
                var sizeClass = size.QuerySelector("a").GetAttribute("class");
                if (sizeClass.Contains("sizeoption sizeactive"))
                {
                    var sizeString = size.QuerySelector("cite").InnerHtml;
                    //var sizeEU = sizeUS.QuerySelector("span").InnerHtml;
                    if (sizeString != null)
                    {
                        SneakerSize snsize = new SneakerSize(sizeString);
                        sneaker.sizes.Add(snsize);
                        //sizesEU.Add(sizeEU);
                    }

                }
            } //sizes

            //categorySneakerFullCatalog and sex
            if (sneaker.sizes.Count > 0)
            {
                var sizeChart = document.QuerySelector("table").QuerySelectorAll("tr");
                var tdList = sizeChart[1].QuerySelectorAll("td");
                var sizeUS = tdList[0].InnerHtml;
                var sizeEU = tdList[1].InnerHtml;
                string category;
                string sex;
                bool result = Helper.GetCategoryAndSex(sizeUS, sizeEU,out category,out sex);
                sneaker.category = category;
                sneaker.sex = sex;
                if (result)
                {
                    return sneaker;
                }
                else
                {
                    Program.Logger.Warn("wrong category sku:" + sneaker.sku + " link: " + sneaker.link);
                    return null;
                }
            }
            else {
                return null;
            }                       
        }

        //private RootParsingObject CreateJson()
        //{
        //    var market_info = new MarketInfo();
        //    var root = new RootParsingObject();
        //    var listings = new List<Listing>();                  

        //    market_info.market = AfewStoreParser.NAME;
        //    market_info.website = AfewStoreParser.SITEURL;
        //    market_info.currency = AfewStoreParser.CURRENCY;
        //    market_info.parse_date = DateTime.Now;
        //    market_info.delivery_to_usa = AfewStoreParser.DELIVERY_TO_USA;
        //    market_info.is_watermark_image = false;
        //    market_info.is_white_background_image = true;
        //    market_info.currently_language = "en";

        //    int i = 0;
        //    foreach (var sneaker in catalog.sneakers)
        //    {
        //        var listing = new Listing();

        //        //id murmurhash
        //        Encoding encoding = new UTF8Encoding();
        //        byte[] input = encoding.GetBytes(sneaker.link);
        //        using (MemoryStream stream = new MemoryStream(input))
        //        {
        //            listing.id = MurMurHash3.Hash(stream);
        //            if (listing.id < 0) listing.id = listing.id * -1;
        //        }

        //        listing.url = sneaker.link;
        //        listing.sku = sneaker.sku;
        //        listing.title = sneaker.title;
        //        listing.brand = sneaker.brand;
        //        listing.colorbrand = sneaker.color;
        //        listing.categorySneakerFullCatalog = Helper.ConvertCategoryRusToEng(sneaker.categorySneakerFullCatalog);
        //        listing.sex = Helper.ConvertSexRusToEng(sneaker.sex);
        //        listing.price = sneaker.price;
        //        listing.old_price = sneaker.oldPrice;
        //        listing.images = sneaker.images;
        //        listing.us_sizes = Helper.GetSizeListUs(sneaker.sizes);
        //        i++;
        //        listing.position = i;

        //        listings.Add(listing);
        //    }

        //    root.market_info = market_info;
        //    root.listings = listings;

        //    return root;
        //    //throw new NotImplementedException();
        //}

        //private void SaveJson(RootParsingObject markets, string JSON_FILENAME, string folder, striftpFolderame)
        //{
        //    var localFileName = folder + JSON_FILENAME;
        //    //сохраняем на яндекс.диск файл
        //    var textJson = JsonConvert.SerializeObject(markets);
        //    System.IO.File.WriteAllText(localFileName, textJson);
            
        //    //подгружаем из конфига данные фтп
        //    var appSettings = ConfigurationManager.AppSettings;
        //    var ftpHost = appSettings["ftpHostParsing"];
        //    var ftpUser = appSettings["ftpUserParsing"];
        //    var ftpPass = appSettings["ftpPassParsing"];

        //    //загружаем на ftp файл
        //    var path = ftpFolder + "/" + JSON_FILENAME;
        //    Helper.LoadFileToFtp(localFileName, path, ftpHost, ftpUser, ftpPass);
        //}
    }
}
