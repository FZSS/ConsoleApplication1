using SneakerIcon.Classes;
using SneakerIcon.Model.ShopCatalogModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Parsing.Model;

namespace SneakerIcon.Controller
{
    public class ShopCatalogController
    {
        public const string ftpFolder = "JsonCatalog";
        public const string ftpFileName = "jsonCatalog.json";
        private static string _ftpHost = ConfigurationManager.AppSettings["ftpHostSneakerIcon"];
        private static string _ftpUser = ConfigurationManager.AppSettings["ftpUserSneakerIcon"];
        private static string _ftpPass = ConfigurationManager.AppSettings["ftpPassSneakerIcon"];

        public static ShopCatalogRoot LoadShopCatalogFromFtp() {
            var shopCatalog = Model.ShopCatalogModel.ShopCatalogRoot.ReadFromFtp();
            return shopCatalog;
        }

        public static List<RootParsingObject> GetShops()
        {
            var shopCatalog = LoadShopCatalogFromFtp();
            var shops = new List<RootParsingObject>();
            foreach (var shopCatalogMarket in shopCatalog.markets)
            {
                var shop = Classes.Parsing.Model.RootParsingObject.ReadAllBrandsShopFromFtp(shopCatalogMarket);
                if (shop != null) //если нул значит магазин еще не добавлен
                {
                    shops.Add(shop);
                }
            }
            return shops;
        }

        public static List<RootParsingObject> GetShopsValidated()
        {
            var shopCatalog = LoadShopCatalogFromFtp();
            var shops = new List<RootParsingObject>();
            foreach (var shopCatalogMarket in shopCatalog.markets)
            {
                var shop = Classes.Parsing.Model.RootParsingObject.ReadValidShop(shopCatalogMarket);
                if (shop != null) //если нул значит магазин еще не добавлен
                {
                    shops.Add(shop);
                }
            }
            return shops;
        }


    }
}
