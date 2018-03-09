using Newtonsoft.Json;
using SneakerIcon.Model.ShopCatalogModel;
using System.Collections.Generic;
using System.IO;
using System.Net;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing.Model
{
    public class RootParsingObject
    {
        public MarketInfo market_info { get; set; }
        public List<Listing> listings { get; set; }

        public RootParsingObject()
        {
            listings = new List<Listing>();
        }

        public static RootParsingObject ReadJsonMarketFromFtp(Shop jsonMarket)
        {
            Program.Logger.Info("Load Market form FTP. Market Name: " + jsonMarket.name);
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostParsing"];
            string ftpUser = appSettings["ftpUserParsing"];
            string ftpPass = appSettings["ftpPassParsing"];

            var Folder = jsonMarket.name + "/";
            var Filename = jsonMarket.name + "_" + jsonMarket.language + ".json";

            try
            {
                var textJson = Helper.GetFileFromFtpIntoString(Filename, Folder + Filename, ftpHost, ftpUser, ftpPass);
                var market = JsonConvert.DeserializeObject<RootParsingObject>(textJson);
                Program.Logger.Info("Market downloaded. Items count:" + market.listings.Count);
                return market;
            }
            catch (WebException e)
            {
                Program.Logger.Warn("Json file is not exist. FileName:" + Folder + Filename);
                return null;
            }
        }

        public static RootParsingObject ReadValidShop(Shop jsonMarket)
        {           
            return ReadValidShop(jsonMarket.name);
        }

        public static RootParsingObject ReadValidShop(string name)
        {
            var path = Config.GetConfig().ValidateShopsFolderAllBrands + name + @"\" + name + ".json";
            return ReadLocalShop(path);
        }

        public static RootParsingObject ReadLocalShop(string path)
        {
            if (path.Contains("sneakersnstuff"))
            {
                bool test = true;
            }
            if (!File.Exists(path))
                return null;
            var text = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<RootParsingObject>(text);
        }

        public static RootParsingObject ReadAllBrandsShopFromFtp(Shop jsonMarket)
        {
            var ftpHost = Config.GetConfig().FtpHostAllBrands;
            var ftpUser = Config.GetConfig().FtpUserAllBrands;
            var ftpPass = Config.GetConfig().FtpPassAllBrands;
            return ReadShopFromFtp(jsonMarket, ftpHost, ftpUser, ftpPass);
        }

        public static RootParsingObject ReadShopFromFtp(Shop jsonMarket,string ftpHost, string ftpUser, string ftpPass)
        {
            Program.Logger.Info("Load Market form FTP. Market Name: " + jsonMarket.name);
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            //string ftpHost = appSettings["ftpHostParsing"];
            //string ftpUser = appSettings["ftpUserParsing"];
            //string ftpPass = appSettings["ftpPassParsing"];

            var Folder = jsonMarket.name + "/";
            var Filename = jsonMarket.name + "_" + jsonMarket.language + ".json";

            try
            {
                var textJson = Helper.GetFileFromFtpIntoString(Filename, Folder + Filename, ftpHost, ftpUser, ftpPass);
                var market = JsonConvert.DeserializeObject<RootParsingObject>(textJson);
                Program.Logger.Info("Market downloaded. Items count:" + market.listings.Count);
                return market;
            }
            catch (WebException e)
            {
                Program.Logger.Warn("Json file is not exist. FileName:" + Folder + Filename);
                return null;
            }
        }

        internal void SaveLocalFile(string folder)
        {
            //проверяем есть ли папка куда сохранять. Если нет, создаем её
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var path = folder + @"\" + this.market_info.name + ".json";
            var text = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(path, text);

            //throw new System.NotImplementedException();
        }
    }
}
