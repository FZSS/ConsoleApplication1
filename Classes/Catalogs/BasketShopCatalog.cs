using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Catalogs
{
    public class BasketShopCatalog : Catalog
    {
        public BasketShopCatalog() 
        {
            Name = "BasketShopCatalog";
            FileNameCatalog = Config.GetConfig().DirectoryPathParsing + @"basketshop.ru\Catalog.csv";
            ReadCatalogFromCSV(FileNameCatalog);
        }
    }
}
