using Newtonsoft.Json;
using NLog;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Model.FullCatalog;
using SneakerIcon.Model.ShopCatalogModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Exporters
{
    public class FullCatalog2
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public const string FOLDER = "FullCatalog";
        public const string FILENAME = "FullCatalog.json";

        public static void Run()
        {
            AddNewSneakersFromAllStores();
        }

        public static void AddNewSneakersFromAllStores() {
          
            Model.FullCatalog.FullCatalogRoot fullCatalogJson = FullCatalog2.LoadFullCatalogFromFtp();
            Program.Logger.Info("Items without category: " + CountItemsWithoutCategory(fullCatalogJson));
            Program.Logger.Info("Items without Image: " + CountItemsWithoutImages(fullCatalogJson));

            var shopCatalog = Model.ShopCatalogModel.ShopCatalogRoot.ReadFromFtp();
            
            Program.Logger.Info("Update FullCatalog...");
            Update(fullCatalogJson, shopCatalog);
            Program.Logger.Info("Items without category: " + CountItemsWithoutCategory(fullCatalogJson));
            Program.Logger.Info("Items without Image: " + CountItemsWithoutImages(fullCatalogJson));
            SaveFullCatalogToFtp(fullCatalogJson);
        }

        private static void Update(Model.FullCatalog.FullCatalogRoot fullCatalogJson, Model.ShopCatalogModel.ShopCatalogRoot shopCatalog)
        {
            int addedSneakersCount = 0;
            int numMarket = 1;
            foreach (var market in shopCatalog.markets)
            {
                
                var jsonMarket = Parsing.Model.RootParsingObject.ReadJsonMarketFromFtp(market);
                if (jsonMarket != null) //если нул значит магазин еще не добавлен
                {
                    Program.Logger.Info("Update market...");
                    int addedCount = 0;
                    UpdateFromOneMarket(fullCatalogJson, jsonMarket, shopCatalog, out addedCount);
                    addedSneakersCount += addedCount;
                    Program.Logger.Info(jsonMarket.market_info.name + ": Updated market complete. Added sneakers: " + addedCount);
                }

            }
            Program.Logger.Info(numMarket + ". Update FullCatalog complete. Items count: " + fullCatalogJson.records.Count + ". Added items:" + addedSneakersCount);
            fullCatalogJson.update_time = DateTime.Now;
        }

        private static void UpdateFromOneMarket(Model.FullCatalog.FullCatalogRoot fullCatalogJson, Parsing.Model.RootParsingObject jsonMarket, ShopCatalogRoot shopCatalog, out int addedCount)
        {
            addedCount = 0;
            foreach (var listing in jsonMarket.listings)
            {

                var fcSneaker = fullCatalogJson.records.Find(x => x.sku.ToUpper() == listing.sku.ToUpper());
                if (fcSneaker == null)
                {
                    //если фулкаталогсникер нул, значит этого кроссовка еще нет в фулкаталоге и надо его добавить
                    var fcRecord = CreateFullCatalogRecordFromListing(listing,jsonMarket,shopCatalog);
                    if (fcRecord != null) {
                        fullCatalogJson.records.Add(fcRecord);
                        addedCount++;
                    }
                    else
                    {
                        //значит ошибка какая-то
                        bool test = true;
                    }

                }
                else
                {
                    //если кросс уже существуюет то смотрим что есть в листинге, если есть данные для обновления то обновляем их в кроссовке.
                    //проверить, сохранятся ли данные обновления в фулкаталоге, потому что передаю просто листинг а не весь каталог
                    UpdateSneaker(fcSneaker, listing);
                }
            }
        }

        private static void UpdateSneaker(FullCatalogRecord fcSneaker, Listing listing)
        {
            //category
            var category = fcSneaker.category;
            if (string.IsNullOrWhiteSpace(category))
            {
                category = listing.GetCategory();
                if (category != null)
                {
                    fcSneaker.category = category;
                }
            }

            //images

        }

        private static FullCatalogRecord CreateFullCatalogRecordFromListing(Listing listing, RootParsingObject jsonMarket, ShopCatalogRoot shopCatalog)
        {
            //если нет категории то посмотреть есть ли размеры разных сеток, если есть, то определить категорию по разным размерам.
            FullCatalogRecord fcRecord = new FullCatalogRecord();

            //brand
            if (Validator.ValidateBrand(listing.brand))
            {
                fcRecord.brand = listing.brand;             
            }
            else
            {
                Program.Logger.Warn("wrong brand. sku: " + listing.sku + " market: " + jsonMarket.market_info.name);
                return null;
            }

            //sku
            var sku = listing.sku.ToUpper(); //Aa1234-001 to AA1234-001 nike sku
            if (Validator.ValidateSku(sku,listing.brand))
            {
                fcRecord.sku = sku;
            }
            else
            {
                Program.Logger.Warn("wrong sku: " + listing.sku + " market: " + jsonMarket.market_info.name);
                return null;
            }

            //category
            if (String.IsNullOrWhiteSpace(listing.category))
            {
                //пытаемся определить категорию по размерным сеткам
                listing.GetCategory();
            }
            if (!String.IsNullOrWhiteSpace(listing.category))
            {
                if (Validator.ValidateCategory(listing.category))
                {
                    fcRecord.category = listing.category;
                    fcRecord.sex = Helper.GetEngSexFromEngCategory(fcRecord.category);
                }
                else
                {
                    Program.Logger.Warn("FullCatalog2.CreateFullCatalogRecordFromListing. Invalid listing.category:" + listing.category + " shop:" + jsonMarket.market_info.name + " sku:" + listing.sku);
                }
            }

            //добавить валидации на эти позиции
            fcRecord.title = listing.title;
            fcRecord.color = listing.colorbrand;
            fcRecord.collection = listing.collection;           
            fcRecord.link = listing.url;

            //дописать смену изображений на изображения с сервера
            //fcRecord.images = listing.images;
            fcRecord.images = ChangeImageLinksFromShopToImagesFromOurServer(listing.images, jsonMarket, shopCatalog, listing);


            if (fcRecord.images.Count < listing.images.Count)
            {
                fcRecord.images = listing.images;
                Program.Logger.Warn("Wrong our Server Images. shop:" + jsonMarket.market_info.name + " sku:" + listing.sku);
            }
            else
            {
                bool test = true;
            }

            fcRecord.add_time = DateTime.Now;

            return fcRecord;
        }

        public static FullCatalogRoot LoadFullCatalogFromFtp()
        {
            Program.Logger.Info("Load FullCatalog from FTP...");
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostSneakerIcon"];
            string ftpUser = appSettings["ftpUserSneakerIcon"];
            string ftpPass = appSettings["ftpPassSneakerIcon"];
            string path = appSettings["fullCatalogFtpPath"];

            var textJson = Helper.GetFileFromFtpIntoString(path, ftpHost, ftpUser, ftpPass);
            var fullCatalogJson = JsonConvert.DeserializeObject<FullCatalogRoot>(textJson);
            Program.Logger.Info("FullCatalog downloaded. Items count: " + fullCatalogJson.records.Count);
            return fullCatalogJson;
            
        }

        public static void SaveFullCatalogToFtp(FullCatalogRoot fullCatalogJson)
        {
            Program.Logger.Info("Upload FullCatalog To Ftp...");
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostSneakerIcon"];
            string ftpUser = appSettings["ftpUserSneakerIcon"];
            string ftpPass = appSettings["ftpPassSneakerIcon"];
            //string path = appSettings["fullCatalogFtpPath"];

            var folder = "FullCatalog";
            var filename = "FullCatalog.json";
            var localFileName = Config.GetConfig().DirectoryPathParsing + folder + @"\" + filename;

            var textJson = JsonConvert.SerializeObject(fullCatalogJson);
            System.IO.File.WriteAllText(localFileName, textJson);

            var ftpFileName = folder + "/" + filename;
            Helper.LoadFileToFtp(localFileName, ftpFileName, ftpHost, ftpUser, ftpPass);
            Program.Logger.Info("Uploaded FullCatalog Complete");
        }

        public static FullCatalogRoot LoadLocalFile()
        {
            var folder = FOLDER;
            var filename = FILENAME;
            var localFileName = Config.GetConfig().DirectoryPathParsing + folder + @"\" + filename;
            var textJson = System.IO.File.ReadAllText(localFileName);
            var fullCatalog = JsonConvert.DeserializeObject<FullCatalogRoot>(textJson);
            return fullCatalog;
        }

        private static int CountItemsWithoutCategory(FullCatalogRoot fullCatalogJson)
        {
            var itemsWithoutCategory = fullCatalogJson.records.FindAll(x => string.IsNullOrWhiteSpace(x.category));           
            return itemsWithoutCategory.Count;
        }

        private static int CountItemsWithoutImages(FullCatalogRoot fullCatalog)
        {
            var itemsWithoutImage = fullCatalog.records.FindAll(x => x.images.Count == 0);
            return itemsWithoutImage.Count;
        }

        /// <summary>
        /// Удаляет дубликаты из фулкаталога по sku
        /// </summary>
        public static void DeleteDuplicateItems()
        {
            Program.Logger.Info("Delete duplicate for FullCatalog");
            var fullCatalog = LoadFullCatalogFromFtp();
            var itemsCount = fullCatalog.records.Count;
            
            var newFullCatalog = new FullCatalogRoot();
            newFullCatalog.update_time = DateTime.Now;
            foreach (var record in fullCatalog.records)
            {
                var rec = newFullCatalog.records.Find(x => x.sku == record.sku);
                if (rec == null)
                {
                    newFullCatalog.records.Add(record);
                }
                else
                {
                    Program.Logger.Debug("Duplicate item. Sku: " + rec.sku);
                }
            }
            var countDeleteItems = itemsCount - newFullCatalog.records.Count;
            Program.Logger.Info("Items deleted: " + countDeleteItems);
            Program.Logger.Info("Delete duplicate completed. Items count: " + newFullCatalog.records.Count);

            if (countDeleteItems > 0)
                SaveFullCatalogToFtp(newFullCatalog);
        }

        public static void LoadImagesToFullCatalogFromImagesFolder()
        {
            //получаем список изображений из папки Images на серваке
            var host = "ftp://" + Config.GetConfig().FtpHostContabo;
            var user = Config.GetConfig().FtpUserContabo;
            var pass = Config.GetConfig().FtpPassContabo;
            var images = Helper.GetFileListFromDirectory("Images", host, user, pass).ToList();

            var fullCatalog = LoadFullCatalogFromFtp();

            //очищаем левые фотки 
            //проверить надо, нужен ли вообще этот метод
            //да, нужен. по ходу из дисконта такие косяки прилетают
            foreach (var record in fullCatalog.records)
            {
                if (record.images.Count == 1 && string.IsNullOrWhiteSpace(record.images[0])) 
                    record.images = new List<string>();
            }
            _logger.Info("Старт. Артикулов без фотографий: " + fullCatalog.records.FindAll(x => x.images.Count == 0).Count);

            int countAdded = 0;
            foreach (var fcRecord in fullCatalog.records)
            {
                if (fcRecord.images.Count == 0)
                {
                    var skuImages = images.FindAll(x => x.Contains(fcRecord.sku));
                    //_logger.Warn("\nhas not images. sku: " + fcRecord.sku);
                    if (skuImages.Count > 0)
                    {
                        foreach (var image in skuImages)
                        {
                            var imageUrl = "http://img.sneaker-icon.ru/images/" + image;
                            fcRecord.images.Add(imageUrl);
                        }
                        countAdded++;
                        _logger.Info("\n" + countAdded + " Images added. sku: " + fcRecord.sku);
                    }
                    else
                    {
                        //todo добавить создание файла со списком артикулов без изображений
                    }
                }
            }

            _logger.Info("Старт. Артикулов без фотографий: " + fullCatalog.records.FindAll(x => x.images.Count == 0).Count);

            fullCatalog.SaveToFtp();
        }

        /// <summary>
        /// метод обновляет принудительно все фотки в фулкаталоге на фотки из магазинов. Эти фотки берутся с нашего сервака для фоток
        /// метод проходит по всем магазинам и ищет там фотки для артикула, если они есть то загружает их. 
        /// магазины отсортированы в порядке приоритета фотографи.
        /// если в магазинах нет фоток, то фотки берутся из папки для фоток, загруженных вручную
        /// </summary>
        public static void UpdateFullCatalogImages()
        {
            /*
             * Алгоритм обновления изображений: 
             * берем фулкаталог
             * берем список магазинов
             * берем только магазины без водяных знаков
             * желательно чтобы кроссовки были на белом фоне (или на сером, но не стайловые), 
             * стайловые можно в конце добавить. потом опять пройтись по всем магазам и в конец добавить стайловых фоток. 
             * чтобы у кросса например было 4 обычных фотки и потом стайловые дальше шли
             * также можно сделать приоритет по направлению первого кроссовка 
             * например чтобы в первую очередь брать фотки кросс у которых влево смотрит)
             * Если нигде кросс не найден, то искать фотки в папке images 
             * туда типа вручную фотки загружаем, например для кроссовок из дисконта
             */

            var fullCatalog = LoadFullCatalogFromFtp();
            var shopCatalog = ShopCatalogRoot.ReadFromFtp();
            var shops = new List<RootParsingObject>();
            var startTime = DateTime.Now;

            //очищаем левые фотки
            foreach (var record in fullCatalog.records)
            {
                if (record.images.Count == 1 && string.IsNullOrWhiteSpace(record.images[0]))
                    record.images = new List<string>();
            }

            Program.Logger.Info("Старт. Артикулов без фотографий: " + fullCatalog.records.FindAll(x => x.images.Count == 0).Count);

            //подгружаем json всех магазинов
            foreach (var market in shopCatalog.markets)
            {
                var shop = RootParsingObject.ReadJsonMarketFromFtp(market);
                if (shop != null)
                    shops.Add(shop);
            }

            //отсортировали список магазинов по убыванию количества листингов
            shops.Sort(delegate(RootParsingObject shop1, RootParsingObject shop2)
            { return shop2.listings.Count.CompareTo(shop1.listings.Count); });

            //делаем первый эшелон магазинов с самыми классными фотками
            var titolo = shops.Find(x => x.market_info.name == "titolo.ch");
            var chmielna = shops.Find(x => x.market_info.name == "chmielna20.pl");
            var asfaltgold = shops.Find(x => x.market_info.name == "asphaltgold.de");
            var hhv = shops.Find(x => x.market_info.name == "hhv.de");
            var streetBeat = shops.Find(x => x.market_info.name == "street-beat.ru");
            var firstEshelon = new List<RootParsingObject>() { titolo, chmielna, asfaltgold, hhv, streetBeat };

            //очищаем все артикулы от фоток
            //foreach (var fcRecord in fullCatalog.records)
            //{
            //    fcRecord.images = new List<string>();
            //}
            //Program.logger.Info("Очистили от фоток. Артикулов без фотографий: " + fullCatalog.records.FindAll(x => x.images.Count == 0).Count);

            /*Теперь делаем самое главное:
             * проходимся по всему списку артикулов
             * каждый артикул проверяем на наличие в магазине
             * если он в магазине есть, смотрим, есть ли для него фотки
             * если есть то заполняем
             */

            //первый проход делаем по первому эшелону магазинов
            int i = 0;
            int addedPhotos = 0;
            int updatedPhotos = 0;
            int status1 = 1;
            int status2 = 1;
            Program.Logger.Info("Start FirstEshelon. Всего артикулов:" + fullCatalog.records.Count + " Артикулов без фотографий: " + fullCatalog.records.FindAll(x => x.images.Count == 0).Count);
            foreach (var fcRecord in fullCatalog.records)
            {
                if (fcRecord.sku == "724821-100")
                {
                    bool test = true;
                }

                //пропускаем артикул, если для него уже были взяты фотки с одного из сайтов эшелонов
                List<string> images = new List<string>();
                if (fcRecord.images == null)
                    images = GetImages(fcRecord, firstEshelon, shopCatalog);
                else if (fcRecord.images.Count == 0)
                    images = GetImages(fcRecord, firstEshelon, shopCatalog);
                else if (!fcRecord.images[0].Contains("img.sneaker-icon.ru"))
                    images = GetImages(fcRecord, firstEshelon, shopCatalog);
                else
                {
                    //если уже с нашего сервака фотка, то проверяем, с сайта эшелона или нет
                    var image = fcRecord.images[0];
                    bool isEshelonPhotos = false;
                    foreach (var shop in firstEshelon)
                    {
                        int shopNumber = shopCatalog.markets.Find(x => x.name == shop.market_info.name).number;
                        if (image.Contains("/shops/" + shopNumber + "/images/"))
                        {
                            isEshelonPhotos = true;
                            break;
                        }    
                    }
                    if (!isEshelonPhotos)
                        images = GetImages(fcRecord, firstEshelon, shopCatalog);
                }
                
                if (images.Count > 0)
                {
                    fcRecord.images = images;
                    if (fcRecord.images == null)
                        addedPhotos++;
                    else
                        if (fcRecord.images.Count == 0)
                            addedPhotos++;
                        else
                            updatedPhotos++;
                }
                i++;
                if (i > 100 * status1)
                {
                    status1++;
                }
                if (status1 > status2)
                {
                    var time = DateTime.Now - startTime; 
                    Program.Logger.Debug("Пройдено:" + i + " Добавлено:" + addedPhotos + " Обновлено:" + updatedPhotos + " time:" + time.Minutes);
                    status2 = status1;
                }
            }
            Program.Logger.Info("Finish FirstEshelon. Всего артикулов:" + fullCatalog.records.Count + " Артикулов без фотографий: " + fullCatalog.records.FindAll(x => x.images.Count == 0).Count);
            Program.Logger.Info("Добавлено артикулов:" + addedPhotos + " Обновлено артикулов:" + updatedPhotos);

            
            //теперь проходимся по всем магазинам (можно конечно первый эшелон удалить, но чет лень, всё равно фотки с изображениями пропускаем)
            Program.Logger.Info("Start AllShops. Всего артикулов:" + fullCatalog.records.Count + " Артикулов без фотографий: " + fullCatalog.records.FindAll(x => x.images.Count == 0).Count);
            i = 0;
            status1 = 1;
            status2 = 1;
            foreach (var fcRecord in fullCatalog.records)
            {
                //пропускаем фотки у которых в имени файла уже содержится img.sneaker-icon.ru, т.е. значит что они уже были добавлены из первого эшелона или про прошлом проходе UpdateFullCatalogImages. по сути нам нужно взять только те артикулы, у которых нет фоток или у которых фотки с сайта а не с нашего сервака
                List<string> images = new List<string>();
                if (fcRecord.images == null)
                {
                    images = GetImages(fcRecord, shops, shopCatalog);
                }
                else if (fcRecord.images.Count == 0)
                {
                    images = GetImages(fcRecord, shops, shopCatalog);
                }
                else if (!fcRecord.images[0].Contains("img.sneaker-icon.ru/shops/"))
                {
                    images = GetImages(fcRecord, shops, shopCatalog);
                }
                
                if (images.Count > 0)
                {
                    fcRecord.images = images;
                    if (fcRecord.images == null)
                        addedPhotos++;
                    else
                        if (fcRecord.images.Count == 0)
                            addedPhotos++;
                        else
                            updatedPhotos++;
                }
                i++;
                if (i > 100 * status1)
                {
                    status1++;
                }
                if (status1 > status2)
                {
                    var time = DateTime.Now - startTime;
                    Program.Logger.Debug("Пройдено:" + i + " Добавлено:" + addedPhotos + " Обновлено:" + updatedPhotos + " time:" + time.Minutes);
                    status2 = status1;
                }
            }
            Program.Logger.Info("Finish AllShops. Всего артикулов:" + fullCatalog.records.Count + " Артикулов без фотографий: " + fullCatalog.records.FindAll(x => x.images.Count == 0).Count);
            Program.Logger.Info("Добавлено артикулов:" + addedPhotos + " Обновлено артикулов:" + updatedPhotos);

            SaveFullCatalogToFtp(fullCatalog);
        }

        /// <summary>
        /// метод ищет изображения для fullCatalogRecord во всех магазинах
        /// </summary>
        /// <param name="fcRecord">FullCatalogRecord</param>
        /// <param name="shops">Магазины с листингами в json</param>
        /// <param name="shopCatalog"></param>
        /// <returns>возвращает список ссылок на изображения с нашего сервера</returns>
        private static List<string> GetImages(FullCatalogRecord fcRecord, List<RootParsingObject> shops, ShopCatalogRoot shopCatalog)
        {
            /* Как лучше всего сделать изображения? Какие варианты?
             * - пройтись по всем магазинам, найти магазин где фоток больше всего и взять оттуда
             * - пройтись по всем магазинам и взять все фотки
             * - пройтись по магазам и взять там где 4 фотки в первую очередь (хорошо для бонанзы)
             * - сделать список магазинов по приоритетам откуда брать фотки (в первую очередь оттуда где белый фон)
             * - добавлять стайловые фотки в конце
             * - оттуда где первый кроссовок влево смотрит
             * - отсортировать магазы по кол-ву артикулов. брать в первую очередь оттуда, где артикулов больше (чтобы больше кроссовок были в едином стиле)
             * бля, вариантов куча, у каждого свои преимущества... что же делать как же быть...
             * - точно нужно отсеивать магазины с водяными знаками. сто пудов. например оверкилл
             * - можно в конец добавлять стайловые фотки если их больше 4 (тогда на бонанзу только норм фотки, а на сайт sneaker-icon.ru только качественные фотки
             * - можно еще правило что с русских сайтов берем фотки в последнюю очередь
             */
            var imagesFromOurServer = new List<string>();

            foreach (var shop in shops)
            {
                var shopRecord = shop.listings.Find(x => x.sku == fcRecord.sku);
                if (shopRecord != null)
                {
                    if (shopRecord.images.Count > 0)
                    {
                        //меняем ссылки с сайта на ссылки с нашего сервера (если они там есть), если нет ищем дальше
                        imagesFromOurServer = ChangeImageLinksFromShopToImagesFromOurServer(shopRecord.images, shop, shopCatalog, shopRecord);
                        if (imagesFromOurServer.Count == shopRecord.images.Count)
                        {
                            return imagesFromOurServer;
                        }
                            
                    }
                }
            }

            return imagesFromOurServer;
        }

        private static List<string> ChangeImageLinksFromShopToImagesFromOurServer_OldVer(List<string> images, RootParsingObject shop, ShopCatalogRoot shopCatalog, Listing shopRecord)
        {
            var imagesFromOurServer = new List<string>();

            for (int i = 1; i < images.Count+1; i++)
            {
                string host = "http://img.sneaker-icon.ru";
                int shopNumber = shopCatalog.markets.Find(x => x.name == shop.market_info.name).number;

                var ftphost = "ftp://" + Config.GetConfig().FtpHostContabo;
                var user = Config.GetConfig().FtpUserContabo;
                var pass = Config.GetConfig().FtpPassContabo;
                var folder = "Shops/" + shopNumber + "/Images";
                var folderImages = Helper.GetFileListFromDirectory(folder, ftphost, user, pass).ToList();

                string url = host + "/shops/" + shopNumber + "/images/" + shopRecord.sku + "-" + i + ".jpg";

                //var skuImages = folderImages.FindAll(x => x.)

                if (NetworkUtils.UrlExists(url))
                {
                    imagesFromOurServer.Add(url);
                }
            }
            return imagesFromOurServer;
        }

        private static List<string> ChangeImageLinksFromShopToImagesFromOurServer(List<string> images, RootParsingObject shop, ShopCatalogRoot shopCatalog, Listing shopRecord)
        {
            var imagesFromOurServer = new List<string>();
            

            string host = "http://img.sneaker-icon.ru";
            int shopNumber = shopCatalog.markets.Find(x => x.name == shop.market_info.name).number;

            for (int i = 1; i < images.Count + 1; i++)
            {            
                string url = host + "/shops/" + shopNumber + "/images/" + shopRecord.sku + "-" + i + ".jpg";           

                if (NetworkUtils.UrlExists(url))
                {
                    imagesFromOurServer.Add(url);
                }
            }


            //ver2.0
            var imagesFromOurServer2 = new List<string>();

            var ftphost = "ftp://" + Config.GetConfig().FtpHostContabo;
            var user = Config.GetConfig().FtpUserContabo;
            var pass = Config.GetConfig().FtpPassContabo;
            var folder = "shops/" + shopNumber + "/images";
            var folderImages = Helper.GetFileListFromDirectory(folder, ftphost, user, pass).ToList();
            var skuImages = folderImages.FindAll(x => x.Contains(shopRecord.sku));

            foreach (var image in skuImages)
            {
                var imageUrl = host + "/" + folder + "/" + image;
                imagesFromOurServer2.Add(imageUrl);
            }

            if (imagesFromOurServer.Count != imagesFromOurServer2.Count)
            {
                bool test = true;
            }

            return imagesFromOurServer;
        }

        public static void PostNewTodayItems(FullCatalogRoot fullCatalog = null) {
            if (fullCatalog == null)
                fullCatalog = FullCatalog2.LoadFullCatalogFromFtp();
            var allstock = AllStockExporter2.LoadLocalFile();
            var items = fullCatalog.records.FindAll(x => DateTime.Now - x.add_time < new TimeSpan(24,0,0));
            var chatId = "-1001116441282"; //"@sneaker_icon_new_today"

            var message = "New items today: " + items.Count;
            _logger.Info(message);
            Helper.TelegramPost(message, chatId);
            System.Threading.Thread.Sleep(15000);

            var count = items.Count;
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                var ret = "\n";
                var m = "New model in our store:\n";
                m += item.title + ret;
                m += "SKU: " + item.sku + ret;
                m += "Category: " + item.category + "\n";
                m += "Add time: " + item.add_time + ret;
                //m += "Link: " + item.link + "\n\n";
                foreach (var image in item.images)
                {
                    m += image + "\n";
                }


                _logger.Info("NewToday post sku:" + item.sku);
                Helper.TelegramPost(m, chatId);
                System.Threading.Thread.Sleep(15000);
            }
        }

        public static void DetectCategoryFromTitleAndUpdateFullCatalog()
        {
            var countDetect = 0;
            var correctCount = 0;
            var warncount = 0;
            var fc = FullCatalog2.LoadFullCatalogFromFtp();
            _logger.Info("FullCatalog items: " + fc.records.Count);
            _logger.Info("FullCatalog items without category: " + fc.records.FindAll(x => String.IsNullOrWhiteSpace(x.category)).Count);
            foreach (var record in fc.records)
            {
                if (String.IsNullOrWhiteSpace(record.category))
                {
                    record.title = record.title.Replace("Десткие", "").Trim();
                    var category = Validator.DetectCategoryFromTitle(record);
                    if (category != null)
                    {
                        _logger.Info(" \n " + record.sku + " " + record.title);
                        _logger.Info("\ndetect category: " + category + " sku: " + record.sku);
                        record.category = category;
                        countDetect++;
                    }


                }
                else
                {
                    //проверяем совпадает ли категория с той что уже есть в фулкаталоге
                    var category = Validator.DetectCategoryFromTitle(record);
                    if (category != null)
                    {
                        if (category != record.category)
                        {
                            _logger.Info(" \n " + record.sku + " " + record.title);
                            _logger.Info("\nchange category. old: " + record.category + "  new: " + category + " sku: " + record.sku);
                            record.category = category;
                            //record.category = category;
                            correctCount++;
                        }
                    }
                    else
                    {
                        if (record.category != "men")
                        {
                            _logger.Warn("\ntitle: " + record.title + "\n" 
                                + "category:" + record.category + "\n" 
                                + "sku:" + record.sku + "\n" 
                                + "link:" + record.link);
                            warncount++;
                        }
                    }
                }
            }
            _logger.Info("Определено категорий: " + countDetect);
            _logger.Info("Исправлено категорий: " + correctCount);
            _logger.Info("Подозрительных: " + warncount);
            _logger.Info("FullCatalog items without category: " + fc.records.FindAll(x => String.IsNullOrWhiteSpace(x.category)).Count);

            FullCatalog2.SaveFullCatalogToFtp(fc);
        }
    }
}
