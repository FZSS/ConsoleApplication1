using Newtonsoft.Json;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.SizeConverters.Model;
using SneakerIcon.Classes.Utils;
using SneakerIcon.Model.AvitoModel;
using SneakerIcon.Model.FullCatalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Exporters2
{
    public class AvitoExporter3
    {
        public const int MARGIN = 1500;
        private static string _ftpHost = ConfigurationManager.AppSettings["ftpHostSneakerIcon"];
        private static string _ftpUser = ConfigurationManager.AppSettings["ftpUserSneakerIcon"];
        private static string _ftpPass = ConfigurationManager.AppSettings["ftpPassSneakerIcon"];
        public const string FolderName = "Avito3";
        

        /// <summary>
        /// беру файл оллстока и создаю xml файл для авито
        /// </summary>
        public static void Run() {
            DateTime startTime = new DateTime(2017, 04, 17, 07, 00, 00,DateTimeKind.Local); //запуск 17 апреля в 7 утра
            var allstock = AllStockExporter2.LoadLocalFile();
            var fullCatalog = FullCatalog2.LoadLocalFile();
            AvitoAds avito = createAds(allstock, fullCatalog, startTime);

            //удалим все детские кроссы
            avito.AdList.RemoveAll(x => x.Category == "Детская одежда и обувь");

            SaveToJson(avito, FolderName);
            //AvitoAds avito2 = LoadFromJson();
            SaveToXml(avito, FolderName);
            ReplaceChecialCharacterInXml(FolderName);
            UploadXmlToFtp(FolderName,"Avito");
        }

        internal static void UploadXmlToFtp(string localFolder, string ftpFolder)
        {
            ////подгружаем из конфига данные фтп
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            var ftpHost = appSettings["ftpHostSneakerIcon"];
            var ftpUser = appSettings["ftpUserSneakerIcon"];
            var ftpPass = appSettings["ftpPassSneakerIcon"];

            ////загружаем на ftp файл
            var filename = Config.GetConfig().DirectoryPathExport + localFolder + @"\avito.xml";
            var ftpFileName = ftpFolder + "/" + "Avito.xml";
            Helper.LoadFileToFtp(filename, ftpFileName, ftpHost, ftpUser, ftpPass);
        }

        protected static void ReplaceChecialCharacterInXml(string folder)
        {
            var filename = Config.GetConfig().DirectoryPathExport + folder + @"\avito.xml";
            var text = System.IO.File.ReadAllText(filename);
            text = text.Replace("&lt;", "<").Replace("&gt;", ">");
            System.IO.File.WriteAllText(filename, text);
        }

        public static void SaveToJson(AvitoAds avitoAds, string folder) {
            var filename = Config.GetConfig().DirectoryPathExport + folder + @"\avito.json";
            var text = JsonConvert.SerializeObject(avitoAds);
            System.IO.File.WriteAllText(filename,text);
        }

        public static AvitoAds LoadFromJson(string folder)
        {
            var filename = Config.GetConfig().DirectoryPathExport + folder + @"\avito.json";
            //var text = JsonConvert.SerializeObject(avitoAds);
            var text = System.IO.File.ReadAllText(filename);
            var avito = JsonConvert.DeserializeObject<AvitoAds>(text);
            return avito;
        }

        private static AvitoAds createAds(Model.AllStock.AllStockRoot allstock, FullCatalogRoot fullCatalog, DateTime startTime)
        {
            var Ads = new AvitoAds();
            Ads.StartTime = startTime;
            for (int i = 0; i < allstock.sneakers.Count; i++)
            {
                var sneaker = allstock.sneakers[i];
                var fcSneaker = fullCatalog.records.Find(x => x.sku == sneaker.sku);
                AvitoAd Ad = GetAdFromSneaker(sneaker, fcSneaker, Ads.StartTime, i + 1, allstock.sneakers.Count); //тут есть ошибка. У аллстока может быть больше позиций чем для авито, так как некоторые ад могут нул вернуть и не добавиться. По правильному нужно сначала сделать первый прогон, заполнить все объявления и только потом добавлять дату постинга (зная точно кол-во объявлений)
                if (Ad != null)
                    Ads.AdList.Add(Ad);
            }
            return Ads;
        }

        private static AvitoAd GetAdFromSneaker(Model.AllStock.AllStockSneaker sneaker, FullCatalogRecord fcSneaker, DateTime startTime, int position, int count )
        {
            AvitoAd Ad = new AvitoAd();

            if (fcSneaker.images == null)
                return null;
            if (fcSneaker.images.Count == 0)
                return null;
            if (String.IsNullOrWhiteSpace(sneaker.category))
                return null;
            if (sneaker.sizes == null)
                return null;
            if (sneaker.sizes.Count == 0)
                return null;

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
            double delay = 2592000 / count * position;
            var intDelay = (int)delay;
            //Ad.DateBegin = startTime.AddSeconds(intDelay);
            Ad.DateBegin = startTime;
            Ad.ManagerName = "Евгений";
            Ad.ContactPhone = "88002001121";
            Ad.AllowEmail = "Да";
            Ad.Region = "Москва";
            

            //goods type
            if (sneaker.category == "men")
            {
                Ad.Category = "Одежда, обувь, аксессуары";
                Ad.GoodsType = "Мужская одежда";
            }
            else if (sneaker.category == "women")
            {
                Ad.Category = "Одежда, обувь, аксессуары";
                Ad.GoodsType = "Женская одежда";
            }
            else if (sneaker.category == "kids")
            {
                Ad.Category = "Детская одежда и обувь";
                var rnd = new Random().NextDouble();
                if (rnd > 0.5)
                    Ad.GoodsType = "Для девочек";
                else
                    Ad.GoodsType = "Для мальчиков";
            }

            Ad.Apparel = "Обувь";

            //size 
            //буду брать случайный русский размер из списка размеров, нужно будет округлить до целого
            var rnd2 = new Random().Next(sneaker.sizes.Count - 1);
            var sizeUsAllStock = sneaker.sizes[rnd2];           
            var sizeAllStock = new Size(sneaker.brand,sneaker.category,sizeUsAllStock.us,null,null,null,null);
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
                if (sneaker.category == "men")
                    Ad.Size = "42";
                else if (sneaker.category == "women")
                    Ad.Size = "37";
                else if (sneaker.category == "kids")
                    Ad.Size = "35";
            }

            //title
            Ad.Title = "Новые оригинальные " + sneaker.title.ToUpper() + " " + sneaker.sku;
            if (Ad.Title.Length > 50)
                Ad.Title = Ad.Title.Substring(0, 50);
            //Ad.Title = Ad.Title.ToLower();

            //description
            var br = "\n";
            var desc = "<![CDATA[" + br;
            if (fcSneaker.type == null) fcSneaker.type = String.Empty;
            desc += "<p>Новые оригинальные " + fcSneaker.type.ToLower() + " " + sneaker.title.ToUpper() + "</p>" + br;
            desc += "<p>Артикул: " + sneaker.sku + "</p>" + br;
            desc += "<p>В оригинальной упаковке Nike.</p>" + br;
            desc += "<p>БЕСПЛАТНАЯ доставка по Москве</p>" + br;
            desc += "<p>Доставка по России 3-5 дней компанией СДЭК.</p>" + br;
            desc += "<p>Стоимость доставки по РФ - 300 рублей.</p>" + br;
            desc += "<p>Доставка в регионы только по 100% предоплате, наложенным платежом не отправляем</p>" + br;
            desc += "<p>Работаем с 2012 года, более 4000 моделей в ассортименте. Можете перейти в магазин авито (страница Контакты), потом на сайт и в группу Вконтакте, чтобы убедиться в том, что мы продаем только оригинальную обувь. Никаких подделок и реплик!</p>" + br;
            desc += "<p>В нашей группе Вконтакте более 70 реальных отзывов от клиентов!</p>" + br;
            desc += "<p>Размеры в наличии:</p>" + br;
            desc += "<ul>" + br;
            foreach (var size in sneaker.sizes)
            {
                var scSize = SizeChart.GetSizeStatic(new Size(sneaker.brand, sneaker.category, size.us, null, null, null, null));
                var priceSize = size.offers[0].price_usd_with_delivery_to_usa_and_minus_vat;
                var priceSizeRub = (int)CurrencyRate.ConvertCurrency("USD", "RUB", priceSize) + MARGIN;
                if (scSize != null)
                    desc += "<li>" + scSize.GetAllSizeString() + ". Цена:" + priceSizeRub + "</li>" + br;
            }
            desc += "</ul>" + br;
            desc += "<p>Уточняйте наличие размеров.</p>" + br;
            desc += "<p>Если размера нет в наличии, можем его привезти под заказ. Срок поставки товара под заказ: 7-10 дней. </p>" + br;
            desc += "<p>Sneaker Icon - это большой выбор оригинальной брендовой обуви Nike и Jordan. Кроссовки, кеды, бутсы, футзалки, шиповки, сланцы, ботинки и другие типы обуви</p>" + br;
            desc += "<p>Звоните или пишите в сообщения Авито по всем вопросам</p>" + br;
            desc += "]]>";
            Ad.Description = desc;

            //price
            var priceUsd = sneaker.GetMinPrice(); 
            var priceRub = CurrencyRate.ConvertCurrency("USD","RUB",priceUsd);
            var pricetest = (int)priceRub + MARGIN;
            Ad.Price = (int)Math.Round(priceRub, 0) + MARGIN;

            Ad.Images = fcSneaker.images;

            return Ad;
        }

        public static void SaveToXml(AvitoAds avito, string folder)
        {
            var filename = "Avito140417.xml";
            var pathToXml = Config.GetConfig().DirectoryPathExport + folder + @"\" + filename;
            XmlTextWriter textWritter = new XmlTextWriter(pathToXml, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("Ads");
            textWritter.WriteEndElement();
            textWritter.Close();

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(pathToXml);

            //formatVersion
            XmlElement xRoot = xDoc.DocumentElement;
            XmlAttribute formatVersion = xDoc.CreateAttribute("formatVersion");
            formatVersion.Value = "3";
            xRoot.Attributes.Append(formatVersion);

            //target
            XmlAttribute target = xDoc.CreateAttribute("target");
            target.Value = "Avito.ru";
            xRoot.Attributes.Append(target);

            foreach (var ad in avito.AdList)
            {
                CreateAd(ad, xDoc);
            }

            xDoc.Save(pathToXml);
        }

        private static void CreateAd(AvitoAd ad, XmlDocument document)
        {
            var adXml = document.CreateElement("Ad");


            XmlNode ParamAdXml = document.CreateElement("Id"); // даём имя
            ParamAdXml.InnerText = ad.Id; // и значение
            adXml.AppendChild(ParamAdXml);

            ParamAdXml = document.CreateElement("DateBegin"); // даём имя
            ParamAdXml.InnerText = ad.DateBegin.ToString("yyyy-MM-ddTHH:mm:ss+04:00"); // и значение
            adXml.AppendChild(ParamAdXml);

            ParamAdXml = document.CreateElement("AllowEmail"); // даём имя
            ParamAdXml.InnerText = ad.AllowEmail;
            adXml.AppendChild(ParamAdXml);

            //ManagerName
            ParamAdXml = document.CreateElement("ManagerName"); // даём имя
            ParamAdXml.InnerText = ad.ManagerName;
            adXml.AppendChild(ParamAdXml);

            //ContactPhone
            ParamAdXml = document.CreateElement("ContactPhone"); // даём имя
            ParamAdXml.InnerText = ad.ContactPhone;
            adXml.AppendChild(ParamAdXml);

            //Region
            ParamAdXml = document.CreateElement("Region"); // даём имя
            ParamAdXml.InnerText = ad.Region;
            adXml.AppendChild(ParamAdXml);

            //Region
            if (!String.IsNullOrWhiteSpace(ad.City))
            {
                ParamAdXml = document.CreateElement("City"); // даём имя
                ParamAdXml.InnerText = ad.City;
                adXml.AppendChild(ParamAdXml);
            }          

            //Category
            ParamAdXml = document.CreateElement("Category"); // даём имя
            ParamAdXml.InnerText = ad.Category;
            adXml.AppendChild(ParamAdXml);

            //GoodsType
            ParamAdXml = document.CreateElement("GoodsType"); // даём имя
            ParamAdXml.InnerText = ad.GoodsType;
            adXml.AppendChild(ParamAdXml);

            //Apparel
            ParamAdXml = document.CreateElement("Apparel"); // даём имя
            ParamAdXml.InnerText = ad.Apparel;
            adXml.AppendChild(ParamAdXml);

            //Size
            ParamAdXml = document.CreateElement("Size"); // даём имя
            ParamAdXml.InnerText = ad.Size;
            adXml.AppendChild(ParamAdXml);

            //Title
            ParamAdXml = document.CreateElement("Title"); // даём имя
            ParamAdXml.InnerText = ad.Title;
            adXml.AppendChild(ParamAdXml);

            //Description
            ParamAdXml = document.CreateElement("Description"); // даём имя
            ParamAdXml.InnerText = ad.Description;
            adXml.AppendChild(ParamAdXml);

            //Price
            ParamAdXml = document.CreateElement("Price"); // даём имя
            ParamAdXml.InnerText = ad.Price.ToString();
            adXml.AppendChild(ParamAdXml);

            //Images
            ParamAdXml = document.CreateElement("Images");
            foreach (var image in ad.Images)
            {
                var imageXml = document.CreateElement("Image"); // даём имя
                XmlAttribute url = document.CreateAttribute("url");
                url.Value = image;
                imageXml.Attributes.Append(url);
                ParamAdXml.AppendChild(imageXml);
            }
            adXml.AppendChild(ParamAdXml);

            document.DocumentElement.AppendChild(adXml);
        }

        
    }
}
