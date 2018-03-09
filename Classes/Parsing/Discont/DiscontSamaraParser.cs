using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.Stocks;
using System;
using SneakerIcon.Classes.Parsing.Discont;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing
{
    public class DiscontSamaraParser : DiscontParser
    {
        public const string SITEURL = "";
        public static readonly string FOLDER = DIRECTORY_PATH + @"discont\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "ru";
        public const string CURRENCY = "RUB";
        public const string NAME = "discont-samara";
        public const int MARZHA = DELIVERY_TO_USA + 1500;
        public const int DELIVERY_TO_USA = 2000;
        public const double VAT_VALUE = 0; //Если 15% ват, то пишем 0.15, если вычета нет, то 0


        public void Run()
        {
            Program.Logger.Info("Parsing NashStock " + NAME + ". Convert to Catalog and Stock CSV");
            var discont = new DiscontStock();
            discont.ReadStockFromCSV2(Config.GetConfig().DirectoryPathParsing + @"discont\StockDiscont2.csv");
            discont.AddSaleToRecords2();
            catalog = ParseStock(discont);
            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);

            var market_info = new MarketInfo();
            market_info.name = NAME;
            market_info.website = SITEURL;
            market_info.currency = CURRENCY;
            market_info.start_parse_date = DateTime.Now;
            market_info.end_parse_date = DateTime.Now;
            market_info.delivery_to_usa = DELIVERY_TO_USA;
            market_info.photo_parameters.is_watermark_image = false;
            market_info.photo_parameters.background_color = "white";
            market_info.photo_parameters.first_photo_sneaker_direction = "left";
            market_info.currently_language = "en";

            var json = CreateJson(market_info);
            SaveJson(json, JSON_FILENAME, FOLDER, NAME);
        }

        public static RootParsingObject LoadLocalFileJson()
        {
            return LoadLocalFileJson(JSON_FILENAME, FOLDER);
        }
    }
}
