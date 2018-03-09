using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Parsing
{
    public class SolehavenParser : Parser
    {
        //public const string JSON_FILENAME = @"soleheaven.com\soleheaven.com_en.markets"; 
        public const string FTP_FILENAME = "soleheaven.com/soleheaven.com_en.json";
        //public const string JSON_FILENAME = "soleheaven.com_en.markets";
        public static readonly string FOLDER = DIRECTORY_PATH + @"solehaven\";
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        public const string CURRENCY = "USD";
        public const string NAME = "solehaven.com";
        public const int MARZHA = DELIVERY_TO_USA + 25;
        public const int DELIVERY_TO_USA = 0;
        public const double VAT_VALUE = 0; //Если 15% ват, то пишем 0.15, если вычета нет, то 0
        public RootParsingObject Json { get; set; }


        public void Run()
        {
            Program.Logger.Info("Parsing JSON " + NAME + ". Convert to Catalog and Stock CSV");
            Initialize();
            catalog = ParseCatalogFromJson(Json);
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
            Json = DeserializeJson(FOLDER + JSON_FILENAME);
        }
    }
}
