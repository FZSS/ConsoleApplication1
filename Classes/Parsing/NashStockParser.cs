using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.Stocks;
using SneakerIcon.Classes.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing
{
    public class NashStockParser : Parser
    {
        public const string SITEURL = "";
        public static readonly string FOLDER = DIRECTORY_PATH + @"nashstock\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "ru";
        public const string CURRENCY = "RUB";
        public const string NAME = "nashstock";
        public const int MARZHA = DELIVERY_TO_USA + 1500; 
        public const int DELIVERY_TO_USA = 2000;
        public const double VAT_VALUE = 0; //Если 15% ват, то пишем 0.15, если вычета нет, то 0
        public NashStock nashstock = new NashStock(Config.GetConfig().NashStockFilename);

        public void Run()
        {
            Program.Logger.Info("Parsing NashStock " + NAME + ". Convert to Catalog and Stock CSV");
            Initialize();
            catalog = ParseNashStock(nashstock);
            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);
            var json = CreateJson();
            SaveJson(json, JSON_FILENAME, FOLDER, NAME);
        }

        public void Initialize()
        {
            //var appSettings = ConfigurationManager.AppSettings;
            //string ftpHost = appSettings["ftpHostParsing"];
            //string ftpUser = appSettings["ftpUserParsing"];
            //string ftpPass = appSettings["ftpPassParsing"];
            //Helper.GetFileFromFtp(JSON_FILENAME, FTP_FILENAME, ftpHost, ftpUser, ftpPass);
            //Json = DeserializeJson(JSON_FILENAME);
        }

        private Catalogs.Catalog ParseNashStock(NashStock nashstock)
        {
            var newCatalog = new Catalogs.Catalog();

            foreach (var item in nashstock.records)
            {
                if (item.brand.ToUpper() == "NIKE" || item.brand.ToUpper() == "JORDAN")
                {
                    if (item.condition == "New with box")
                    {
                        var sneaker = newCatalog.sneakers.Find(x => x.sku == item.sku);
                        if (sneaker == null)
                        {
                            sneaker = new Sneaker();
                            sneaker.sku = item.sku;
                            sneaker.title = item.title;
                            sneaker.brand = item.brand;
                            sneaker.category = item.category;
                            if (sneaker.category != Settings.CATEGORY_KIDS)
                            {
                                if (sneaker.category == Settings.CATEGORY_MEN)
                                {
                                    sneaker.sex = Settings.GENDER_MAN;
                                }
                                else if (sneaker.category == Settings.CATEGORY_WOMEN)
                                {
                                    sneaker.sex = Settings.GENDER_WOMAN;
                                }
                                else
                                {
                                    throw new Exception("wrong category");
                                }
                            }
                            sneaker.price = item.price;

                            if (item.quantity > 0)
                            {
                                //sizes
                                sneaker.AddSize(item.size, item.quantity, item.upc);

                                newCatalog.AddUniqueSneaker(sneaker);
                            }
                        }
                        else //если артикул уже есть в каталоге
                        {
                            //проверяем, есть ли уже этот размер у кроссовка
                            var size = sneaker.GetSize(item.size);
                            if (size == null)
                            {
                                sneaker.AddSize(item.size, item.quantity, item.upc);
                            }
                            else
                            {
                                Program.Logger.Warn("Дубликат размера в нашем стоке: sku:" + item.sku + " size:" + item.size);
                            }
                        }
                    }
                }
            }

            return newCatalog;
        }

        protected RootParsingObject CreateJson()
        {
            var market_info = new MarketInfo();
            var root = new RootParsingObject();
            var listings = new List<Listing>();

            market_info.name = NashStockParser.NAME;
            market_info.website = NashStockParser.SITEURL;
            market_info.currency = NashStockParser.CURRENCY;
            market_info.start_parse_date = DateTime.Now;
            market_info.end_parse_date = DateTime.Now;
            market_info.delivery_to_usa = NashStockParser.DELIVERY_TO_USA;
            market_info.photo_parameters.is_watermark_image = false;
            market_info.photo_parameters.background_color = "white";
            market_info.currently_language = "en";

            int i = 0;
            foreach (var sneaker in catalog.sneakers)
            {
                var listing = new Listing();

                //id murmurhash
                Encoding encoding = new UTF8Encoding();
                byte[] input = encoding.GetBytes(sneaker.title);
                using (MemoryStream stream = new MemoryStream(input))
                {
                    listing.id = MurMurHash3.Hash(stream);
                    if (listing.id < 0) listing.id = listing.id * -1;
                }

                listing.url = sneaker.link;
                listing.sku = sneaker.sku;
                listing.title = sneaker.title;
                listing.brand = sneaker.brand;
                listing.colorbrand = sneaker.color;
                listing.category = Helper.ConvertCategoryRusToEng(sneaker.category);
                if (String.IsNullOrWhiteSpace(listing.category)) Program.Logger.Warn("Wrong category: " + sneaker.category + " sku: " + sneaker.sku);
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
    }
}
