using SneakerIcon.Classes.Parsing.Discont;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.Stocks;
using SneakerIcon.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Parsing
{
    public class DiscontMskNovoslobParser : DiscontParser
    {
        public const string NAME = "discont_msk_novoslob";
        public const string SITEURL = "";
        public static readonly string FOLDER = DIRECTORY_PATH + NAME + @"\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "ru";
        public const string CURRENCY = "RUB";
        public const int MARZHA = DELIVERY_TO_USA + 1500;
        public const int DELIVERY_TO_USA = 2000;
        public const double VAT_VALUE = 0; //Если 15% ват, то пишем 0.15, если вычета нет, то 0

        public void Run()
        {
            DiscontParsingCSV(NAME);

            Program.Logger.Info("Parsing NashStock " + NAME + ". Convert to Catalog and Stock CSV");
            var discont = new DiscontStock();
            var path = Config.GetConfig().DirectoryPathParsing + NAME + @"\StockDiscont2.csv";
            discont.ReadStockFromCSV2(path);
            //discont.AddSaleToRecords2(); подгружаются почему-то от самарского дисконта скидки
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

        public static void DiscontParsingCSV(string NAME)
        {
            var dStock = new DiscontStock();
            var path = Config.GetConfig().DirectoryPathParsing + NAME + @"\";
            //dStock.ParseStockOnHand(path + "Stock On Hand.csv", ',');
            //dStock.SaveStockToCSV(path + "StockDiscont.csv");
            dStock.ParseStockOnHand2(path + "Stock On Hand.csv");
            dStock.SaveStockToCSV2(path + "StockDiscont2.csv");
        }

        public static RootParsingObject LoadLocalFileJson()
        {
            return LoadLocalFileJson(JSON_FILENAME, FOLDER);
        }


    }
}
