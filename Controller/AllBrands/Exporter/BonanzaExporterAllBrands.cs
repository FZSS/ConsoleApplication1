using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using SneakerIcon.Controller.AllBrands.Db;
using SneakerIcon.Controller.Exporter;
using SneakerIcon.Model.AllBrands.DB;
using SneakerIcon.Model.BonanzaModel;
using System.IO;
using CsvHelper;
using SneakerIcon.Classes;
using SneakerIcon.Classes.SizeConverters.Model;

namespace SneakerIcon.Controller.AllBrands.Exporter
{
    public class BonanzaExporterAllBrands : BonanzaExporter2
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private DbRoot Db { get; set; }
        private static string Folder = "BonanzaAllBrands";

        public BonanzaExporterAllBrands()
        {
            Db = DbController.LoadLocalFile();
            Validator.Validate(Db);          
        }

        public new void Run()
        {
            BonanzaRoot bonanza = CreateBonanzaRecords();
            bonanza.update_date = DateTime.Now;

            //отрежем детские
            bonanza.Records = bonanza.Records.FindAll(x => x.category != "kids");

            //другие бренды
            var noNikeBonanzaRoot = new BonanzaRoot();
            noNikeBonanzaRoot.update_date = DateTime.Now;
            noNikeBonanzaRoot.Records = bonanza.Records.FindAll(x => x.brand.ToLower() != "nike");
            noNikeBonanzaRoot.Records = noNikeBonanzaRoot.Records.FindAll(x => x.brand.ToLower() != "jordan");
            SaveJson(noNikeBonanzaRoot);
            SaveCsv(noNikeBonanzaRoot);
        }

        private BonanzaRoot CreateBonanzaRecords()
        {
            BonanzaRoot bonanza = new BonanzaRoot();

            foreach (var dbSneaker in Db.Sneakers)
            {
                foreach (var dbSize in dbSneaker.Sizes)
                {
                    Model.BonanzaModel.BonanzaRecord bRecord = GetBonanzaRecord(dbSneaker, dbSize);
                    if (bRecord != null)
                        bonanza.Records.Add(bRecord);
                }
            }

            return bonanza;
        }

        private BonanzaRecord GetBonanzaRecord(DbSneaker dbSneaker, DbSize dbSize)
        {
            var bRecord = new Model.BonanzaModel.BonanzaRecord();

            //standart data
            bRecord.quantity = 5;
            bRecord.condition = "New with box";
            bRecord.force_update = "true";
            bRecord.brand = dbSneaker.Brand;
            //shipping
            bRecord.shipping_price = 29;
            bRecord.shipping_type = "flat";
            bRecord.shipping_carrier = "usps";
            bRecord.shipping_service = "EconomyShipping";
            bRecord.shipping_package = "normal";
            //bRecord.worldwide_shipping_type = "flat";
            //bRecord.worldwide_shipping_price = 29;

            bRecord.id = dbSneaker.Sku + "-" + dbSize.Us;
            bRecord.description = GetBonanzaRecordDescription(dbSize, dbSneaker);
            if (bRecord.description == null)
            {
                _logger.Warn("Пустое описание. sku2:" + bRecord.id);
                return null;
            }

            //todo поправить метод, так как пола теперь нет, только категория
            bRecord.category = GetBonanzaCategory(dbSneaker.Category, dbSneaker.Category);
            bRecord.width = GetBonanzaWidth(dbSneaker.Category);
            bRecord.title = GetTitle(dbSneaker.Brand, dbSize.Us, dbSneaker.Sku, dbSneaker.Titles[0], dbSneaker.Category);

            //price
            var offer = GetNonRussianOffer(dbSize); //отфильтруем русские магазы и берем самый дешевый оффер
            if (offer == null)
                return null;
            bRecord.price = GetPrice(offer.price_usd_with_delivery_to_usa_and_minus_vat);

            //images
            List<string> images = GetImages(dbSneaker);
            if (images == null)
            {
                _logger.Warn("Нет изображений. sku2:" + bRecord.id);
                return null;
            }
            if (images.Count == 0)
            {
                _logger.Warn("Нет изображений. sku2:" + bRecord.id);
                return null;
            }
            bRecord.image1 = images[0];
            if (images.Count > 1)
                bRecord.image2 = images[1];
            if (images.Count > 2)
                bRecord.image3 = images[2];
            if (images.Count > 3)
                bRecord.image4 = images[3];


            bRecord.size = dbSize.Us;
            bRecord.traits = "[[US Size:" + dbSize.Us + "]] ";
            bRecord.upc = dbSize.Upc;
            bRecord.MPN = dbSneaker.Sku; //это вроде не работает
            bRecord.traits += "[[MPN:" + dbSneaker.Sku + "]] ";

            return bRecord;
        }

        private List<string> GetImages(DbSneaker dbSneaker)
        {
            if (dbSneaker.ImageCollectionList.Count == 0)
            {
                return null;
            }
            var images = new List<string>();
            var imgList = dbSneaker.ImageCollectionList[0].Images;
            foreach (var image in imgList)
            {
                images.Add(image.OurUrl);
            }
            return images;
            //todo добавить сортировку изображений, чтобы показывать в первую очередь те фотки, которые лучше всего подходят под магазин (сначала из первого эшелона, потом из других магазинов и так далее
        }

        private static DbOffer GetNonRussianOffer(DbSize dbSize)
        {
            var offers = dbSize.Offers.FindAll(x => x.currency != "RUB");
            if (offers.Count == 0)
                return null;
            offers = offers.OrderBy(x => x.price_usd_with_delivery_to_usa_and_minus_vat).ToList();

            if (dbSize.Offers[0].currency != "RUB")
            {
                return dbSize.Offers[0];
            }
            foreach (var offer in dbSize.Offers)
            {
                if (offer.currency != "RUB")
                {
                    return offer;
                }
            }
            return null;
        }

        private string GetBonanzaRecordDescription(DbSize dbSize, DbSneaker dbSneaker)
        {
            var boothUrl = System.Configuration.ConfigurationManager.AppSettings["bonanzaBoothUrlAllBrands"];
            var title = dbSneaker.Titles[0];
            var sku = dbSneaker.Sku;
            var size = dbSize.Us;
            if (string.IsNullOrWhiteSpace(size))
            {
                _logger.Warn("Ошибка в размере. Размер пустой. sku: " + sku);
                return null;
            }

            var sizeString = size + " US (" + dbSneaker.Category.ToUpper() + "'S)";

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

            description += "<h3 style=\"text-align: center;\"><strong>" +
                           "Other sizes of this model available in out stock: " +
                           "<a href=\"" + boothUrl + "?utf8=%E2%9C%93&amp;item_sort_options%5Bfilter_string%5D=" + sku + "&amp;item_sort_options%5Bfilter_category_id%5D=&amp;item_sort_options%5Bcustom_category_id%5D=&amp;commit=Go\" rel=\"nofollow\" target=\"_blank\">Link</a>" +
                           "</strong></h3>";

            description += "<h3 style=\"text-align: center;\"><strong>" + "Please feel free to ask any questions and see <a href=\"" + boothUrl + "\" rel=\"nofollow\" target=\"_blank\">all our listings</a> for more great deals." + "</strong></h3>";

            //throw new NotImplementedException();
            return description;
        }

        private static void SaveJson(BonanzaRoot bonanza)
        {
            var filename = Sys.Config.GetConfig().DirectoryPathExport + Folder + @"\Bonanza.json";
            var text = JsonConvert.SerializeObject(bonanza);
            System.IO.File.WriteAllText(filename, text);
        }

        private static void SaveCsv(BonanzaRoot bonanza)
        {
            //string JSON_FILENAME = @"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\BonanzaExport.csv";
            var Records = bonanza.Records;
            var filename = Sys.Config.GetConfig().DirectoryPathExport + Folder + @"\Bonanza.csv";
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
    }
}
