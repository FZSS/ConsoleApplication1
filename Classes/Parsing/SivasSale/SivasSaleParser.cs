using CsvHelper;
using Newtonsoft.Json;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Parsing.SivasSale
{
    public class SivasSaleParser
    {
        //public const string FOLDER = Exporter.DIRECTORY_PATH + @"BonanzaSivasSale\";
        //public const string JSON_FILENAME =  FOLDER + "sivasdescalzo.com_sneakers_sales_en.markets";
        //public const string CSV_FILENAME = "Bonanza.csv";
        ////public const string IMAGE_PATH = @"C:\www\SS";
        //public const string IMAGE_PATH = FOLDER + @"Images\";

        public const string FOLDER = "";
        public const string FTP_FOLDER = "sivasdescalzo.com";
        public const string FTP_FILENAME = FTP_FOLDER + "/" + JSON_FILENAME;
        public const string JSON_FILENAME = "sivasdescalzo.com_sneakers_sales_en.json";
        public const string CSV_FILENAME = "Bonanza.csv";
        public const string CSV_FILENAME2 = "BonanzaExcel.csv";
        public const string IMAGE_PATH = @"Images\";

        public const string HTTP_IMAGE_PATH = "http://80.241.220.50/ss/";
        public const double DELIVERY_TO_USA = 15;
        public const double MARZHA = 25;

        //public RootObjectSaleSivas Json { get; set; }
        //public List<ListingSaleSivas> ExportListings { get; set; }

        public void Run()
        {

            var appSettings = ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostParsing"];
            string ftpUser = appSettings["ftpUserParsing"];
            string ftpPass = appSettings["ftpPassParsing"];
            Helper.GetFileFromFtp(FOLDER + JSON_FILENAME, FTP_FILENAME, ftpHost, ftpUser, ftpPass);

            RootParsingObject Json = DeserializeJson(JSON_FILENAME);
            WriteCountListingAndSizesInFile(Json); //пишем на экран сколько в исходном файле листингов и размеров
            List<Listing> ExportListings = CreateExportLisings(Json);
            //DownloadImages(ExportListings, IMAGE_PATH);
            ExportListings = ReplaceImageNames(ExportListings, HTTP_IMAGE_PATH);
            List<SivasSaleBonanzaRecord> BonanzaRecords = CreateBonanzaRecords(ExportListings);          
            ExportToCSV(FOLDER + CSV_FILENAME, BonanzaRecords);

            Helper.LoadFileToFtp(FOLDER + CSV_FILENAME, FTP_FOLDER + "/" + CSV_FILENAME, ftpHost, ftpUser, ftpPass);
            Helper.LoadFileToFtp(FOLDER + CSV_FILENAME2, FTP_FOLDER + "/" + CSV_FILENAME, ftpHost, ftpUser, ftpPass);
        }

        private void ExportToCSV(string filename, List<SivasSaleBonanzaRecord> Records)
        {
            //string JSON_FILENAME = @"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\BonanzaExport.csv";
            using (var sw = new StreamWriter(filename))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ",";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(Records);
            }

            filename = filename.Replace("Bonanza.csv", "BonanzaExcel.csv");
            using (var sw = new StreamWriter(filename))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(Records);
            }
        }

        private List<SivasSaleBonanzaRecord> CreateBonanzaRecords(List<Listing> ExportListings)
        {
            var Records = new List<SivasSaleBonanzaRecord>();

            foreach (var listing in ExportListings)
            {
                for (int i = 0; i < listing.sizes.Count; i++)
                {
                    var record = new SivasSaleBonanzaRecord();
                    var sizeUS = listing.sizes[i].us;
                    var sizeUK = listing.sizes[i].uk;
                    var sizeEU = listing.sizes[i].eu;
                    var sizeCM = listing.sizes[i].cm;

                    //id
                    record.id = listing.sku + "-" + sizeUS;

                    //description
                    var sizeString = sizeUS + " US / " + sizeUK + " UK / " + sizeEU + " EU / " + sizeCM + " CM";
                    record.description =
                        "<h3 style=\"text-align: center;\"><strong>" +
                        listing.title.ToUpper() +
                        "</strong></h3>" +
                        "<h3 style=\"text-align: center;\"><strong>" +
                        "STYLE: " + listing.sku +
                        "</strong></h3>" +
                        "<h3 style=\"text-align: center;\"><strong>" +
                        "SIZE: " + sizeString +
                        "</strong></h3>" +
                        "<h3 style=\"text-align: center;\"><strong>" +
                        "100% AUTHENTIC" +
                        "</strong></h3>" +
                        "<h3 style=\"text-align: center;\"><strong>" +
                        "WORLDWIDE SHIPPING FOR 5-10 DAYS IN DOUBLE BOX WITH TRACKING NUMBER.<br>" +
                        "</strong></h3>" +
                        "<h3 style=\"text-align: center;\"><strong>" +
                        "SHIP IN 2 BUSINESS DAY." +
                        "</strong></h3>";

                    //quantity
                    record.quantity = 2;

                    //categorySneakerFullCatalog
                    record.category = "11450"; //общая категория fashion

                    //title
                    //var sizeStrTitle = "SZ " + sizeUS + "US";
                    //var countSizeStr = sizeStrTitle.Count();
                    //var title = listing.title.Trim();
                    //var titleLength = title.Count();
                    //if (titleLength > 47 - countSizeStr)
                    //{
                    //    title = title.Substring(0, 47 - countSizeStr - 1);
                    //}
                    //title = title + " " + sizeStrTitle + " " + listing.sku;

                    string hvost = " SZ " + sizeUS + "US " + listing.sku;
                    if (listing.title.Count() + hvost.Count() < 80)
                    {
                        record.title = listing.title.ToUpper() + hvost;
                    }
                    else
                    {
                        int indexSubstringTitle = 79 - hvost.Count();
                        record.title = listing.title.ToUpper().Substring(0, indexSubstringTitle) + hvost;
                        int titlecount = record.title.Count();
                    }

                    if (listing.sku == "G61070")
                    {
                        bool test = true;
                    }

                    //price
                    //цена в евро, преобразуем её в доллары
                    var price = ((listing.price + DELIVERY_TO_USA + MARZHA) * 1.18); //18% комиссия примерно 
                    CurrencyRate currate = CurrencyRate.ReadObjectFromJsonFile();
                    double curInputRate = currate.GetCurrencyRate("EUR");
                    double curOutputRate = currate.GetCurrencyRate("USD");
                    double curInputRateBuy = curInputRate * 1.05;
                    double curOutputRateSell = curOutputRate * 1;
                    var priceBonanza = (price * curInputRateBuy / curOutputRateSell) - 20;
                    priceBonanza = Math.Round(priceBonanza, 2);
                    record.price = priceBonanza.ToString("F", CultureInfo.CreateSpecificCulture("en-US"));

                    //shipping
                    record.shipping_type = "fixed";
                    record.worldwide_shipping_type = "fixed";
                    record.shipping_price = 20;
                    record.worldwide_shipping_price = 20;

                    //images
                    record.image1 = HTTP_IMAGE_PATH + listing.sku + "-1.jpg";
                    record.image2 = HTTP_IMAGE_PATH + listing.sku + "-2.jpg";
                    record.image3 = HTTP_IMAGE_PATH + listing.sku + "-3.jpg";
                    record.image4 = HTTP_IMAGE_PATH + listing.sku + "-4.jpg";

                    record.brand = listing.brand;
                    record.size = sizeUS;
                    record.us_size = sizeUS;
                    record.condition = "New with box";
                    record.force_update = "true";

                    Records.Add(record);
                }
            }

            return Records;
        }

        private List<Listing> ReplaceImageNames(List<Listing> ExportListings, string http_image_path)
        {
            var newListings = new List<Listing>();
            foreach (var listing in ExportListings)
	        {
                for (int i = 1; i < listing.images.Count + 1; i++)
                {
                    listing.images[i-1] = http_image_path + listing.sku + "-" + i + ".jpg";
                }
                newListings.Add(listing);		 
	        }

            return newListings;
        }

        private void DownloadImages(List<Listing> ExportListings, string IMAGE_PATH)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostContabo"];
            string ftpUser = appSettings["ftpUserContabo"];
            string ftpPass = appSettings["ftpPassContabo"];

            foreach (var listing in ExportListings)
            {
                //проверяем, загружены ли для этого листинга изображения

                bool isImageExist = CheckListingImage(listing.sku);
                if (!isImageExist)
                {
                    DownloadImage(listing, IMAGE_PATH, ftpHost, ftpUser, ftpPass);
                }
                
            }
        }

        private bool CheckListingImage(string sku)
        {
            List<string> images = new List<string>();
            Console.WriteLine("check photo for sku: " + sku);
            for (int i = 1; i < 5; i++)
            {
                string url = "http://80.241.220.50/ss/" + sku + "-" + i + ".jpg";

                if (NetworkUtils.UrlExists(url))
                {
                    images.Add(url);
                }
            }
            if (images.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            //throw new NotImplementedException();
        }

        private void DownloadImage(Listing listing, string IMAGE_PATH, string ftpHost, string ftpUser, string ftpPass)
        {
            int j = 1;
            foreach (var item in listing.images)
            {
                string filename2 = listing.sku + "-" + j + ".jpg";
                string filename = IMAGE_PATH + listing.sku + "-" + j + ".jpg";
                j++;
                //check exist image file
                //if (!File.Exists(filename))
                //{
                    WebClient client = new WebClient();
                    string url = item;
                    Uri uri = new Uri(url);
                    client.DownloadFile(uri, filename);
                    System.Threading.Thread.Sleep(5000);
                    Helper.LoadFileToFtp(filename,"SS/" + filename2, ftpHost, ftpUser, ftpPass);
                    Console.WriteLine("image downloaded: " + filename);
                //}
                //else
                //{
                //    Console.WriteLine("image exist: " + filename);
                //}
            }
            Console.WriteLine("image downloaded for sku: " + listing.sku);
        }


        private void WriteCountListingAndSizesInFile(RootParsingObject Json)
        {
            int i = 0;
            foreach (var item in Json.listings)
            {
                i += item.sizes.Count;
            }
            Program.Logger.Info("В исходном файле листингов:" + Json.listings.Count);
            Program.Logger.Info("В исходном файле размеров:" + i);

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

        public List<Listing> CreateExportLisings(RootParsingObject Json)
        {
            var listings = new List<Listing>();
            int sizesCount = 0;

            foreach (var jsonListing in Json.listings)
            {
                //var price = ((jsonListing.price + DELIVERY_TO_USA + MARZHA) * 1.18);
                //if (price < jsonListing.old_price)
                //{
                    var brand = jsonListing.brand.ToUpper();
                    if (!brand.Contains("NIKE") && !brand.Contains("JORDAN"))
                    {
                        listings.Add(jsonListing);
                        sizesCount += jsonListing.sizes.Count;
                    }
                //}
            }

            Program.Logger.Info("Листингов для выгрузки: " + listings.Count);
            Program.Logger.Info("Размеров для выгрузки:" + sizesCount);

            //посчитаем кол-во размеров выгружаемых

            return listings;
        }
    }
}
