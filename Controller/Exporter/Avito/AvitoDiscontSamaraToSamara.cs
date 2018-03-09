using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.Exporters2;
using SneakerIcon.Classes.Parsing;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Model.AvitoModel;
using SneakerIcon.Model.FullCatalog;
using SneakerIcon.Classes.SizeConverters.Model;
using SneakerIcon.Classes.Utils;

namespace SneakerIcon.Controller.Exporter.Avito
{
    public class AvitoDiscontSamaraToSamara : AvitoDiscontExporter
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public new const string FolderName = "AvitoDiscontSamaraToSamara";
        public static int StartHour = 9;
        public static int EndHour = 19;
        public static int DaysPeriod = 30;


        /// <summary>
        /// беру файл оллстока и создаю xml файл для авито
        /// </summary>
        public static new void Run()
        {
            DateTime startTime = new DateTime(2017, 05, 08, 00, 00, 00, DateTimeKind.Local); // 8 мая в 7 утра
            //var allstock = AllStockExporter2.LoadLocalFile();
            var discontSamara = DiscontSamaraParser.LoadLocalFileJson();
            var fullCatalog = FullCatalog2.LoadLocalFile();
            AvitoAds avito = createAds(discontSamara, fullCatalog, startTime);
            //todo отсортировать артикулы по возрастанию или убыванию артикулов. чтобы всегда был единый порядок


            SaveToJson(avito, FolderName);
            //AvitoAds avito2 = LoadFromJson();
            SaveToXml(avito, FolderName);
            ReplaceChecialCharacterInXml(FolderName);
            //UploadXmlToFtp(FolderName,"Avito");
        }

        private static object LoadLocalDiscontSamaraFile()
        {
            throw new NotImplementedException();
        }

        private static AvitoAds createAds(RootParsingObject discontSamara, FullCatalogRoot fullCatalog,
            DateTime startTime)
        {
            var Ads = new AvitoAds();
            Ads.StartTime = startTime;

            var menWomenOnlyListings = discontSamara.listings.FindAll(x => x.category != "kids");
            //var listingsMoreThan2SizesAvailable = menWomenOnlyListings.FindAll(x => x.sizes.Count > 2);
            var listings = menWomenOnlyListings;
            listings = listings.OrderBy(x => x.sku).ToList();
            var listingCount = listings.Count;            
            for (int i = 0; i < listingCount; i++)
            {
                var sneaker = listings[i];
                var fcSneaker = fullCatalog.records.Find(x => x.sku == sneaker.sku);
                AvitoAd Ad = GetAdFromSneaker(sneaker, fcSneaker, Ads.StartTime, Ads.AdList.Count + 1, listingCount);
                if (Ad != null)
                    Ads.AdList.Add(Ad);
            }
            _logger.Info("Кол-во листингов: " + listingCount);
            _logger.Info("Кол-во объявлений: " + Ads.AdList.Count);
            return Ads;
        }



        private static AvitoAd GetAdFromSneaker(Listing sneaker, FullCatalogRecord fcSneaker, DateTime startTime,
            int position, int count)
        {
            AvitoAd Ad = new AvitoAd();
            _logger.Info("sku: " + sneaker.sku + "  title: " + fcSneaker.title);

            if (String.IsNullOrWhiteSpace(sneaker.category))
            {
                _logger.Warn("Skip sneaker. Reason: category is empty");
                return null;
            }
            else if (sneaker.category == "kids")
            {
                _logger.Info("skip sneaker. Reason: kids category");
                return null;
            }

            if (sneaker.sizes == null)
            {
                _logger.Warn("Skip sneaker. Reason: sizes is empty");
                return null;
            }

            if (sneaker.sizes.Count == 0)
            {
                _logger.Warn("Skip sneaker. Reason: category is empty. Sku:" + sneaker.sku);
                return null;
            }

            if (fcSneaker.images == null)
            {
                _logger.Warn("Skip sneaker. Reason: images is empty. Sku: " + sneaker.sku);
                return null;
            }
            else if (fcSneaker.images.Count == 0)
            {
                _logger.Warn("Skip sneaker. Reason: images is empty. Sku: " + sneaker.sku);
                return null;
            }           

            Ad.Id = sneaker.sku;

            //DateBegin
            /* нужно все объявления размазать на месяц (ну пусть будет на 30 дней). 
             * допустим если у меня есть 3000 объявлений то это значит что нужно постить по 100 в день
             * Постить будем допустим с 7.00 утра 23.59. Получается 17 часов. это 61200 секунд. 
             * Получается объявление нужно постить каждые 612 секунд, начиная с 7 утра
             * 
             * В сутках 24 часа, это 86400 секунд.
             * В 30 днях 2 592 000 секунд
             * То есть если у нас есть 3000 объявлений, значит начиная с текущего момента нужно постить объявление каждые 864 секунды
             */
            double delay = 2592000 / count * position; //todo сделать распределение на 10 часов а не 24
            var intDelay = (int) delay;
            //Ad.DateBegin = startTime.AddSeconds(intDelay); //todo datebegin можно определить только после того, как известно точное кол-во выгружаемых позиций (с учетом отфильтрованных невалидных)
            //Ad.DateBegin = startTime;
            Ad.DateBegin = GetDateBegin(startTime, count, position, DaysPeriod, EndHour, StartHour);
            Ad.ManagerName = "Евгений";
            Ad.ContactPhone = "88002001121";
            Ad.AllowEmail = "Да";
            Ad.Region = "Самарская область";
            Ad.City = "Самара";

            //goods type
            var category = fcSneaker.category;
            if (category == "men")
            {
                Ad.Category = "Одежда, обувь, аксессуары";
                Ad.GoodsType = "Мужская одежда";
            }
            else if (category == "women")
            {
                Ad.Category = "Одежда, обувь, аксессуары";
                Ad.GoodsType = "Женская одежда";
            }
            else if (category == "kids")
            {
                _logger.Info("skip sneaker. Reason: kids category");
                return null;
            }
            else
            {
                _logger.Warn("null or wrong category");
                return null;
            }

            Ad.Apparel = "Обувь";

            //size 
            //буду брать случайный русский размер из списка размеров, нужно будет округлить до целого
            var rnd2 = new Random().Next(sneaker.sizes.Count - 1); //todo брать только артикулы у которых 3 или больше размеров в наличии
            var sizeUsAllStock = sneaker.sizes[rnd2];
            var sizeAllStock = new Size(sneaker.brand, sneaker.category, sizeUsAllStock.us, null, null, null, null);
            var sizeFromSizeChart = SizeChart.GetSizeStatic(sizeAllStock);
            if (sizeFromSizeChart == null)
                Ad.Size = "Без размера";
            else
            {
                var ruSize = sizeFromSizeChart.ru.Replace(".", ",");
                double doubleRuSize = 0;
                if (Double.TryParse(ruSize, out doubleRuSize))
                {
                    Ad.Size = Math.Round(doubleRuSize, 0).ToString();
                }
                else
                {
                    Ad.Size = "Без размера";
                }
            }
            if (Ad.Size == "Без размера")
            {
                _logger.Error("Wrong size: " + sizeUsAllStock.us);
                return null;
                //if (sneaker.category == "men")
                //    Ad.Size = "42";
                //else if (sneaker.category == "women")
                //    Ad.Size = "37";
                //else if (sneaker.category == "kids")
                //    Ad.Size = "35";
            }

            //title
            var type = string.Empty;
            if (!string.IsNullOrWhiteSpace(fcSneaker.type))
                type = fcSneaker.type + " ";
            Ad.Title = "Новые " + type + sneaker.title.ToLower();
            if (Ad.Title.Length > 50)
                Ad.Title = Ad.Title.Substring(0, 50);
            Ad.Title = Helper.UpperFirstCharInWord(Ad.Title);

            //description
            var br = "\n";
            var desc = "<![CDATA[" + br;
            if (fcSneaker.type == null) fcSneaker.type = String.Empty;
            desc += "<p>Новые оригинальные " + fcSneaker.type.ToLower() + " " + sneaker.title.ToUpper() + "</p>" + br;
            desc += "<p>Артикул: " + sneaker.sku + "</p>" + br;
            desc += "<p>В оригинальной упаковке Nike.</p>" + br;
            desc += "<p>БЕСПЛАТНАЯ доставка по Самаре в день заказа или на следующий день</p>" + br;
            desc += "<p>Работаем 7 дней в неделю, без праздников и выходных</p>" + br;
            desc += "<p>Доставка по России 3-5 дней компанией СДЭК.</p>" + br;
            desc += "<p>Стоимость доставки по РФ - 300 рублей.</p>" + br;
            desc += "<p>Доставка по РФ только по 100% предоплате, наложенным платежом не отправляем</p>" + br;
            desc += "<p>Работаем с 2014 года, более 4000 моделей в ассортименте. Можете перейти в магазин авито (страница Контакты), потом на сайт и в группу Вконтакте, чтобы убедиться в том, что мы продаем только оригинальную обувь. Никаких подделок и реплик!</p>" + br;
            desc += "<p>В нашей группе Вконтакте более 70 реальных отзывов от клиентов!</p>" + br;
            desc += "<p>Размеры в наличии:</p>" + br;
            desc += "<ul>" + br;
            foreach (var size in sneaker.sizes)
            {
                var scSize = SizeChart.GetSizeStatic(new Size(sneaker.brand, sneaker.category, size.us, null, null, null, null));
                if (scSize != null)
                    desc += "<li>" + scSize.GetAllSizeString() + "</li>" + br;
            }
            desc += "</ul>" + br;
            desc += "<p>Уточняйте наличие размеров.</p>" + br;
            desc += "<p>Если размера нет в наличии, возможно его можно его привезти под заказ. Срок поставки товара под заказ: 7-10 дней. </p>" + br;
            desc += "<p>Sneaker Icon - это большой выбор оригинальной брендовой обуви Nike и Jordan. Кроссовки, кеды, бутсы, футзалки, шиповки, сланцы, ботинки и другие типы обуви</p>" + br;
            desc += "<p>Звоните или пишите в сообщения Авито по всем вопросам</p>" + br;
            desc += "]]>";
            Ad.Description = desc;

            //price
            Ad.Price = (int)sneaker.price + MARGIN;

            Ad.Images = fcSneaker.images;

            return Ad;
        }
    }
}
