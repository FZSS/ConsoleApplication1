using SneakerIcon.Classes;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.SizeConverters.Model;
using SneakerIcon.Model.ShopCatalogModel;
using SneakerIcon.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Controller.ValitatorController
{
    public class ShopValidator : ValidatorAllBrands
    {
        //internal static string br = "\n";
        //public string log { get; set; }
        public const string _folder = "ValidatedShops";
        public static string _ftpFolder = "ValidateLogs";

        private SizeChart _sizeChart { get; set; }

        public ShopValidator()
        {
            _sizeChart = SizeChart.ReadSizeChartJson();
        }

        public static void ValidateAllShops()
        {
            //загружаем каталог магазинов
            var shopCatalog = ShopCatalogRoot.ReadFromFtp();

            foreach (var shop in shopCatalog.markets)
            {
                RootParsingObject market = SneakerIcon.Classes.Parsing.Model.RootParsingObject.ReadJsonMarketFromFtp(shop);
                if (market != null)
                {
                    var shopValidator = new ShopValidator();
                    shopValidator.ValidateShop(market, shopCatalog);                  
                }
                else
                {
                    bool test = true;
                }
            }
        }

        private static void SaveLogShopValidate(string log, string shopName)
        {
            var fileName = shopName + ".txt";
            var ftpPath = _ftpFolder + "/" + fileName;
            var path = _folder + @"\" + fileName;
            
            System.IO.File.WriteAllText(path, log);
            Helper.LoadFileToFtp(path, ftpPath, Config.GetConfig().FtpHostContabo, Config.GetConfig().FtpUserContabo, Config.GetConfig().FtpPassContabo);

            //throw new NotImplementedException();
        }

        private void ValidateShop(RootParsingObject shopParser, ShopCatalogRoot shopCatalog, string folder = _folder)
        {
            var validatedShop = new RootParsingObject();
            validatedShop.market_info = shopParser.market_info;
            //string br = "\n";
            log = DateTime.Now.ToString() + br;
            foreach (var listing in shopParser.listings)
            {
                string listingLog = String.Empty;
                var ValidListing = ValidateListing(listing);
                if (ValidListing != null)
                    validatedShop.listings.Add(ValidListing);
            }
            validatedShop.SaveLocalFile(folder);
            SaveLogShopValidate(this.log, validatedShop.market_info.name);
            //return validatedShop;
        }

        /// <summary>
        /// возвращает валидный листинг или null
        /// </summary>
        /// <param name="listing"></param>
        /// <returns>Возвращает листинг с валидными размерами и т.д.</returns>
        private Listing ValidateListing(Listing listing)
        {
            /* Основные проблемы сейчас это: невалидные sku, неверная категория (или пустая), неверные размеры (которые нельзя в категорию преобразовать)
             * Пустые тайтлы, или тайтлы в которых два раза указан бренд
             */

            var ValidListing = listing;
            log += "INFO: Validate" + "   Sku: " + listing.sku + "   Listing. Id: " + listing.id + "  Title: " + listing.title + br;
            log += "url: " + listing.url + br;
            //проверяем обязательные параметры, одиночные

            //brand
            bool isBrandValid = Validator.ValidateBrand(listing.brand);
            if (!isBrandValid)
            {
                log += "Error: Wrong Brand: " + listing.brand + br;
                return null;
            }

            //sku
            bool isValidSku = Validator.ValidateSku(listing.sku, listing.brand);
            if (!isBrandValid)
            {
                log += "Error: Wrong Sku: " + listing.sku + br;
                return null;
            }

            //title
            var logTitle = String.Empty;
            bool isValidTitle = Validator.ValidateTitle(listing.title, listing.brand, out logTitle);
            if (!isValidTitle)
                log += logTitle + br;
            

            //category
            /* Если категория уже есть, то всё ок. (на соответствие категории и размеров проверяем позже)
             * Если категории нет, то
             *  Смотрим, есть ли размеры в разных размерных сетках
             *      Если нет, 
             *          то оставляем листинг без категории, бросаем варнинг что категория не определена
             *          и проверку на размеры проходить не надо, 
             *          или проходить но всё равно добавлять эти листинги в валид json, иначе они не попадут в фулкаталог. 
             *          Или же в фулкаталог добавлять товары 
             *      Если да, 
             *          то пробуем определить категорию по размерным сеткам разным. 
             *          При чем нужно проверять не один, а все размеры. 
             *          И присваивать категорию только в том случае если все размеры (или больше половиные) подтверждают эту категорию. 
             *          Чтобы исключить ошибку, что первый размер например детский, а второй и следующий женский. 
             *          Вряд ли конечно такое будет, но всё же.
             */


            //sizes
            var sizes = new List<ListingSize>();
            foreach (var size in listing.sizes)
            {
                var logInvalidSize = Validator.ValidateSize(listing.brand,listing.category,size.us,size.eu,size.uk,size.cm, _sizeChart);
                if (logInvalidSize != null)
                    log += "WARN: " + logInvalidSize + br;
                    
                else
                    sizes.Add(size);
            }
            if (sizes.Count == 0)
            {
                log += "ERROR: Validated sizes not found" + br;
                return null;
            }
            ValidListing.sizes = sizes;




            return ValidListing;

            //throw new System.NotImplementedException();
        }
    }
}
