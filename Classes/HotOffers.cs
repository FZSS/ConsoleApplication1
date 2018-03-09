using Newtonsoft.Json;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.SizeConverters.Model;
using SneakerIcon.Classes.Utils;
using SneakerIcon.Model.AllStock;
using SneakerIcon.Model.FullCatalog;
using SneakerIcon.Model.HotOffersModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SneakerIcon.Controller;
using SneakerIcon.Controller.Exporter.VK;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes
{
    public class HotOffers
    {
        public const int MARGIN_USD = 20; //маржа в долларах
        public const int MIN_NEED_MARGIN = 50;
        private static string _filename = "HotOffers.json";
        public static int vkGroupId = 131716451;

        public static void Run(int start = 0, int stop = 0, AllStockRoot allStock = null) 
        {
            if (allStock == null)
                allStock = AllStockExporter2.LoadLocalFile();

            var hotOffers = GetHotOffers(allStock);

            if (hotOffers != null)
            {                
                Post3MaxMarginValueHotOffer(hotOffers, start, stop);
            }
        }

        /// <summary>
        /// Этот метод отправляет n*2 случайных хотофферов (n мужских и n женских)
        /// </summary>
        public static void Run2(int n)
        {
            var allStock = AllStockExporter2.LoadLocalFile();

            var hotOffers = GetHotOffers(allStock);

            if (hotOffers != null)
            {
                //берем только те офферы где три размера или больше
                hotOffers.offers = hotOffers.offers.FindAll(x => x.sizes.Count > 3);

                //сортируем размеры от большой скидки к меншей
                hotOffers.offers = hotOffers.offers.OrderByDescending(x => x.regular_price - x.our_price).ToList();

                var menOffers = hotOffers.offers.FindAll(x => x.category == "men");
                var womenOffers = hotOffers.offers.FindAll(x => x.category == "women");
                //var nonCategoryOffers = hotOffers.offers.FindAll(x => string.IsNullOrWhiteSpace(x.category));

                //теперь постим в телеграм первые 3 оффера
                for (int i = 0; i < n; i++)
                {
                    //men
                    var rnd = new Random().Next(menOffers.Count - 1);
                    var offer = menOffers[rnd];
                    if (offer.images[0].Contains("img.sneaker-icon.ru"))
                    {
                        PostPrivateToTelegram(offer);
                        PostPublicToTelegram(offer);
                        PostToTelegramForInsta(offer);
                        System.Threading.Thread.Sleep(5000);
                    }

                    //women
                    rnd = new Random().Next(womenOffers.Count - 1);
                    offer = womenOffers[rnd];
                    if (offer.images[0].Contains("img.sneaker-icon.ru"))
                    {
                        PostPrivateToTelegram(offer);
                        PostPublicToTelegram(offer);
                        PostToTelegramForInsta(offer);
                        System.Threading.Thread.Sleep(5000);
                    }
                }

                SaveJson(hotOffers, _filename);
            }

        }

        public static HotOffersRoot Load()
        {
            var hotOffers = HotOffers.LoadLocalFile();
            var raznica = DateTime.Now - hotOffers.update_time;
            var day = new TimeSpan(24,0,0,0);
            if (raznica > day)
            {
                hotOffers = CreateHotOffers();
            }
            return hotOffers;
        }

        public static void PostVkRandomOffer()
        {
            var hotOffers = Load();
            var rnd = new Random().Next(2);
            var category = "men";
            if (rnd == 1)
                category = "women";
            var offer = GetRandomOffer(hotOffers, category);
            HotOffers.PostToVk(offer);
        }

        public static HotOffersRoot CreateHotOffers()
        {
            var allStock = AllStockExporter2.LoadLocalFile();

            var hotOffers = GetHotOffers(allStock);
            hotOffers.update_time = DateTime.Now;

            if (hotOffers != null)
            {
                //берем только те офферы где три размера или больше
                hotOffers.offers = hotOffers.offers.FindAll(x => x.sizes.Count > 3);

                //сортируем размеры от большой скидки к меншей
                hotOffers.offers = hotOffers.offers.OrderByDescending(x => x.regular_price - x.our_price).ToList();

                SaveJson(hotOffers, _filename);

                return hotOffers;
            }
            else
            {
                return null;
            }
        }

        public static HotOffer GetRandomOffer(HotOffersRoot hotOffers, string category)
        {
            var categoryOffers = hotOffers.offers.FindAll(x => x.category == category);
            var rnd = new Random().Next(categoryOffers.Count - 1);
            return categoryOffers[rnd];
        }

        public static void PostToVk(HotOffer offer)
        {
            var category = string.Empty;
            if (offer.category == "men")
                category = "MEN'S";
            else if (offer.category == "women")
                category = "WOMEN'S";
            else if (offer.category == "kids")
                category = "KIDS";
            else throw new Exception("Wrong category");

            var br = "\n";
            var m = "🔥🔥🔥 СКИДКИ 🔥🔥🔥" + br;
            m += offer.title + " (" + category + ") " + offer.sku + br;
            m += "Обычная цена: " + CurrencyRate.ConvertUsdToRub(offer.regular_price) + " ₽" + br;
            m += "Цена со скидкой: " + CurrencyRate.ConvertUsdToRub(offer.our_price + MARGIN_USD) + " ₽" + br;
            m += "Оригинал. Доставляем по всей России и СНГ за 5-10 дней" + br;
            m += "Для заказа пишите в личку админу, в сообщения группы " + br + "или 89296495017 (Телеграм, Вотсап, Вибер)" + br;
            m +=
                @"#sneakericon #"+offer.category+" #nike #jordan #sneakers #sale #discount #найк #кроссовки #скидки #дисконт #распродажа #kicks #kicksonfire #kicks0l0gy #kicksoftheday #sneaker #sneakerhead";

            VkPosting vk = new VkPosting(vkGroupId);
            vk.PostIntoVkWall(m,offer.images);
        }

        private static void Post3MaxMarginValueHotOffer(HotOffersRoot hotOffers, int start, int stop)
        {
            //берем только те офферы где три размера или больше
            hotOffers.offers = hotOffers.offers.FindAll(x => x.sizes.Count > 3);

            //сортируем размеры от большой скидки к меншей
            hotOffers.offers = hotOffers.offers.OrderByDescending(x => x.regular_price - x.our_price).ToList();

            //теперь постим в телеграм первые 3 оффера
            for (int i = start; i <= stop; i++)
            {
                var offer = hotOffers.offers[i];
                if (offer.images[0].Contains("img.sneaker-icon.ru"))
                {
                    PostPrivateToTelegram(offer);
                    PostPublicToTelegram(offer);
                }
            }

            SaveJson(hotOffers, "HotOffersMaxMargin.json");
        }

        private static void SaveJson(HotOffersRoot json, string filename)
        {
            var folder = Config.GetConfig().DirectoryPathParsing + @"HotOffers\";
            var localFileName = folder + filename;
            //сохраняем на яндекс.диск файл
            var textJson = JsonConvert.SerializeObject(json);
            System.IO.File.WriteAllText(localFileName, textJson);

            ////подгружаем из конфига данные фтп
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            var ftpHost = appSettings["ftpHostSneakerIcon"];
            var ftpUser = appSettings["ftpUserSneakerIcon"];
            var ftpPass = appSettings["ftpPassSneakerIcon"];

            ////загружаем на ftp файл
            var ftpFileName = "HotOffers" + "/" + filename;
            Helper.LoadFileToFtp(localFileName, ftpFileName, ftpHost, ftpUser, ftpPass);
        }

        public static HotOffersRoot LoadLocalFile()
        {
            var folder = Config.GetConfig().DirectoryPathParsing + @"HotOffers\";
            var localFileName = folder + _filename;
            var text = File.ReadAllText(localFileName);
            return JsonConvert.DeserializeObject<HotOffersRoot>(text);
        }

        private static async void PostPublicToTelegram(HotOffer offer)
        {
            var message = "Горячее предложение\n"
                + "Название: " + offer.title + " \n"
                + "Артикул: " + offer.sku + "\n";
            var rusCategory = Helper.ConvertEngToRusCategory(offer.sizes[0].size.category);
            message += "Категория: " + rusCategory + "\n";

            message += "\nЦены: \n";
            var regPriceRub = CurrencyRate.ConvertCurrency("USD", "RUB", offer.regular_price);
            message += "Ритейл: " + offer.regular_price + " USD = " + regPriceRub + " RUB\n";
            var price = offer.our_price + MARGIN_USD;
            var ourPriceRub = CurrencyRate.ConvertCurrency("USD", "RUB", price);
            message += "Наша: " + price + " USD = " + ourPriceRub + "RUB\n";
            
            //sizes
            message += "\nДоступные размеры:\n";
            var sizeChart = SizeChart.ReadSizeChartJson();
            var shops = new List<string>();
            var sizesUS = new List<string>();
            var ourPrices = new List<double>();
            foreach (var offerSize in offer.sizes)
            {
                var size = sizeChart.GetSize(offerSize.size);
                if (size != null)
                {
                    message += size.us + " US / " + size.eu + " EU / " + size.uk + " UK / " + size.cm + " CM \n";
                    ourPrices.Add(offerSize.price);
                    //message += offerSize.price + "USD (with ship and return vat) \n";
                    if (!shops.Contains(offerSize.url))
                        shops.Add(offerSize.url);
                    //message += offerSize.url + "\n";
                }
            }

            //
            message += "\nКак заказать:\n";
            message += "Звоните по телефону +78002001121 \n";
            message += "Пишите в WhatsApp, Viber: +79179549490 (Дмитрий)\n";
            message += "В Телеграм: @Sneaker_icon (Евгений)\n";
            //message += "Наша группа Вконтакте (отзывы о нашей работе с 2012 года): https://vk.com/sneaker_icon \n";


            message += "\nДоставка / Оплата \n";
            message += "Доставка по РФ и СНГ в течение 7-14 дней\n";
            message += "Работаем только по 100% предоплате\n";

            //images
            message += "\nИзображения:\n";
            foreach (var image in offer.images)
            {
                message += image + "\n";
            }

            var chatId = "@sneaker_icon_hot";
            await Helper.TelegramPost(message, chatId);
        }

        private static async void PostPrivateToTelegram(HotOffer offer)
        {
           
            var message = "Горячее предложение\n"
                + "Название: " + offer.title + " \n" 
                + "Артикул: " + offer.sku + "\n";
            var regPriceRub = CurrencyRate.ConvertCurrency("USD", "RUB", offer.regular_price);
            message += "Цена в розницу: " + offer.regular_price + " USD = " + regPriceRub + " RUB\n";
            var ourPriceRub = CurrencyRate.ConvertCurrency("USD", "RUB", offer.our_price);
            message += "Наша цена со скидкой: " + offer.our_price + " USD = " + ourPriceRub + "RUB\n";
            var rusCategory = Helper.ConvertEngToRusCategory(offer.sizes[0].size.category);
            message += "Категория: " + rusCategory + "\n";
            //sizes
            message += "\nДоступные размеры:\n";
            var sizeChart = SizeChart.ReadSizeChartJson();
            var shops = new List<string>();
            var sizesUS = new List<string>();
            var ourPrices = new List<double>();
            foreach (var offerSize in offer.sizes)
            {
                var size = sizeChart.GetSize(offerSize.size);
                if (size != null)
                {
                    message += size.us + " US / " + size.eu + " EU / " + size.uk + " UK / " + size.cm + " CM \n";
                    ourPrices.Add(offerSize.price);
                    //message += offerSize.price + "USD (with ship and return vat) \n";
                    if (!shops.Contains(offerSize.url))
                        shops.Add(offerSize.url);
                    //message += offerSize.url + "\n";
                }
                else
                {
                    Program.Logger.Warn("Ошибка в размере " + offer.title + " " + offer.sku);
                }
            }

            //our prices
            message += "\nНаша цена (с доставкой до РФ и вычетом VAT если есть)\n";
            foreach (var offerSize in offer.sizes)
            {
                var rubPrice = CurrencyRate.ConvertCurrency("USD", "RUB", offerSize.price);
                message += offerSize.size.us + " US: " + offerSize.price + "USD = " + rubPrice + "RUB\n"; 
            }

            //shops
            message += "\nОткуда брать:\n";
            foreach (var url in shops)
            {
                message += url + "\n";
            }

            //images
            message += "\nИзображения:\n";
            foreach (var image in offer.images) 
            {
                message += image + "\n";
            }

            var chatId = "-1001101919442";
            await Helper.TelegramPost(message, chatId);
                
        }

        private static void PostToTelegramForInsta(HotOffer offer)
        {
            var br = "\n";
            var m = "HOT OFFER! 🔥";
            m += br + offer.title.ToUpper() + " " + offer.sku + " (" + offer.category + "'s)";
            m += br + "Regular price: $" + offer.regular_price;
            var sale_price = offer.our_price + MARGIN_USD;
            m += br + "Sale price: $" + sale_price;

            //var chatName = "@hot_offers_insta";
            var chatid = "-1001115230279";
            Helper.TelegramPost(m, chatid);
            System.Threading.Thread.Sleep(15000);

            m = "Images:" + br;
            foreach (var image in offer.images)
            {
                m += image + br; 
            }
            Helper.TelegramPost(m, chatid);
            System.Threading.Thread.Sleep(15000);
        }

        private static HotOffersRoot GetHotOffers(AllStockRoot allStock)
        {
            /* Как определить лучшее предолжение? Горячий пирожок? Как найти самые большие скидки в нашей базе данных?
             * Я думаю что для нас выгодными будут предложения, которые с учетом доставки и ват будут на 30% или даже дешевле чем ритейл цена.
             * Что такое горячий пирожок? Это артикул, у которого есть несколько размеров, удовлетворяющих этому условию
             * Что мы заносим в пирожок? 
             * - артикул, 
             * - цену себестоимости, 
             * - размеры которые попадают под это условия. 
             *  - на каждый размер:
             *   - ссылка на магазин, где можно купить по этой цене данный размер
             *   - ссылки на изображения с нашего сервака (несколько изображений)
             *   - цена этого размера
             * - максимальную цену среди всех размеров (какие-то размеры дешевле, какие-то дороже
             * чтобы определить ритейл цену, берем среднюю цену без учета скидки всех размеров всех офферов
             */

            double minMargin = MIN_NEED_MARGIN; //минимальная маржа в долларах с пирожка. То есть минимальная стоимость разницы между ритейл ценой и себест. с доставкой и вычетом ват
            double minSalePercent = 0.3; //минимальная скидка в процентах
            var myHotOffers = new HotOffersRoot();
            FullCatalogRoot fullCatalog = FullCatalog2.LoadFullCatalogFromFtp();
            foreach (var sneaker in allStock.sneakers)
            {
                var hOffer = GetHotOfferFromAllStockSneaker(sneaker, fullCatalog, minMargin, minSalePercent);
                if (hOffer != null)
                    myHotOffers.offers.Add(hOffer);
            }

            if (myHotOffers.offers.Count > 0)
                return myHotOffers;
            else
                return null;
        }

        private static HotOffer GetHotOfferFromAllStockSneaker(AllStockSneaker sneaker, FullCatalogRoot fullCatalog, double minMargin, double minSalePercent)
        {

            //определяем среднюю ритейл цену этого кросса в долларах
            var averageSneakerRegularPrice = GetAverageRegularPrice(sneaker);

            //смотрим, есть ли офферы, которые удовлетворяютм минимальным требованиям
            var sizes = GetHotOfferSizes(sneaker, averageSneakerRegularPrice, minMargin, minSalePercent);

            //если офферов больше нуля (ну то есть такие офферы, которые удовлетворяют условиям), то добавляем этот горячий пирожок в список горячих пирожков
            if (sizes.Count > 0)
            {
                var hOffer = new HotOffer();
                hOffer.sizes = sizes;
                hOffer.sku = sneaker.sku;
                hOffer.brand = sneaker.brand;
                hOffer.title = sneaker.title;
                hOffer.category = sneaker.category;
                var fcRecord = fullCatalog.records.Find(x => x.sku == sneaker.sku);
                if (fcRecord != null)
                    hOffer.images = fcRecord.images;
                hOffer.our_price = hOffer.GetOurPrice();
                hOffer.regular_price = averageSneakerRegularPrice;
                hOffer.add_time = DateTime.Now;
                return hOffer;
            }
            else
            {
                return null;
            }
        }

        private static List<HotOfferSize> GetHotOfferSizes(AllStockSneaker sneaker, double averageSneakerRegularPrice, double minMargin, double minSalePercent)
        {
            var hoSizes = new List<HotOfferSize>();

            foreach (var size in sneaker.sizes)
            {
                if (size.offers != null)
                    if (size.offers.Count > 0)
                    {
                        var offer = size.offers[0];
                        var price = offer.price_usd_with_delivery_to_usa_and_minus_vat; //по идее нулевой оффер с самой низкой ценой, но нужно проверять
                        var discontValue = averageSneakerRegularPrice - price;
                        var disconPercent = discontValue / averageSneakerRegularPrice;
                        if (discontValue > minMargin && disconPercent > minSalePercent)
                        {
                            var hoSize = new HotOfferSize();
                            hoSize.price = price;
                            hoSize.size = new Size(sneaker.brand, sneaker.category, size.us, null, null, null, null);
                            hoSize.url = offer.url;
                            hoSizes.Add(hoSize);
                        }
                    }
            }

            return hoSizes;
        }

        private static double GetAverageRegularPrice(AllStockSneaker sneaker)
        {
            //определяем среднюю ритейл цену этого кросса в долларах
            var regPriceList = new List<double>();
            foreach (var size in sneaker.sizes)
            {
                foreach (var offer in size.offers)
                {
                    var regPrice = offer.price;
                    if (offer.old_price != 0)
                        regPrice = offer.old_price;
                    var regPriceUsd = CurrencyRate.ConvertCurrency(offer.currency, "USD", regPrice);
                    regPriceList.Add(regPriceUsd);
                }
            }

            double sum = 0;
            foreach (var price in regPriceList)
            {
                sum += price;
            }
            double average = sum / regPriceList.Count;
            return Math.Round(average,2);
        }
    }
}
