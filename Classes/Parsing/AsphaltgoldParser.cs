using SneakerIcon.Classes.Parsing.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Parsing
{
    public class AsphaltgoldParser : Parser
    {
        public const string NAME = "asphaltgold.de";
        public const string FTP_FILENAME = NAME + "/" + JSON_FILENAME;
        public static readonly string FOLDER = DIRECTORY_PATH + NAME + @"\";
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        public const string CURRENCY = "EUR";
        public const int MARZHA = DELIVERY_TO_USA + 25;
        public const int DELIVERY_TO_USA = 30;
        public const double VAT_VALUE = 0.16; //Если 15% ват, то пишем 0.15, если вычета нет, то 0
        public RootParsingObject Json { get; set; }


        public void Run()
        {
            Program.Logger.Info("Parsing JSON " + NAME + ". Convert to Catalog and Stock CSV");
            Initialize();
            CorrectSKU();
            catalog = ParseCatalogFromJson(Json);
            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);
        }

        public void Initialize()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostParsing"];
            string ftpUser = appSettings["ftpUserParsing"];
            string ftpPass = appSettings["ftpPassParsing"];
            Helper.GetFileFromFtp(FOLDER + JSON_FILENAME, FTP_FILENAME, ftpHost, ftpUser, ftpPass);
            Json = DeserializeJson(FOLDER + JSON_FILENAME);
        }

        private void CorrectSKU()
        {
            foreach (var item in Json.listings)
            {
                item.sku = item.sku.Replace(" ", "-");
            }
            //throw new NotImplementedException();
        }

    }
}
