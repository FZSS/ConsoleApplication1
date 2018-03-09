using AngleSharp.Parser.Html;
using CsvHelper;
using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class TitoloParser : Parser
    {
        public const string SITEURL = "https://en.titolo.ch";
        public static readonly string FOLDER = DIRECTORY_PATH + @"titolo\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        public const string FTP_FILENAME = NAME + "/" + JSON_FILENAME;
        public const string CURRENCY = "CHF";
        public const string NAME = "titolo.ch";
        public const int MARZHA = DELIVERY_TO_USA + 25;
        public const int DELIVERY_TO_USA = 36;
        public const double VAT_VALUE = 0;
        private RootParsingObject _json { get; set; }

        public void Run()
        {
            catalog = ParseSneakersFromAllPages(); //парсим все ссылки и общую инфу
            ParseAllSneakers(catalog); //парсим все кроссы по одному
            SetSellPrices();

            //UpdateCatalogAndStock();

            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);

            //ReadCatalogFromCSV(CATALOG_FILENAME);
            //DownloadAllSneakersImages(IMAGE_FOLDER);
            //DownloadAllSneakersImages(DIRECTORY_PATH + @"BigImages\");

           
        }

        public void Run2() {
            Program.Logger.Info("Parsing JSON " + NAME + ". Convert to Catalog and Stock CSV");
            Initialize();
            catalog = ParseCatalogFromJson(_json);
            SetSellPrices();
            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);
        }

        public void Initialize()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostParsing"];
            string ftpUser = appSettings["ftpUserParsing"];
            string ftpPass = appSettings["ftpPassParsing"];
            Helper.GetFileFromFtp(FOLDER + JSON_FILENAME, FTP_FILENAME, ftpHost, ftpUser, ftpPass);
            _json = DeserializeJson(FOLDER + JSON_FILENAME);
        }

        public void Update()
        {
            UpdateCatalogAndStock();
            SetSellPrices();
            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);
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
                ParseOneSneaker(newSneaker);
                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                i++;

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
            //ParseSneakersFromPage(catalog, "Nike", "https://en.titolo.ch/sneakers/nike?limit=108");
            ParseSneakersFromPage(catalog, "Nike", "https://en.titolo.ch/sneakers/nike?limit=36");
        

            //ParseSneakersFromPage(catalog, "Nike SB", "https://en.titolo.ch/sneakers/nike-sb?limit=108");

            ParseSneakersFromPage(catalog, "Jordan", "https://en.titolo.ch/sneakers/jordan?limit=36");
            
            //cохрняем результат в csv файл
            //SaveLinksToCSV(JSON_FILENAME);

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string brand, string link)
        {
            //link = "https://en.titolo.ch/sneakers/nike?limit=36&p=3";
            Console.WriteLine("Парсим " + link);
            Uri uri = new Uri(link);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.85 Safari/537.36");

            string source = webClient.DownloadString(uri);
            webClient.Dispose();
            var parser = new HtmlParser();
            var document = parser.Parse(source);

            //items
            var items = document.QuerySelectorAll("li.item");
            
            foreach (var item in items)
            {
                if (item.QuerySelector("p.out-of-stock") == null)
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
                    var priceHTML = item.QuerySelector("div.price-box");
                    var salePriceHTML = item.QuerySelector("p.special-price");
                    string priceString = String.Empty;
                    //если товар идет по сейлу
                    if (salePriceHTML != null)
                    {
                        //price
                        priceString = salePriceHTML.QuerySelector("span.price").InnerHtml;
                        priceString = priceString.Replace("CHF", "");
                        priceString = priceString.Replace(".", ",").Trim();
                        sneaker.price = double.Parse(priceString);

                        //old price
                        priceString = priceHTML.QuerySelector("p.old-price").QuerySelector("span.price").InnerHtml;
                        priceString = priceString.Replace("CHF", "");
                        priceString = priceString.Replace(".", ",").Trim();
                        sneaker.oldPrice = double.Parse(priceString);
                    }
                    else
                    {
                        priceString = priceHTML.QuerySelector("span.price").InnerHtml;
                        priceString = priceString.Replace("CHF", "");
                        priceString = priceString.Replace(".", ",").Trim();
                        sneaker.price = double.Parse(priceString);
                    }

                    //title
                    sneaker.title = String.Empty;
                    if (sneaker.brand != "Jordan") sneaker.title = sneaker.brand + " "; //у найк и найк сб нет бренда в тайтле, а у джордан уже есть
                    sneaker.title += item.QuerySelector("span.name").InnerHtml;
                    sneaker.title = sneaker.title.ToUpper();
                    sneaker.ParseTitle();

                    //add to catalog
                    if (!catalog.isExistSneakerInCatalog(sneaker))
                        catalog.sneakers.Add(sneaker);
                }              
            }

            //next page
            var nextPage = document.QuerySelector("a.next");
            if (nextPage != null)
            {
                string nextPageLink = nextPage.GetAttribute("href");
                //Thread.Sleep(5000);
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
                //sneakerParser.DownloadSneakerImages(i);
            }
            catalog.sneakers = newCatalog.sneakers;
        }

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            string url = sneaker.link;
            Uri uri = new Uri(url);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            string source = webClient.DownloadString(uri);
            var parser = new HtmlParser();
            var document = parser.Parse(source);
            
            
            if (document.QuerySelector("div.add-to-cart") != null) //если null значит товар еще не поступил в продажу
            {
                //sku
                var ul_sku_color = document.QuerySelector("ul.sku-color");
                sneaker.sku = ul_sku_color.QuerySelectorAll("li")[0].InnerHtml;

                //color 
                sneaker.color = ul_sku_color.QuerySelectorAll("li")[1].InnerHtml;

                //добавляем цвет к тайтлу
                sneaker.title = sneaker.title + " " + sneaker.color.ToUpper();

                //images
                var images = document.QuerySelectorAll("li.large-2.medium-6.small-6.columns");
                //List<String> listImage = new List<String>();
                foreach (var image in images)
                {
                    string imageString = image.QuerySelector("a").GetAttribute("href");
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
                var dd = document.QuerySelector("dd.attribute-sizes.attribute-custom-size_us");     
                if (dd == null) //это значит что женские
                {
                    dd = document.QuerySelector("dd.attribute-sizes.attribute-custom-size_uswomen");
                    iswomen = true;
                }
                if (dd == null) return null;
                var sizes = dd.QuerySelectorAll("option");

                for (int i = 1; i < sizes.Count(); i++)
                {
                    string sizeUS = sizes[i].InnerHtml.Trim();
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
                }

                return sneaker;
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