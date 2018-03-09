using System.Configuration;

namespace SneakerIcon.Sys
{
    public class Config
    {
        private static Config _config;

        public string ImageFolger { get; }
        public string DirectoryPath { get; }
        public string DirectoryPathParsing { get; }
        public string DirectoryPathExport { get; }
        //todo нигде не используется, кроме себя самого в этом классе. В публичности нет смысла
        public string BasketshopFolder { get; }
        public string BasketshopCatalogFilename { get; }
        public string BasketshopStockFilename { get; }
        public string BasketshopImageFolger { get; }
        public string NashStockFilename { get; }
        public string DiscontStockSaleFilename { get; }
        public string DiscontStockFilename { get; }
        public string DiscontKuzminkyFilename { get; }
        //todo вообще нигде не используется
        public string DiscontSamaraFilename { get; }
        public int TimeoutImageLoad { get; }
        public int IdGroup { get; }
        public string Login { get; }
        public string FtpHostContabo { get; }
        public string FtpUserContabo { get; }
        public string FtpPassContabo { get; }
        public string FtpHostAllBrands { get; }
        public string FtpUserAllBrands { get; }
        public string FtpPassAllBrands { get; }
        public string AllBrandsFolder { get; }
        public string DbFolder { get; }
        public string DbFileName { get; }
        public string Pass { get; }
        public int AppId { get; }
        public string CapchaId { get; }
        public string FtpHostSneakerIcon { get; }
        public string FtpUserSneakerIcon { get; }
        public string FtpPassSneakerIcon { get; }
        public string FullCatalogFtpPath { get; }
        public string YdVkGoodsFile { get; set; }
        //public string VkGoodsFtpPath { get; }
        //public string VkGoodsFtpFile { get; }

        private static readonly object Obj = new object();

        public static Config GetConfig()
        {
            if (_config != null) return _config;

            lock (Obj)
            {
                _config = new Config();
            }

            return _config;
        }

        private Config()
        {
            var appSettings = ConfigurationManager.AppSettings;
            DirectoryPath = appSettings["directoryPath"];
            DirectoryPathParsing = DirectoryPath + @"Parsing\";
            DirectoryPathExport = DirectoryPath + @"Export\";
            BasketshopFolder = DirectoryPathParsing + @"basketshop.ru\";
            BasketshopCatalogFilename = BasketshopFolder + "Catalog.csv";
            BasketshopStockFilename = BasketshopFolder + "Stock.csv";
            ImageFolger = @"Images\";
            BasketshopImageFolger = BasketshopFolder + ImageFolger;
            NashStockFilename = DirectoryPathParsing + @"nashstock\NashStock.csv";
            DiscontStockSaleFilename = DirectoryPathParsing + @"discont\sale.csv";
            DiscontStockFilename = DirectoryPathParsing + @"discont\StockDiscont.csv";
            DiscontKuzminkyFilename = DirectoryPathParsing + @"discont_msk_kuzminki\StockDiscont.csv";
            DiscontSamaraFilename = DirectoryPathParsing + @"discont\Stock_On_Hand.csv";
            TimeoutImageLoad = 300;
            IdGroup = int.Parse(appSettings["idGroup"]);
            Login = appSettings["login"];
            Pass = appSettings["pass"];
            AppId = int.Parse(appSettings["appId"]);
            FtpHostContabo = @"80.241.220.50/";
            FtpUserContabo = @"ftp_user1";
            FtpPassContabo = @"ftp_user1!3827";
            CapchaId = appSettings["capchaId"];
            FtpHostAllBrands = @"s07.webhost1.ru/";
            FtpUserAllBrands = @"amaro_allbrands";
            FtpPassAllBrands = @"3IIW7eD8N5Tb";
            AllBrandsFolder = DirectoryPath + @"AllBrands\";
            ParsersFolderAllBrands = AllBrandsFolder + @"Parsing\";
            DbFolder = AllBrandsFolder + @"DB\";
            DbFileName = "DB.json";
            ValidateShopsFolderAllBrands = AllBrandsFolder + @"ValidateShops\";
            ServerImageUrl = "http://img.sneaker-icon.ru";
            FtpHostSneakerIcon = @"s07.webhost1.ru/";
            FtpUserSneakerIcon = @"amaro_snicon";
            FtpPassSneakerIcon = @"38273827";
            FullCatalogFtpPath = @"FullCatalog/FullCatalog.json";
            YdVkGoodsFile = DirectoryPath + @"Вконтакте\Товары\goods.json";
            MaxCountVkGoodsPosting = 5000;
        }

        public string ParsersFolderAllBrands { get; set; }

        public string ValidateShopsFolderAllBrands { get; private set; }
        public string ServerImageUrl { get; private set; }
        public int MaxCountVkGoodsPosting { get; private set; }
    }
}
