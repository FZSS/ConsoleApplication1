using AngleSharp.Parser.Html;
using CsvHelper;
using Newtonsoft.Json;
using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.SizeConverters;
using SneakerIcon.Classes.SizeConverters.Model;
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
    public class SivasParser : Parser
    {
        public const string SITEURL = "http://www.sivasdescalzo.com";
        public static readonly string FOLDER = DIRECTORY_PATH + @"sivas\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string CURRENCY = "EUR";
        public const string NAME = "sivasdescalzo.com";
        public const string FTP_FILENAME = "sivasdescalzo.com/sivasdescalzo.com_en.json";
        //public const string JSON_FILENAME = "sivasdescalzo.com_en.markets";
        public const string JSON_FILENAME = NAME + "_en.json";
        public const int MARZHA = DELIVERY_TO_USA + 25;
        public const int DELIVERY_TO_USA = 15;
        public const double VAT_VALUE = 0; //Если 15% ват, то пишем 0.15, если вычета нет, то 0

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

        public void Run2()
        {
            Program.Logger.Info("Parsing JSON " + NAME + ". Convert to Catalog and Stock CSV");
            Initialize();
            catalog = ParseCatalogFromJson();
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
            DeserializeJson(FOLDER + JSON_FILENAME);
        }

        public void DeserializeJson(string filename)
        {
            if (File.Exists(filename))
            {
                var textJson = System.IO.File.ReadAllText(filename);
                _json = JsonConvert.DeserializeObject<RootParsingObject>(textJson);
            }
            else
            {
                throw new Exception("file not exist");
            }
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
            ParseSneakersFromPage(catalog, "Nike", "http://www.sivasdescalzo.com/en/search/?q=nike&fq[size_us][0]=1C&fq[size_us][1]=2C&fq[size_us][2]=3C&fq[size_us][3]=4C&fq[size_us][4]=5C&fq[size_us][5]=6C&fq[size_us][6]=7C&fq[size_us][7]=8C&fq[size_us][8]=9C&fq[size_us][9]=10C&fq[size_us][10]=11C&fq[size_us][11]=11.5C&fq[size_us][12]=12C&fq[size_us][13]=12.5C&fq[size_us][14]=13C&fq[size_us][15]=13.5C&fq[size_us][16]=1Y&fq[size_us][17]=1.5Y&fq[size_us][18]=2Y&fq[size_us][19]=2.5Y&fq[size_us][20]=3Y&fq[size_us][21]=3.5Y&fq[size_us][22]=4&fq[size_us][23]=4Y&fq[size_us][24]=4.5&fq[size_us][25]=4.5Y&fq[size_us][26]=5&fq[size_us][27]=5Y&fq[size_us][28]=5.5&fq[size_us][29]=5.5Y&fq[size_us][30]=6&fq[size_us][31]=6Y&fq[size_us][32]=6.5&fq[size_us][33]=6.5Y&fq[size_us][34]=7&fq[size_us][35]=7Y&fq[size_us][36]=7.5&fq[size_us][37]=8&fq[size_us][38]=8.5&fq[size_us][39]=9&fq[size_us][40]=9.5&fq[size_us][41]=10&fq[size_us][42]=10.5&fq[size_us][43]=11&fq[size_us][44]=11.5&fq[size_us][45]=12&fq[size_us][46]=12.5&fq[size_us][47]=13&fq[size_us][48]=14");

            ParseSneakersFromPage(catalog, "Jordan", "http://www.sivasdescalzo.com/en/search/?fq%5Bsize_us%5D%5B0%5D=5C&fq%5Bsize_us%5D%5B1%5D=6C&fq%5Bsize_us%5D%5B2%5D=7C&fq%5Bsize_us%5D%5B3%5D=8C&fq%5Bsize_us%5D%5B4%5D=9C&fq%5Bsize_us%5D%5B5%5D=10C&fq%5Bsize_us%5D%5B6%5D=10.5C&fq%5Bsize_us%5D%5B7%5D=11C&fq%5Bsize_us%5D%5B8%5D=11.5C&fq%5Bsize_us%5D%5B9%5D=12C&fq%5Bsize_us%5D%5B10%5D=12.5C&fq%5Bsize_us%5D%5B11%5D=13C&fq%5Bsize_us%5D%5B12%5D=13.5C&fq%5Bsize_us%5D%5B13%5D=1Y&fq%5Bsize_us%5D%5B14%5D=1.5Y&fq%5Bsize_us%5D%5B15%5D=2Y&fq%5Bsize_us%5D%5B16%5D=2.5Y&fq%5Bsize_us%5D%5B17%5D=3Y&fq%5Bsize_us%5D%5B18%5D=3.5Y&fq%5Bsize_us%5D%5B19%5D=4Y&fq%5Bsize_us%5D%5B20%5D=4.5Y&fq%5Bsize_us%5D%5B21%5D=5Y&fq%5Bsize_us%5D%5B22%5D=5.5Y&fq%5Bsize_us%5D%5B23%5D=6Y&fq%5Bsize_us%5D%5B24%5D=6.5Y&fq%5Bsize_us%5D%5B25%5D=7&fq%5Bsize_us%5D%5B26%5D=7Y&fq%5Bsize_us%5D%5B27%5D=7.5&fq%5Bsize_us%5D%5B28%5D=7.5Y&fq%5Bsize_us%5D%5B29%5D=8&fq%5Bsize_us%5D%5B30%5D=8Y&fq%5Bsize_us%5D%5B31%5D=8.5&fq%5Bsize_us%5D%5B32%5D=8.5Y&fq%5Bsize_us%5D%5B33%5D=9&fq%5Bsize_us%5D%5B34%5D=9Y&fq%5Bsize_us%5D%5B35%5D=9.5&fq%5Bsize_us%5D%5B36%5D=9.5Y&fq%5Bsize_us%5D%5B37%5D=10&fq%5Bsize_us%5D%5B38%5D=10.5&fq%5Bsize_us%5D%5B39%5D=11&fq%5Bsize_us%5D%5B40%5D=11.5&fq%5Bsize_us%5D%5B41%5D=12&fq%5Bsize_us%5D%5B42%5D=12.5&fq%5Bsize_us%5D%5B43%5D=13&fq%5Bsize_us%5D%5B44%5D=14&fq%5Bsize_us%5D%5B45%5D=15&fq%5Bsize_us%5D%5B46%5D=16&p=1&q=jordan");
            
            //cохрняем результат в csv файл
            //SaveLinksToCSV(JSON_FILENAME);

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
            var items = document.QuerySelector("ul.products-list").QuerySelectorAll("li");
            
            foreach (var item in items)
            {
                var sneaker = new Sneaker();
                
                //fullCatalogSneaker.sex = sex;
                //fullCatalogSneaker.categorySneakerFullCatalog = categorySneakerFullCatalog;
                
                //brand
                sneaker.brand = brand;

                //link
                var links = item.QuerySelectorAll("a");
                sneaker.link = links[2].GetAttribute("href");

                //prices
                var priceHTML = item.QuerySelector("div.price");
                var salePriceHTML = item.QuerySelector("div.sale-price");
                string priceString = String.Empty;
                //если товар идет по сейлу
                if (salePriceHTML != null)
                {
                    //price
                    priceString = salePriceHTML.QuerySelector("b").InnerHtml;
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.price = double.Parse( priceString );

                    //old price
                    priceString = priceHTML.QuerySelector("b").InnerHtml;
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.oldPrice = double.Parse(priceString);
                }
                else 
                {
                    priceString = priceHTML.QuerySelector("b").InnerHtml;
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.price = double.Parse(priceString);
                }

                //title
                sneaker.title = item.QuerySelector("span.model").InnerHtml;
                if (!sneaker.title.Contains(brand.ToUpper())) 
                    sneaker.title = brand.ToUpper() + " " + sneaker.title;
                sneaker.ParseTitle();
                
                if (!catalog.isExistSneakerInCatalog(sneaker))
                    catalog.sneakers.Add(sneaker);
            }

            //next page
            var nextPages = document.QuerySelector("ol.left").QuerySelectorAll("a");
            foreach (var nextPage in nextPages)
            {
                if (nextPage.GetAttribute("title") == "Next page")
                {
                    string nextPageLink = nextPage.GetAttribute("href");
                    ParseSneakersFromPage(catalog, brand, nextPageLink);
                }
            }
        }

        private Catalog ParseCatalogFromJson()
        {
            Catalog catalog = new Catalog();

            var items = _json.listings;
            foreach (var item in items)
            {
                var sneaker = new Sneaker();
                sneaker.brand = item.brand;
                sneaker.link = item.url;
                sneaker.price = item.price;
                sneaker.oldPrice = item.old_price;
                sneaker.sku = item.sku;
                sneaker.images = item.images;
                if (item.sizes != null)
                {
                    if (item.sizes.Count > 0)
                    {
                        //sizes
                        foreach (var sizeitem in item.sizes)
                        {
                            string sizeUS = sizeitem.us;
                            SneakerSize size = new SneakerSize(sizeUS);
                            sneaker.sizes.Add(size);
                        } //sizes


                        //title
                        sneaker.title = item.title;
                        if (!sneaker.title.ToUpper().Contains(item.brand.ToUpper()))
                            sneaker.title = item.brand.ToUpper() + " " + sneaker.title;
                        sneaker.ParseTitle();

                        if (sneaker.title.ToUpper().Contains("NIKE NIKE"))
                        {
                            bool test = true;
                        }

                        if (item.category == "men") sneaker.category = Settings.CATEGORY_MEN;
                        else if (item.category == "women") sneaker.category = Settings.CATEGORY_WOMEN;
                        else if (item.category == "kids") sneaker.category = Settings.CATEGORY_KIDS;
                        else throw new Exception ("wrong category");

                        if (item.sex == "men") sneaker.sex = Settings.GENDER_MAN;
                        else if (item.sex == "women") sneaker.sex = Settings.GENDER_WOMAN;
                        else if (item.sex == null) sneaker.sex = null;
                        else
                        {
                            bool test = true;
                        }

                        sneaker.color = item.colorbrand;
                            
                       

                        //add to catalog
                        if (!catalog.isExistSneakerInCatalog(sneaker))
                            catalog.sneakers.Add(sneaker);
                    }
                    else
                    {
                        bool test = true;
                    }
                }
                else
                {
                    bool test = true;
                }
            }

            return catalog;
            //throw new NotImplementedException();
        }

        public void ParseAllSneakers2(Catalog @catalog)
        {
            var newCatalog = new Catalog();
            int count = catalog.sneakers.Count;
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(i + " " + catalog.sneakers[i].link);

                //ParseOneSneaker
                Sneaker sneaker = catalog.sneakers[i];

                //sneaker.sku = 

                if (sneaker != null) newCatalog.AddUniqueSneaker(sneaker);

                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
            }
        }

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            string url = sneaker.link;
            Uri uri = new Uri(url);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            string source = String.Empty;
            try
            {
                source = webClient.DownloadString(uri);
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.StackTrace);
                return null;
            }
            var parser = new HtmlParser();
            var document = parser.Parse(source);
            
            //sku
            sneaker.sku = document.QuerySelector("p.ref.hide-for-small").InnerHtml;

            //images
            var images = document.QuerySelector("div.medium-5.columns.column-carousel").QuerySelectorAll("meta");
            //List<String> listImage = new List<String>();
            foreach (var image in images)
            {
                string imageString = image.GetAttribute("content");
                if (imageString.Contains(".jpg"))
                    sneaker.images.Add(imageString);
            }
            sneaker.images.RemoveAt(sneaker.images.Count - 1);

            //sizes

            
            var HTMLSizes = document.QuerySelector("div.content.size-options.size_us-options");
            if (HTMLSizes == null) //если null значит выход модели только ожидается
            {
                return null;
            }
            else 
            {
                var sizes = HTMLSizes.QuerySelectorAll("a.size-button.available");


                foreach (var sizeitem in sizes)
                {
                    string sizeUS = sizeitem.InnerHtml.Trim();
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
                        string sizeEU = document.QuerySelector("div.content.size-options.size_eu-options").QuerySelectorAll("a.size-button.available")[0].InnerHtml;
                        SizeConverter converterMan = new SizeConverter(sneaker.sizes[0].sizeUS, Settings.CATEGORY_MEN);
                        if (converterMan.sizeEUR == sizeEU)
                        {
                            sneaker.category = Settings.CATEGORY_MEN;
                            sneaker.sex = Settings.GENDER_MAN;
                        }
                        else
                        {
                            SizeConverter converterWoman = new SizeConverter(sneaker.sizes[0].sizeUS, Settings.CATEGORY_WOMEN);
                            if (converterWoman.sizeEUR == sizeEU)
                            {
                                sneaker.category = Settings.CATEGORY_WOMEN;
                                sneaker.sex = Settings.GENDER_WOMAN;
                            }
                            else
                            {
                                Program.Logger.Warn("wrong category sku:" + sneaker.sku + " link: " + sneaker.link);
                            }
                        }
                    }
                }

                //color
                sneaker.color = document.QuerySelector("p.variant.hide-for-small").InnerHtml;
                sneaker.title += " " + sneaker.color;

                return sneaker;
            }
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

        public void SetSellPrices()
        {
            foreach (var sneaker in catalog.sneakers)
            {
                sneaker.sellPrice = sneaker.price + 40; //15 доставка 25 маржа
            }
        }
    }
}
