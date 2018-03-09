using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Controller.AllBrands;
using SneakerIcon.Model.ShopCatalogModel;
using SneakerIcon.Sys;

namespace SneakerIcon.Controller.ValitatorController
{
    public class ShopValidatorAllBrands : ShopValidator
    {
        private static string ValidateShopsLocalFolder = Config.GetConfig().ValidateShopsFolderAllBrands;
        private ShopCatalogRoot ShopCatalog { get; set; }
        public SizeChartAllBrands SizeChart { get; set; }

        public ShopValidatorAllBrands()
        {
            ShopCatalog = ShopCatalogRoot.ReadFromFtp();
            SizeChart = new SizeChartAllBrands();
            SizeChart.LoadSizeChartFromFtp();
        }

        public new void ValidateAllShops()
        {
            //загружаем каталог магазинов
            var shopCatalog = ShopCatalog;

            foreach (var shop in shopCatalog.markets)
            {
                RootParsingObject market = RootParsingObject.ReadAllBrandsShopFromFtp(shop);
                if (market != null)
                {
                    var shopValidator = new ShopValidator();
                    ValidateShop(market);
                }
                else
                {
                    bool test = true;
                }
            }
        }

        private void ValidateShop(RootParsingObject shopParser)
        {
            var shopCatalog = ShopCatalog;
            var folder = ValidateShopsLocalFolder;
            var validatedShop = new RootParsingObject();
            validatedShop.market_info = shopParser.market_info;
            //string br = "\n";
            log = DateTime.Now.ToString() + br;
            int i = 1;
            foreach (var listing in shopParser.listings)
            {
                string listingLog = String.Empty;
                var validListing = ValidateListing(listing);
                if (validListing != null)
                {
                    validListing.position = i;
                    i++;
                    validatedShop.listings.Add(validListing);
                }
                    
            }
            var localPath = folder + shopParser.market_info.name;
            validatedShop.SaveLocalFile(localPath);
            SaveLogShopValidate(this.log, validatedShop.market_info.name, localPath);
            //return validatedShop;
        }

        public void ValidateShop(string name)
        {
            var shop = ShopCatalog.markets.Find(x => x.name == name);
            RootParsingObject market = RootParsingObject.ReadAllBrandsShopFromFtp(shop);
            ValidateShop(market);
        }

        private static void SaveLogShopValidate(string log, string shopName, string folder)
        {
            var fileName = shopName + ".txt";
            var ftpPath = _ftpFolder + "/" + fileName;
            var path = folder + @"\" + fileName;

            System.IO.File.WriteAllText(path, log);
            //Helper.LoadFileToFtp(path, ftpPath, Config.GetConfig().FtpHostContabo, Config.GetConfig().FtpUserContabo, Config.GetConfig().FtpPassContabo);

            //throw new NotImplementedException();
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

            //var validListing = listing;
            log += br + "INFO: Validate" + "   Sku: " + listing.sku + "   Listing. Id: " + listing.id + br;
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
            if (!isValidSku)
            {
                log += "Error: Wrong Sku: " + listing.sku + br;
                return null;
            }

            //title
            var logTitle = String.Empty;
            listing.title = Validator.CorrectTitle(listing.title, listing.brand);
            bool isValidTitle = Validator.ValidateTitle(listing.title, listing.brand, out logTitle);
            if (!isValidTitle)
                log += logTitle + br;
            log += "Title: " + listing.title + br; 

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

            //проверяем правильность написания категории
            if (!string.IsNullOrWhiteSpace(listing.category))
            {
                listing.category = Validator.CorrectCategory(listing.category);
                bool isValidCategory = Validator.ValidateCategory(listing.category);
                if (!isValidCategory)
                {
                    log += "Error: Wrong WRITE category: " + listing.category + br;
                    return null;
                }
            }

            if (string.IsNullOrWhiteSpace(listing.category))
            {
                listing.category = DetectCategoryFromSize(listing, SizeChart);
                if (listing.category == null)
                {
                    log += "Error: Category did not detected: " + listing.category + br;
                    return null;
                }
            }

            //if (listing.category == "kids")
            //{
            //    log += "Info: category = kids" + br;
            //}

            //sizes
            //var sizes = new List<ListingSize>();
            //foreach (var size in listing.sizes)
            //{
            //    var logInvalidSize = Validator.ValidateSize(listing.brand, listing.category, size.us, size.eu, size.uk, size.cm, _sizeChart);
            //    if (logInvalidSize != null)
            //        log += "WARN: " + logInvalidSize + br;

            //    else
            //        sizes.Add(size);
            //}
            //if (sizes.Count == 0)
            //{
            //    log += "ERROR: Validated sizes not found" + br;
            //    return null;
            //}
            //ValidListing.sizes = sizes;




            return listing;

            //throw new System.NotImplementedException();
        }
    }
}
