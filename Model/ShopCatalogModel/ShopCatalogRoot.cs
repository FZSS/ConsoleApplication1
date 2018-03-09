using Newtonsoft.Json;
using SneakerIcon.Classes;
using System.Collections.Generic;
using System.IO;

namespace SneakerIcon.Model.ShopCatalogModel
{
    public class ShopCatalogRoot
    {
        public List<Shop> markets { get; set; }

        public ShopCatalogRoot()
        {
            markets = new List<Shop>();
        }

        public static ShopCatalogRoot ReadFromFtp()
        {
            Program.Logger.Info("Load MarketCatalog from FTP");
            var json = new ShopCatalogRoot();

            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostSneakerIcon"];
            string ftpUser = appSettings["ftpUserSneakerIcon"];
            string ftpPass = appSettings["ftpPassSneakerIcon"];

            var Folder = "JsonCatalog/";
            var Filename = "jsonCatalog.json";

            //Helper.GetFileFromFtp(Filename, Folder+Filename, ftpHost, ftpUser, ftpPass);
            var textJson = Helper.GetFileFromFtpIntoString(Filename, Folder + Filename, ftpHost, ftpUser, ftpPass);
            var shopCatalog = JsonConvert.DeserializeObject<ShopCatalogRoot>(textJson);
            Program.Logger.Info("MarketCatalog downloaded. Items Count:" + shopCatalog.markets.Count);
            return shopCatalog;
        }

        public void SaveToFtp()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostSneakerIcon"];
            string ftpUser = appSettings["ftpUserSneakerIcon"];
            string ftpPass = appSettings["ftpPassSneakerIcon"];

            var Folder = "JsonCatalog/";
            var Filename = "jsonCatalog.json";
            var Path = Folder + Filename;

            var text = JsonConvert.SerializeObject(this);
            File.WriteAllText(Filename,text);
            Helper.LoadFileToFtp(Filename,Path,ftpHost,ftpUser,ftpPass);
            File.Delete(Filename);
        }
    }
}
