using CsvHelper;
using Newtonsoft.Json;
using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.SizeConverters.Model;
using SneakerIcon.Model.AllStock;
using SneakerIcon.Model.BonanzaModel;
using SneakerIcon.Model.FullCatalog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Model.ShopCatalogModel;

namespace SneakerIcon.Controller.Exporter
{
    public class BonanzaExporter2
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        protected static string _ftpHost = ConfigurationManager.AppSettings["ftpHostSneakerIcon"];
        protected static string _ftpUser = ConfigurationManager.AppSettings["ftpUserSneakerIcon"];
        protected static string _ftpPass = ConfigurationManager.AppSettings["ftpPassSneakerIcon"];
        public const int MARGIN_USD = 35;
        public const double PERCENT_MARGIN = 0.1;

        /// <summary>
        /// берем оллсток2, берем фулкаталог
        /// проходимся по оллстоку 2 и для каждого размера создает запись бонанзы
        /// формируем цену и другие данные
        /// в конце проходимся по всему каталогу и добавляем upc 
        /// </summary>
        public static void Run()
        {
            var allstock = AllStockExporter2.LoadLocalFile();
            var fullCatalog = FullCatalog2.LoadLocalFile();
            allstock.sneakers = allstock.sneakers.FindAll(x => x.category != "kids").ToList();
            fullCatalog.records = fullCatalog.records.FindAll(x => x.category != "kids").ToList();

            BonanzaRoot bonanza = CreateBonanzaRecords(allstock, fullCatalog);
            bonanza.update_date = DateTime.Now;
            //AddUPC(bonanza);

            SaveJson(bonanza);
            SaveCSV(bonanza);
        }

        public static void Run2_MultiListing()
        {
            var allstock = AllStockExporter2.LoadLocalFile();
            var fullCatalog = FullCatalog2.LoadLocalFile();

            //костыль. удаляем артикулы с пустыми размерами
            var count = allstock.sneakers.RemoveAll(x => x.sizes.Count == 0);
            //костыль. удаляем артикулы у которых все офферы пустые
            var count2 = allstock.sneakers.RemoveAll(x => x.sizes.Find(y => y.offers.Count > 0) == null);


            _logger.Info("Всего артикулов в фулкаталог: " + fullCatalog.records.Count);
            _logger.Info("Всего артикулов оллсток: " + allstock.sneakers.Count);
            DeleteRussianOffersFromAllStock(allstock);
            _logger.Info("Артикулов в оллсток после удаления предложений из русских магазинов: " + allstock.sneakers.Count);
            _logger.Info("Размеров в оллсток: " + allstock.GetCountSizes());

            var count3 = allstock.sneakers.RemoveAll(x => x.sizes.FindAll(y => y.offers.Count > 0) == null);

            BonanzaRoot bonanza = CreateBonanzaRecords_MultiListing(allstock, fullCatalog);
            _logger.Info("Создано записей бонанзы: " + bonanza.Records.Count);
            bonanza.update_date = DateTime.Now;
            //AddUPC(bonanza);

            SaveJson(bonanza);
            SaveCSV(bonanza);
        }

        private static void DeleteRussianOffersFromAllStock(AllStockRoot allstock)
        {
            //var newAllStock = new AllStockRoot();
            //newAllStock.update_time = allstock.update_time;

            //foreach (var asRecord in allstock.sneakers)
            //{
            //    //var offers = new List<AllStockOffer>();
            //    var NewRecord = new AllStockSneaker();
            //    foreach (var asSize in asRecord.sizes)
            //    {
            //        var offers = asSize.offers.FindAll(x => x.currency != "RUB");
            //        if (offers != null)
            //        {
            //            var newSize = new AllStockSize();
            //            newSize.sku2 = asSize.sku2;
            //            newSize.upc = asSize.upc;
            //            newSize.us = asSize.us;
            //            asSize.offers = offers;
            //            NewRecord.sizes.Add(asSize);
            //        }
            //    }
            //    if (NewRecord.sizes.Count > 0)
            //    {
            //        NewRecord.sku = asRecord.sku;
            //        NewRecord.title = asRecord.title;
            //        newAllStock.sneakers.Add(NewRecord);
            //    }
            //    else
            //    {
            //        bool test = true;
            //    }
            //}

            //Удаляем те артикулы где только русские офферы
            //дословно: берем все кроссовки у которых нашлись размеры, у которых нашлись офферы с валютой не рубль

            //var russianArtikuls = allstock.sneakers.FindAll(x => x.sizes.Find(y => y.offers.Find(z => z.currency != "RUB") == null) == null);
            //int countDeletedArtikuls = allstock.sneakers.RemoveAll(x => x.sizes.Find(y => y.offers.Find(z => z.currency != "RUB") == null) == null);

            //теперь убираем размеры без русских офферов
            int countDeletedSizes = 0;
            int countDeletedOffers = 0;
            foreach (var sneaker in allstock.sneakers)
            {
                var sizes = sneaker.sizes;
                //countDeletedSizes += sneaker.sizes.RemoveAll(y => y.offers.Find(z => z.currency != "RUB") == null);
                //теперь убираем у размеров русские офферы
                foreach (var size in sneaker.sizes)
                {                    
                    countDeletedOffers += size.offers.RemoveAll(z => z.currency == "RUB");
                }
                var deletedSizes = sneaker.sizes.FindAll(x => x.offers.Count == 0);
                countDeletedSizes += sneaker.sizes.RemoveAll(x => x.offers.Count == 0);
            }
            var deletedArtikuls = allstock.sneakers.FindAll(x => x.sizes.Count == 0);
            var countDeletedArtikuls = allstock.sneakers.RemoveAll(x => x.sizes.Count == 0);
        }

        private static void AddUPC(BonanzaRoot bonanza)
        {
            throw new NotImplementedException();
        }

        private static void SaveCSV(BonanzaRoot bonanza)
        {
            //string JSON_FILENAME = @"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\BonanzaExport.csv";
            var Records = bonanza.Records;
            var filename = Sys.Config.GetConfig().DirectoryPathExport + @"Bonanza2\Bonanza.csv";
            using (var sw = new StreamWriter(filename))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ",";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(Records);
            }

            filename = filename.Replace("Bonanza.csv", "BonanzaExcel.csv");
            using (var sw = new StreamWriter(filename))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(Records);
            }
        }

        private static void SaveJson(BonanzaRoot bonanza)
        {
            var filename = Sys.Config.GetConfig().DirectoryPathExport + @"Bonanza2\Bonanza.json";
            var text = JsonConvert.SerializeObject(bonanza);
            System.IO.File.WriteAllText(filename, text);
            //throw new NotImplementedException();
        }

        private static BonanzaRoot CreateBonanzaRecords(AllStockRoot allstock, FullCatalogRoot fullCatalog)
        {
            ShopCatalogRoot shopCatalog = ShopCatalogController.LoadShopCatalogFromFtp();
            BonanzaRoot bonanza = new BonanzaRoot();

            foreach (var asSneaker in allstock.sneakers)
            {
                foreach (var asSize in asSneaker.sizes)
                {
                    Model.BonanzaModel.BonanzaRecord bRecord = GetBonanzaRecord(asSize, asSneaker,fullCatalog,shopCatalog);
                    if (bRecord != null)
                        bonanza.Records.Add(bRecord);
                }
            }

            return bonanza;
        }

        private static BonanzaRoot CreateBonanzaRecords_MultiListing(AllStockRoot allstock, FullCatalogRoot fullCatalog)
        {
            BonanzaRoot bonanza = new BonanzaRoot();

            foreach (var asSneaker in allstock.sneakers)
            {
                Model.BonanzaModel.BonanzaRecord bRecord = GetBonanzaRecord_MultiListing(asSneaker, fullCatalog);
                if (bRecord != null)
                    bonanza.Records.Add(bRecord);
            }

            return bonanza;
        }

        private static Model.BonanzaModel.BonanzaRecord GetBonanzaRecord_MultiListing(AllStockSneaker asSneaker, FullCatalogRoot fullCatalog)
        {
            //валидации
                //sizes
            if (asSneaker.sizes.Count == 0)
            {
                return null;
            }
                //description
            var description = GetBonanzaRecordDescription_MultiListing(asSneaker, fullCatalog);            
            if (description == null)
            {
                _logger.Warn("Пустое описание. sku:" + asSneaker.sku);
                return null;
            }

            //инициализация
            var fcSneaker = fullCatalog.records.Find(x => x.sku == asSneaker.sku);
            var bRecord = new Model.BonanzaModel.BonanzaRecord();

            //standart data
            bRecord.quantity = 5; //нужно доработать, кол-во равно 5*кол-во размеров           
            bRecord.condition = "New with box";
            bRecord.force_update = "true";
            bRecord.brand = fcSneaker.brand;
            bRecord.MPN = fcSneaker.sku; //это вроде не работает
            bRecord.traits = "[[MPN:" + fcSneaker.sku + "]] ";
            
            //color  (русские цвета добавляются, пока убрал)
            //if (!string.IsNullOrWhiteSpace(fcSneaker.color))
            //    bRecord.traits += "[[Color:" + fcSneaker.color + "]] ";
            
            //shipping
            bRecord.shipping_price = 29;
            bRecord.shipping_type = "flat";
            bRecord.shipping_carrier = "usps";
            bRecord.shipping_service = "EconomyShipping";
            bRecord.shipping_package = "normal";
            //bRecord.worldwide_shipping_type = "flat";
            //bRecord.worldwide_shipping_price = 29;

            //main
            bRecord.id = fcSneaker.sku;
            bRecord.category = GetBonanzaCategory(asSneaker.category, fcSneaker.sex);
            bRecord.width = GetBonanzaWidth(asSneaker.category);
            bRecord.title = GetTitle_MultiListing(asSneaker.brand, asSneaker.sku, asSneaker.title);
            bRecord.description = description;

            //images
            bool hasImages = GetImages(fcSneaker, bRecord);
            if (!hasImages)
                return null;

            //делаем вариации
            //все предложения из русских магазов уже удалены
            bRecord.price = String.Empty;
            foreach (var size in asSneaker.sizes)
            {
                if (size.offers.Count > 0)
                {
                    var price = GetPrice(size.offers[0]);
                    bRecord.traits += "[[US Size:" + size.us + "][quantity:" + 5 + "][price:" + price + "]]";
                    if (bRecord.price == String.Empty)
                        bRecord.price = price;
                    if (Double.Parse(bRecord.price.Replace(".", ",")) > Double.Parse(price.Replace(".", ",")))
                    {
                        bRecord.price = price;
                    }
                        
                }               
            }
            if (bRecord.price == String.Empty)
            {
                _logger.Warn("Цена бонанза рекорд пустая. Sku: " + asSneaker.sku);
                var offer = asSneaker.sizes.Find(x => x.offers.Count > 0);
                return null;
            }
            




            //price
            //var offer = GetNonRussianOffer_MultiListing(asSneaker); //отфильтруем русские магазы
            //if (offer == null)
            //    return null;
            //bRecord.price = GetPrice(offer);

            //bRecord.size = asSize.us;
            //bRecord.traits = "[[US Size:" + asSize.us + "]] ";
            //bRecord.upc = asSize.upc;


            return bRecord;
        }

        private static bool GetImages(FullCatalogRecord fcSneaker, Model.BonanzaModel.BonanzaRecord bRecord)
        {
            if (fcSneaker.images.Count == 0)
            {
                _logger.Warn("Нет изображений. sku2:" + fcSneaker.sku);
                return false;
            }

            bRecord.image1 = fcSneaker.images[0];
            if (fcSneaker.images.Count > 1)
                bRecord.image2 = fcSneaker.images[1];
            if (fcSneaker.images.Count > 2)
                bRecord.image3 = fcSneaker.images[2];
            if (fcSneaker.images.Count > 3)
                bRecord.image4 = fcSneaker.images[3];
            return true;
        }

        private static Model.BonanzaModel.BonanzaRecord GetBonanzaRecord(AllStockSize asSize, AllStockSneaker asSneaker, FullCatalogRoot fullCatalog, ShopCatalogRoot shopCatalog)
        {
            var fcSneaker = fullCatalog.records.Find(x => x.sku == asSneaker.sku);
            if (fcSneaker == null)
            {
                _logger.Warn("Артикул не найден в фулкаталоге. sku:" + asSneaker.sku);
                return null;
            }
            var bRecord = new Model.BonanzaModel.BonanzaRecord();

            //standart data
            bRecord.quantity = 5;
            bRecord.condition = "New with box";
            bRecord.force_update = "true";
            bRecord.brand = fcSneaker.brand;
            //shipping
            bRecord.shipping_price = 29;
            bRecord.shipping_type = "flat";
            bRecord.shipping_carrier = "usps";
            bRecord.shipping_service = "EconomyShipping";
            bRecord.shipping_package = "normal";
            //bRecord.worldwide_shipping_type = "flat";
            //bRecord.worldwide_shipping_price = 29;

            bRecord.id = asSize.sku2;
            bRecord.description = GetBonanzaRecordDescription(asSize,asSneaker,fullCatalog);
            if (bRecord.description == null)
            {
                _logger.Warn("Пустое описание. sku2:" + bRecord.id);
                return null;
            }            
            
            bRecord.category = GetBonanzaCategory(asSneaker.category,fcSneaker.sex);
            bRecord.width = GetBonanzaWidth(asSneaker.category);
            bRecord.title = GetTitle(asSneaker.brand,asSize.us,asSneaker.sku,asSneaker.title);
            
            //price
            var offer = GetFirstActiveOffer(asSize, shopCatalog);
            //var offer = GetNonRussianOffer(asSize); //отфильтруем русские магазы
            if (offer == null)
                return null;
            bRecord.price = GetPrice(offer);
            //double priceWithMargin = offer.price_usd_with_delivery_to_usa_and_minus_vat + MARGIN_USD;
            //double priceWithFeeMinusBonanzaDelivery = (priceWithMargin * 1.18) - 29;
            //priceWithFeeMinusBonanzaDelivery = Math.Round(priceWithFeeMinusBonanzaDelivery, 2);
            //bRecord.price = priceWithFeeMinusBonanzaDelivery.ToString("F", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

            //images
            if (fcSneaker.images.Count == 0)
            {
                _logger.Warn("Нет изображений. sku2:" + bRecord.id);
                return null;
            }
            bRecord.image1 = fcSneaker.images[0];
            if (fcSneaker.images.Count > 1)
                bRecord.image2 = fcSneaker.images[1];
            if (fcSneaker.images.Count > 2)
                bRecord.image3 = fcSneaker.images[2];
            if (fcSneaker.images.Count > 3)
                bRecord.image4 = fcSneaker.images[3];

            
            bRecord.size = asSize.us;
            bRecord.traits = "[[US Size:" + asSize.us + "]] ";          
            bRecord.upc = asSize.upc;
            bRecord.MPN = fcSneaker.sku; //это вроде не работает
            bRecord.traits += "[[MPN:" + fcSneaker.sku + "]] ";
            

            return bRecord;
        }

        protected static string GetPrice(AllStockOffer offer) {
            double priceWithMargin = offer.price_usd_with_delivery_to_usa_and_minus_vat + MARGIN_USD;
            double priceWithFeeMinusBonanzaDelivery = (priceWithMargin * 1.18) - 29;
            priceWithFeeMinusBonanzaDelivery = Math.Round(priceWithFeeMinusBonanzaDelivery, 2);
            var price = priceWithFeeMinusBonanzaDelivery.ToString("F", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            return price;
        }

        protected static string GetPrice(double price_usd_with_delivery_to_usa_and_minus_vat)
        {
            double priceWithMargin = price_usd_with_delivery_to_usa_and_minus_vat + MARGIN_USD;
            var priceWithMarginPercent = price_usd_with_delivery_to_usa_and_minus_vat * (1 + PERCENT_MARGIN);

            if (priceWithMarginPercent > priceWithMargin)
                priceWithMargin = priceWithMarginPercent;

            double priceWithFeeMinusBonanzaDelivery = (priceWithMargin * 1.18) - 29;
            priceWithFeeMinusBonanzaDelivery = Math.Round(priceWithFeeMinusBonanzaDelivery, 2);
            var price = priceWithFeeMinusBonanzaDelivery.ToString("F", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            return price;
        }

        private static AllStockOffer GetNonRussianOffer(AllStockSize asSize)
        {
            if (asSize.offers[0].currency != "RUB")
            {
                return asSize.offers[0];
            }
            foreach (var offer in asSize.offers)
            {
                if (offer.currency != "RUB")
                {
                    return offer;
                }
            }
            return null;
        }

        private static AllStockOffer GetFirstActiveOffer(AllStockSize asSize, ShopCatalogRoot shopCatalog)
        {
            var bonanzaShops = shopCatalog.markets.FindAll(x => x.active_marketplace_list.Find(y => y == "bonanza") != null);
            //if (asSize.offers[0].currency != "RUB")
            //{
            //    return asSize.offers[0];
            //}
            foreach (var offer in asSize.offers)
            {
                //if (offer.currency != "RUB")
                if (bonanzaShops.Find(x => x.name == offer.stock_name) != null)
                {
                    return offer;
                }
            }
            return null;
        }

        private static string GetTitle_MultiListing(string brand, string sku, string title)
        {
            var resultTitle = String.Empty;
            string hvost = " " + sku;
            if (title.Count() + hvost.Count() < 80)
            {
                resultTitle = title.ToUpper() + hvost;
            }
            else
            {
                int indexSubstringTitle = 79 - hvost.Count() - 1;
                resultTitle = title.ToUpper().Substring(0, indexSubstringTitle) + hvost;
                int titlecount = title.Count();
            }
            return resultTitle;
        }

        protected static string GetTitle(string brand, string sizeUS, string sku, string title)
        {
            //title
            var resultTitle = String.Empty;
            string hvost = " SZ " + sizeUS + "US " + sku;
            if (title.Count() + hvost.Count() < 80)
            {
                resultTitle = title.ToUpper() + hvost;
            }
            else
            {
                int indexSubstringTitle = 79 - hvost.Count() - 1;
                resultTitle = title.ToUpper().Substring(0, indexSubstringTitle) + hvost;
                int titlecount = title.Count();
            }
            return resultTitle;
        }

        protected static string GetTitle(string brand, string sizeUS, string sku, string title, string category)
        {
            //title
            var resultTitle = String.Empty;
            string hvost = " SIZE " + sizeUS + "US (" + category.ToUpper() + "'S) " + sku;
            var count = title.Count() + hvost.Count();
            if (count < 80)
            {
                resultTitle = title.ToUpper() + hvost;
            }
            else
            {
                int indexSubstringTitle = 80 - hvost.Count();
                resultTitle = title.ToUpper().Substring(0, indexSubstringTitle) + hvost;
                int resultCount = resultTitle.Count();
            }
            return resultTitle;
        }

        protected static string GetBonanzaWidth(string category)
        {
            var width = string.Empty;
            switch (category)
            {
                case "men":
                    //resultCategory = "93427"; //Full ftpFolder: Fashion >> Men's Shoes
                    width = "Medium (D, M)";
                    break;
                case "women":
                    //resultCategory = "3034"; //Full ftpFolder: Fashion >> Women's Shoes
                    width = "Medium (B, M)";
                    break;
                case "kids":
                    width = "Medium";
                    break;
                default:
                    throw new Exception("Неверная категория");
                    //return "Medium (D, M)"; //по умолчанию мужские
                    break;
            }
            return width;
        }

        protected static string GetBonanzaCategory(string category, string sex)
        {
            var resultCategory = String.Empty;
            switch (category)
            {
                case "men":
                    resultCategory = "93427"; //Full ftpFolder: Fashion >> Men's Shoes
                    //this.width = "Medium (D, M)";
                    break;
                case "women":
                    resultCategory = "3034"; //Full ftpFolder: Fashion >> Women's Shoes
                    //this.width = "Medium (B, M)";
                    break;
                case "kids":
                    //this.width = "Medium";
                    if (sex == "men")
                    {
                        resultCategory = "57929"; //Fashion >> Kids' Clothing, Shoes & Accs >> Boys' Shoes
                    }
                    else if (sex == "women")
                    {
                        resultCategory = "57974"; //Fashion >> Kids' Clothing, Shoes & Accs >> Girls' Shoes
                    }
                    else //детская унисекс
                    {
                        resultCategory = "155202"; //Full ftpFolder: Fashion >> Kids' Clothing, Shoes & Accs >> Unisex Shoes
                    }
                    break;
                default:
                    throw new Exception("Неверная категория");
                    //resultCategory = "93427"; //по умолчанию тоже мужская обувь
                    break;
            }
            return resultCategory;
        }

        private static string GetBonanzaRecordDescription_MultiListing(AllStockSneaker asSneaker, FullCatalogRoot fullCatalog)
        {
            var boothUrl = System.Configuration.ConfigurationManager.AppSettings["bonanzaBoothUrl"];
            var title = asSneaker.title;
            var sku = asSneaker.sku;

            var stag = "<h3 style=\"text-align: center;\"><strong>";
            var etag = "</strong></h3>";

            string description =
                "<h3 style=\"text-align: center;\"><strong>" +
                title.ToUpper() +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "STYLE: " + sku +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "100% AUTHENTIC" +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "WORLDWIDE SHIPPING FOR 5-10 DAYS IN DOUBLE BOX WITH TRACKING NUMBER.<br>" +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "SHIP IN 2 BUSINESS DAY." +
                "</strong></h3>";

            description += "<h3 style=\"text-align: center;\"><strong>All sizes of this model available in stock:</strong></h3>";
            foreach (var asSneakersSize in asSneaker.sizes)
            {
                var schSize = SizeChart.GetSizeStatic(new Size(asSneaker.brand, asSneaker.category, asSneakersSize.us, null, null, null, null));
                if (schSize == null)
                {
                    _logger.Warn("Ошибка в размере. sku: " + asSneaker.sku + "\n"
                        + "Category: " + asSneaker.category + "\n"
                        + "Size US: " + asSneakersSize.us );
                    //return null;
                }
                else
                {
                    var sizeString = schSize.GetSizeStringUsEuUkCm();
                    description += stag + sizeString + etag;
                }
                
            }

            //if (sizeList != null)
            //{
            //    if (sizeList.Count > 0)
            //    {
            //        foreach (var size in sizeList)
            //        {
            //            //725076-301+6.5US
            //            description += "<h4 style=\"text-align: center;\"><strong><a href=\"" + boothUrl + "?utf8=%E2%9C%93&amp;item_sort_options%5Bfilter_string%5D=" + sku + "+" + size + "US&amp;item_sort_options%5Bfilter_category_id%5D=&amp;item_sort_options%5Bcustom_category_id%5D=&amp;commit=Go\" rel=\"nofollow\" target=\"_blank\">Size " + size + " US</a></strong></h4>";
            //        }
            //    }
            //}

            //доступные размеры в нашем магазине

            //description += "<h3 style=\"text-align: center;\"><strong>" +
            //    "Other sizes of this model available in out stock: " +
            //    "<a href=\"" + boothUrl + "?utf8=%E2%9C%93&amp;item_sort_options%5Bfilter_string%5D=" + sku + "&amp;item_sort_options%5Bfilter_category_id%5D=&amp;item_sort_options%5Bcustom_category_id%5D=&amp;commit=Go\" rel=\"nofollow\" target=\"_blank\">Link</a>" +
            //    "</strong></h3>";

            description += "<h3 style=\"text-align: center;\"><strong>" + "Please feel free to ask any questions and see <a href=\"" + boothUrl + "\" rel=\"nofollow\" target=\"_blank\">all our listings</a> for more great deals." + "</strong></h3>";

            //throw new NotImplementedException();
            return description;
        }

        private static string GetBonanzaRecordDescription(AllStockSize asSize, AllStockSneaker asSneaker, FullCatalogRoot fullCatalog)
        {
            var boothUrl = System.Configuration.ConfigurationManager.AppSettings["bonanzaBoothUrl"];
            var title = asSneaker.title;
            var sku = asSneaker.sku;
            var size = SizeChart.GetSizeStatic(new Size(asSneaker.brand, asSneaker.category, asSize.us, null, null, null, null));
            if (size == null)
            {
                _logger.Warn("Ошибка в размере. sku: " + asSneaker.sku);
                return null;
            }
                
            var sizeString = size.GetSizeStringUsEuUkCm();

            string description =
                "<h3 style=\"text-align: center;\"><strong>" +
                title.ToUpper() +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "STYLE: " + sku +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "SIZE: " + sizeString +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "100% AUTHENTIC" +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "WORLDWIDE SHIPPING FOR 5-10 DAYS IN DOUBLE BOX WITH TRACKING NUMBER.<br>" +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "SHIP IN 2 BUSINESS DAY." +
                "</strong></h3>";

            //description += "<h3 style=\"text-align: center;\"><strong>All sizes of this model available in stock</strong></h3>";

            //if (sizeList != null)
            //{
            //    if (sizeList.Count > 0)
            //    {
            //        foreach (var size in sizeList)
            //        {
            //            //725076-301+6.5US
            //            description += "<h4 style=\"text-align: center;\"><strong><a href=\"" + boothUrl + "?utf8=%E2%9C%93&amp;item_sort_options%5Bfilter_string%5D=" + sku + "+" + size + "US&amp;item_sort_options%5Bfilter_category_id%5D=&amp;item_sort_options%5Bcustom_category_id%5D=&amp;commit=Go\" rel=\"nofollow\" target=\"_blank\">Size " + size + " US</a></strong></h4>";
            //        }
            //    }
            //}

            description += "<h3 style=\"text-align: center;\"><strong>" +
                "Other sizes of this model available in out stock: " +
                "<a href=\"" + boothUrl + "?utf8=%E2%9C%93&amp;item_sort_options%5Bfilter_string%5D=" + sku + "&amp;item_sort_options%5Bfilter_category_id%5D=&amp;item_sort_options%5Bcustom_category_id%5D=&amp;commit=Go\" rel=\"nofollow\" target=\"_blank\">Link</a>" +
                "</strong></h3>";

            description += "<h3 style=\"text-align: center;\"><strong>" + "Please feel free to ask any questions and see <a href=\"" + boothUrl + "\" rel=\"nofollow\" target=\"_blank\">all our listings</a> for more great deals." + "</strong></h3>";

            //throw new NotImplementedException();
            return description;
        }
    }
}
