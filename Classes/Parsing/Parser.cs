using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using CsvHelper;
using Newtonsoft.Json;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.SizeConverters;
using SneakerIcon.Classes.Stocks;
using SneakerIcon.Classes.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using NLog;
using OpenQA.Selenium.PhantomJS;
using SneakerIcon.Model.Enum;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing
{
    public class Parser
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public static readonly string DIRECTORY_PATH = Config.GetConfig().DirectoryPathParsing;
        public string ImageFolder { get; set; }
        //public List<Sneaker> sneakers { get; set; }
        public Catalog catalog = new Catalog();
        public OnlineShopStock stock = new OnlineShopStock();
        public FullCatalog fullCatalog = new FullCatalog();
        public SizeConverter sizeConverter = new SizeConverter();

        public Parser()
        {
            //sneakers = new List<Sneaker>();
            //catalog.sneakers = @sneakers;
        }

        public void ReadCatalogFromCSV(string filename)
        {
            catalog.ReadCatalogFromCSV(filename);
            //using (var sr = new StreamReader(JSON_FILENAME, Encoding.GetEncoding(1251)))
            //{
            //    var reader = new CsvReader(sr);

            //    //reader.Configuration.Encoding = Encoding.GetEncoding(1251);
            //    reader.Configuration.Delimiter = ";";

            //    //CSVReader will now read the whole file into an enumerable
            //    IEnumerable<CatalogRecord> records = reader.GetRecords<CatalogRecord>();

            //    //var links = records.ToArray();

            //    foreach (var record in records)
            //    {
            //        Sneaker fullCatalogSneaker = record.ReadSneakerFromCatalogRecord();
            //        catalog.sneakers.Add(fullCatalogSneaker);
            //    }
            //}
        }

        public void SaveCatalogToCSV(string filename)
        {
            catalog.SaveCatalogToCSV(filename);
            //string filenameCatalog = JSON_FILENAME;
            ////string filenameCatalog = @"C:\SneakerIcon\CSV\StreetBeat\CatalogStreetBeat.csv";
            //int count = this.catalog.sneakers.Count;
            //List<CatalogRecord> catalog = new List<CatalogRecord>();
            //for (int i = 0; i < count; i++)
            //{
            //    CatalogRecord record = new CatalogRecord(this.catalog.sneakers[i]);
            //    catalog.Add(record);
            //}
            ////var streamWriter = new Stream(filenameCatalog);
            //using (var sw = new StreamWriter(filenameCatalog,false,Encoding.GetEncoding(1251)))
            //{
            //    //sw.Encoding = Encoding.GetEncoding(1251);
            //    var writer = new CsvWriter(sw);
            //    writer.Configuration.Delimiter = ";";
            //    //writer.Configuration.Encoding = Encoding.GetEncoding(1251);
            //    writer.WriteRecords(catalog);
            //}
        }
        
        public void SaveStockToCSV(string filename)
        {
            catalog.SaveStockToCSV(filename);
            //string filenameCatalog = JSON_FILENAME;
            //int count = this.catalog.sneakers.Count;
            //List<StockRecord> stock = new List<StockRecord>();
            //for (int i = 0; i < count; i++)
            //{
            //    for (int j = 0; j < this.catalog.sneakers[i].sizes.Count; j++)
            //    {
            //        StockRecord record = new StockRecord();
            //        Sneaker fullCatalogSneaker = this.catalog.sneakers[i];
            //        record.sku = fullCatalogSneaker.sku;
            //        record.title = fullCatalogSneaker.title;
            //        record.price = fullCatalogSneaker.price;
            //        record.oldPrice = fullCatalogSneaker.oldPrice;
            //        record.quantity = 1;
            //        record.queensLink = fullCatalogSneaker.queensLink;
            //        record.sizeUS = fullCatalogSneaker.sizes[j].sizeUS;
            //        stock.Add(record);
            //    }
            //}
            //using (var sw = new StreamWriter(filenameCatalog))
            //{
            //    var writer = new CsvWriter(sw);
            //    writer.Configuration.Delimiter = ";";
            //    //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
            //    writer.WriteRecords(stock);
            //}
        }

        public void DownloadAllSneakersImages(string directoryPath)
        {
            int i = 0;
            foreach (var sneaker in catalog.sneakers)
            {
                i++;
                Console.WriteLine(i);
                DownloadSneakerImages(sneaker, directoryPath);
            }
        }

        public void DownloadSneakerImages(Sneaker sneaker, string directoryPath)
        {
            for (int i = 0; i < sneaker.images.Count; i++)
            {


                int j = i + 1;
                string filename = directoryPath + sneaker.sku + "-" + j + ".jpg";
                //check exist image file
                if (!File.Exists(filename)) {
                    WebClient client = new WebClient();
                    string url = sneaker.images[i];
                    //string url = stockSneaker.images[i].Replace("/616/", "/2000/").Replace("/370/", "/1200/");
                    Uri uri = new Uri(url);       
                    client.DownloadFile(uri, filename);
                    System.Threading.Thread.Sleep(100);
                    Console.WriteLine("image downloaded: " + filename);
                }
                else
                {
                    Console.WriteLine("image exist: " + filename);
                }
            }
        }

        public void MergeCatalogAndStock()
        {
            foreach (var size in stock.records)
            {
                Sneaker sneaker = catalog.GetSneakerFromSKU(size.sku);
                if (sneaker == null)
                {
                    Program.Logger.Warn("нет в каталоге: " + size.sku);
                }
                else
                {
                    sneaker.sizes.Add(new SneakerSize(size.size));
                }
            }
        }

        public RootParsingObject DeserializeJson(string filename)
        {
            if (File.Exists(filename))
            {
                var textJson = System.IO.File.ReadAllText(filename);
                return JsonConvert.DeserializeObject<RootParsingObject>(textJson);
            }
            else
            {
                throw new Exception("file not exist");
            }
        }

        public Catalog ParseCatalogFromJson(RootParsingObject json)
        {
            Catalog catalog = new Catalog();

            var items = json.listings;
            foreach (var item in items)
            {
                var sneaker = new Sneaker();
                sneaker.brand = item.brand;
                sneaker.link = item.url;
                sneaker.price = item.price;
                sneaker.oldPrice = item.old_price;
                sneaker.sku = item.sku;
                sneaker.images = item.images;
                sneaker.color = item.colorbrand;
                if (item.sizes != null)
                {
                    if (item.sizes.Count > 0)
                    {

                        //title
                        sneaker.title = item.title;
                        if (!sneaker.title.ToUpper().Contains(item.brand.ToUpper()))
                            sneaker.title = item.brand.ToUpper() + " " + sneaker.title;
                        sneaker.ParseTitle();

                        //categorySneakerFullCatalog
                        if (item.category == "men") sneaker.category = Settings.CATEGORY_MEN;
                        else if (item.category == "women") sneaker.category = Settings.CATEGORY_WOMEN;
                        else if (item.category == "kids") sneaker.category = Settings.CATEGORY_KIDS;
                        //если категория нул, смотрим есть ли артикул в фулкаталоге и заполнена ли у него категория
                        if (String.IsNullOrWhiteSpace(item.category))
                        {
                            var fullCatalogSneaker = fullCatalog.GetSneakerFromSKU(item.sku);
                            if (fullCatalogSneaker != null) {
                                if (!String.IsNullOrWhiteSpace(fullCatalogSneaker.category)) {
                                    sneaker.category = fullCatalogSneaker.category;
                                }
                            }
                            else
                            {
                                bool test = true;
                            }
                        }
                        //если категория нул, то пробуем ее определить по другим размерам
                        if (String.IsNullOrWhiteSpace(sneaker.category))
                        {
                            if (item.sizes != null)
                            {
                                if (item.sizes.Count > 0)
                                {
                                    var sizeitem = item.sizes[0];
                                    sneaker.category = SizeConverters.SizeConverter.GetCategory(sizeitem.us, sizeitem.eu, sizeitem.uk, sizeitem.cm);
                                }
                            }
                        }

                        //Если категория пустая, то дальше нет смысла продолжать
                        if (!String.IsNullOrWhiteSpace(sneaker.category))
                        {
                            //sizes
                            foreach (var sizeitem in item.sizes)
                            {
                                string sizeUS = String.Empty; 
                                if (!String.IsNullOrWhiteSpace(sizeitem.us)) {
                                    sizeUS = sizeitem.us;
                                }
                                else if (!String.IsNullOrWhiteSpace(sizeitem.eu))
                                {
                                    var sizes = sizeConverter.sizeChart.sizes.FindAll(x => x.eu == sizeitem.eu);
                                    var engCategory = Helper.ConvertCategoryRusToEng(sneaker.category);
                                    var size = sizes.Find(x => x.category == engCategory);
                                    if (size != null)
                                    {
                                        sizeUS = size.us;
                                    }
                                }
                                else if (!String.IsNullOrWhiteSpace(sizeitem.uk))
                                {
                                    var sizes = sizeConverter.sizeChart.sizes.FindAll(x => x.uk == sizeitem.uk);
                                    var engCategory = Helper.ConvertCategoryRusToEng(sneaker.category);
                                    var size = sizes.Find(x => x.category == engCategory);
                                    if (size != null)
                                    {
                                        sizeUS = size.us;
                                    }
                                }
                                else if (!String.IsNullOrWhiteSpace(sizeitem.cm))
                                {
                                    var sizes = sizeConverter.sizeChart.sizes.FindAll(x => x.cm == sizeitem.cm);
                                    var engCategory = Helper.ConvertCategoryRusToEng(sneaker.category);
                                    var size = sizes.Find(x => x.category == engCategory);
                                    if (size != null)
                                    {
                                        sizeUS = size.us;
                                    }
                                }
                                else
                                {
                                    throw new Exception("wrong size");
                                }

                                if (!String.IsNullOrWhiteSpace(sizeUS))
                                {
                                    SneakerSize size = new SneakerSize(sizeUS);
                                    sneaker.sizes.Add(size);
                                }
                                else
                                {
                                    Program.Logger.Warn("Wrong size or category. SKU:" + item.sku + " category:" + sneaker.category + " Size: us:" + sizeitem.us + " eu:" + sizeitem.eu + " uk:" + sizeitem.uk + " cm:" + sizeitem.cm);
                                    //throw new Exception("Wrong sizeUS");
                                }


                            } //sizes

                            //sex
                            if (item.sex == "men") sneaker.sex = Settings.GENDER_MAN;
                            else if (item.sex == "women") sneaker.sex = Settings.GENDER_WOMAN;
                            else if (item.sex == null) sneaker.sex = null;
                            else
                            {
                                Program.Logger.Warn("wrong sex: " + item.sku);
                                bool test = true;
                            }

                            //add to catalog
                            if (!catalog.isExistSneakerInCatalog(sneaker))
                                catalog.sneakers.Add(sneaker);
                        }
                        else //если категория нулл
                        {
                            Program.Logger.Warn("wrong category: " + item.sku);
                        }


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

        public static string GetPage(string url)
        {
            //string url = sneaker.link;
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
                    System.Threading.Thread.Sleep(5000);
                    numberTry++;
                    //return null;
                }
            return source;
        }

        public static IHtmlDocument GetHtmlPage(string url, int tryCount = 5)
        {
            for (int i = 0; i < tryCount; i++)
            {
                try
                {
                    Uri uri = new Uri(url);
                    WebClient webClient = new WebClient();
                    webClient.Encoding = Encoding.UTF8;
                    string source = string.Empty;
                    source = webClient.DownloadString(uri);
                    var parser = new HtmlParser();
                    var document = parser.Parse(source);
                    return document;
                }
                catch (WebException e)
                {
                    Program.Logger.Error("Ошибка при получении вебстраницы");
                    Program.Logger.Error(e.StackTrace);
                    System.Threading.Thread.Sleep(10000);
                }
            }
            return null;
        }

        public static IHtmlDocument GetHtmlPagePhantomJs(string url, int tryCount = 5)
        {
            //PhantomJSOptions phoptions = new PhantomJSOptions();
            //phoptions.AddAdditionalCapability("proxy", "http://proxy.crawlera.com:8010");
            //var encodedApiKey = Helper.Base64Encode("36f14b90c38c4005a81ccbed16a31f58:");
            //phoptions.AddAdditionalCapability("phantomjs.page.customHeaders.Proxy-Authorization", "Basic " + encodedApiKey);
            ////request.Headers.Add("Proxy-Authorization", "Basic " + encodedApiKey);
            //var _driver = new PhantomJSDriver(phoptions);

            PhantomJSDriverService service = PhantomJSDriverService.CreateDefaultService();
            service.AddArgument(string.Format("--proxy-auth={0}:{1}", "36f14b90c38c4005a81ccbed16a31f58", ""));
            service.AddArgument(string.Format("--proxy={0}:{1}", "http://proxy.crawlera.com", "8010"));
            var _driver = new PhantomJSDriver(service);

            _driver.Navigate().GoToUrl(url);
            var source = _driver.PageSource;
            var parser = new HtmlParser();
            var document = parser.Parse(source);
            _driver.Quit();
            return document;
        }

        public static IHtmlDocument GetHtmlPageCrawlera5Try(string url, ERegion region = ERegion.ALL)
        {
            int i = 0;
            bool isGetPage = false;
            while (i < 5)
            {
                try
                {
                    var page = GetHtmlPageCrawlera(url, region);
                    return page;
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);
                    _logger.Error(e.StackTrace);
                    i++;
                }
            }
            _logger.Error("page didn't get: " + url);
            return null;
        }

        public static IHtmlDocument GetHtmlPageCrawlera(string url, ERegion region = ERegion.ALL)
        {
            string apiKeyUs = "2abdabfa02ab4595bbdbb92ac4088432";
            string apiKeyAll = "36f14b90c38c4005a81ccbed16a31f58";
            string apiKey = apiKeyAll;
            if (region == ERegion.ALL)
                apiKey = apiKeyAll;
            if (region == ERegion.US)
                apiKey = apiKeyUs;

            return GetHtmlPageCrawlera(url, apiKey);
        }

        private static IHtmlDocument GetHtmlPageCrawlera(string url, string api)
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var myProxy = new WebProxy("http://proxy.crawlera.com:8010");
            myProxy.Credentials = new NetworkCredential(api, "");

            //string url = "https://twitter.com/";
            //string url = "https://api.upcitemdb.com/prod/trial/search?s=nike%20859524-005&match_mode=1&type=product";
            //string url = Url + "?" + Data;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            var encodedApiKey = Helper.Base64Encode(api + ":");
            request.Headers.Add("Proxy-Authorization", "Basic " + encodedApiKey);
            //request.Proxy = proxy;
            //request.PreAuthenticate = true;

            request.Proxy = myProxy;
            request.PreAuthenticate = true;

            WebResponse response = request.GetResponse();

            HttpWebResponse httpResponse = (HttpWebResponse)response;
            var statusCode = httpResponse.StatusCode;

            //Console.WriteLine("Response Status: "
            //  + ((HttpWebResponse)response).StatusDescription);
            //Console.WriteLine("\nResponse Headers:\n"
            //  + ((HttpWebResponse)response).Headers);

            Stream dataStream = response.GetResponseStream();

            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            //Console.WriteLine("Response Body:\n" + responseFromServer);
            reader.Close();
            response.Close();

            var parser = new HtmlParser();
            var document = parser.Parse(responseFromServer);
            return document;
        }

        protected RootParsingObject CreateJson(MarketInfo market_info)
        {
            //var market_info = new MarketInfo();
            var root = new RootParsingObject();
            var listings = new List<Listing>();

            //market_info.ftpFolder = AfewStoreParser.NAME;
            //market_info.website = AfewStoreParser.SITEURL;
            //market_info.currency = AfewStoreParser.CURRENCY;
            //market_info.start_parse_date = DateTime.Now;
            //market_info.end_parse_date = DateTime.Now;
            //market_info.delivery_to_usa = AfewStoreParser.DELIVERY_TO_USA;
            //market_info.photo_parameters.is_watermark_image = false;
            //market_info.photo_parameters.background_color = "white";
            //market_info.currently_language = "en";

            int i = 0;
            foreach (var sneaker in catalog.sneakers)
            {
                var listing = new Listing();

                //id murmurhash
                Encoding encoding = new UTF8Encoding();
                if (sneaker.link != null)
                {
                    byte[] input = encoding.GetBytes(sneaker.link);
                    using (MemoryStream stream = new MemoryStream(input))
                    {
                        listing.id = MurMurHash3.Hash(stream);
                        if (listing.id < 0) listing.id = listing.id * -1;
                    }
                }
                else
                {
                    byte[] input = encoding.GetBytes(sneaker.title);
                    using (MemoryStream stream = new MemoryStream(input))
                    {
                        listing.id = MurMurHash3.Hash(stream);
                        if (listing.id < 0) listing.id = listing.id * -1;
                    }
                }


                listing.url = sneaker.link;
                listing.sku = sneaker.sku;
                listing.title = sneaker.title;
                listing.brand = sneaker.brand;
                listing.colorbrand = sneaker.color;
                listing.category = Helper.ConvertCategoryRusToEng(sneaker.category);
                //if (String.IsNullOrWhiteSpace(listing.category)) Program.Logger.Warn("Wrong category: " + sneaker.category + " sku: " +sneaker.sku);
                listing.sex = Helper.ConvertSexRusToEng(sneaker.sex);
                listing.price = sneaker.price;
                listing.old_price = sneaker.oldPrice;
                listing.images = sneaker.images;
                listing.sizes = Helper.GetSizeListUs(sneaker.sizes);
                i++;
                listing.position = i;

                listings.Add(listing);
            }

            root.market_info = market_info;
            root.listings = listings;

            return root;
            //throw new NotImplementedException();
        }

        protected static RootParsingObject LoadLocalFileJson(string filename, string folder)
        {
            var localFileName = folder + filename;
            var text = File.ReadAllText(localFileName);
            return JsonConvert.DeserializeObject<RootParsingObject>(text);
        }

        protected void SaveJson(RootParsingObject json, string filename, string folder, string name)
        {
            var localFileName = folder + filename;
            //сохраняем на яндекс.диск файл
            var textJson = JsonConvert.SerializeObject(json);
            System.IO.File.WriteAllText(localFileName, textJson);

            //подгружаем из конфига данные фтп
            var appSettings = ConfigurationManager.AppSettings;
            var ftpHost = appSettings["ftpHostParsing"];
            var ftpUser = appSettings["ftpUserParsing"];
            var ftpPass = appSettings["ftpPassParsing"];

            //загружаем на ftp файл
            var ftpFileName = name + "/" + filename;
            Helper.LoadFileToFtp(localFileName, ftpFileName, ftpHost, ftpUser, ftpPass);
        }

        public static string CorrectTitle(string title, string brand)
        {

            //добавляем бренд, если его нет в заголовке
            if (!title.ToUpper().Contains(brand.ToUpper()))
            {
                title = brand + " " + title;
            }

            //nike nike
            if (title.ToUpper().Contains("NIKE NIKE"))
            {
                title = title.ToUpper().Replace("NIKE NIKE", "NIKE");
            }

            //jordan jordan
            if (title.ToUpper().Contains("JORDAN JORDAN"))
            {
                title = title.ToUpper().Replace("JORDAN JORDAN", "JORDAN");
            }

            //jordan air jordan
            if (title.ToUpper().Contains("JORDAN AIR JORDAN"))
            {
                title = title.ToUpper().Replace("JORDAN AIR JORDAN", "AIR JORDAN");
            }

            //Nike WMNS Nike
            title = title.ToUpper().Replace("NIKE WMNS NIKE", "WMNS NIKE");

            return title;
        }
    }
}
