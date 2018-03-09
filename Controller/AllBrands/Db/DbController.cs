using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Remotion.Linq.Parsing;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Model.AllBrands.DB;
using SneakerIcon.Model.ShopCatalogModel;
using SneakerIcon.Sys;

namespace SneakerIcon.Controller.AllBrands.Db
{
    public class DbController
    {
        
        public static Logger _logger = LogManager.GetCurrentClassLogger();
        private string br = "\n";
        private string log { get; set; }
        public ShopCatalogRoot ShopCatalog { get; set; }

        public DbController()
        {
            log = String.Empty;
            ShopCatalog = ShopCatalogController.LoadShopCatalogFromFtp();
        }

        public static DbRoot LoadLocalFile()
        {
            var path = Config.GetConfig().DbFolder + Config.GetConfig().DbFileName;
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<DbRoot>(text);
            }
            else
            {
                return new DbRoot();
            }
        }

        public static void SaveLocalFile(DbRoot db)
        {
            var path = Config.GetConfig().DbFolder + Config.GetConfig().DbFileName;
            var text = JsonConvert.SerializeObject(db);
            File.WriteAllText(path,text);
        }

        public void Update()
        {
            var db = LoadLocalFile();
            db.UpdateTime = DateTime.Now;

            DeleteOffers(db);

            AddShopsInfo(db);

            SaveLocalFile(db);

            
        }

        public void Create()
        {
            var db = new DbRoot();
            db.UpdateTime = DateTime.Now;

            AddShopsInfo(db);

            db.Sneakers = db.Sneakers.FindAll(x => x.Category != "kids");
            _logger.Info("Delete kids sneakers");

            SaveLocalFile(db);
            SaveLog();
        }

        private void SaveLog()
        {
            var path = Config.GetConfig().DbFolder + Config.GetConfig().DbFileName;
            path = Config.GetConfig().DbFolder + "_createLog.txt";
            System.IO.File.WriteAllText(path, log);
        }

        private void AddShopsInfo(DbRoot db)
        {
            var shopCatalog = ShopCatalog;
            var shops = ShopCatalogController.GetShopsValidated();
            foreach (var shop in shops)
            {
                var shopCatalogItem = shopCatalog.markets.Find(x => x.name == shop.market_info.name);
                AddShopInfo(db,shop, shopCatalogItem);
            }
        }

        private void AddShopInfo(DbRoot db, RootParsingObject shop, Shop shopCatalogItem)
        {
            foreach (var listing in shop.listings)
            {
                var sneaker = db.Sneakers.Find(x => x.Sku == listing.sku);
                if (sneaker == null)
                {
                    AddDbSneaker(db, listing, shop, shopCatalogItem);
                }
                else
                {
                    //db.Sneakers.RemoveAll(x => x.Sku == listing.sku);
                    //AddDbSneaker(db, listing, shop, shopCatalogItem);

                    UpdateDbSneaker(db, sneaker, listing, shop, shopCatalogItem);
                    //todo запилить этот метод
                }
            }
        }

        private void UpdateDbSneaker(DbRoot db, DbSneaker sneaker, Listing listing, RootParsingObject shop, Shop shopCatalogItem)
        {
            log += "Update Sneaker. sku:" + sneaker.Sku + " title: " + sneaker.Titles[0] + br;
            /* логика:
             * проверяем заголовок, если длиннее, берем его
             * проверяем остальные элементы, если они не нул, а были нул то берем их
             * сравниваем категорию листинга и кроссовка, если не совпадают то отклоняем этот листинг (но по идее такого быть не должно
             */

            //title
            var title = sneaker.Titles.Find(x => x.ToLower() == listing.title.ToLower());
            if (title == null)
            {
                sneaker.Titles.Add(listing.title);
                sneaker.Titles = sneaker.Titles.OrderByDescending(x => x.Length).ToList();
            }

            //color
            if (!string.IsNullOrWhiteSpace(listing.colorbrand))
                if (listing.colorbrand.Length > sneaker.Color.Length)
                    sneaker.Color = listing.colorbrand;

            //category
            //если пустая то добавляем
            if (string.IsNullOrWhiteSpace(sneaker.Category) && !string.IsNullOrWhiteSpace(listing.category))
                sneaker.Category = listing.category;
            //если обе непустые сраванием
            else if (!string.IsNullOrWhiteSpace(sneaker.Category) && !string.IsNullOrWhiteSpace(listing.category))
            {
                if (sneaker.Category != listing.category)
                {
                    log += "Warn! Other categories. sku: " + sneaker.Sku + " sneaker.Category:" + sneaker.Category +
                           " listing.category:" + listing.category + " listing.url: " + listing.url + br;
                }
            }
            //если обе пустые то
            else if (string.IsNullOrWhiteSpace(sneaker.Category) && string.IsNullOrWhiteSpace(listing.category))
            {
                log += "Warn: category is empty" + br;
            }

            //links
            sneaker.Links.Add(listing.url);

            //images
            var imgCol = new DbImageCollection();
            imgCol.ShopName = shopCatalogItem.name;
            foreach (var lImage in listing.images)
            {
                var dbImg = new DbImage();
                dbImg.SiteUrl = lImage;
                imgCol.Images.Add(dbImg);
            }
            sneaker.ImageCollectionList.Add(imgCol);

            //sizes
            foreach (var lSize in listing.sizes)
            {
                //find size
                var snSize = new DbSize();
                if (!string.IsNullOrWhiteSpace(lSize.us))
                {
                    snSize = sneaker.Sizes.Find(x => x.Us == lSize.us);
                    //если размера нет, создаем
                    if (snSize == null)
                    {
                        var dbSize = GetDbSize(sneaker, lSize, listing, shop, shopCatalogItem);
                        sneaker.Sizes.Add(dbSize);
                    }
                    //если есть создаем только оффер
                    else
                    {
                        var offer = CreateDbOffer(sneaker, lSize, listing, shop, shopCatalogItem);
                        snSize.Offers.Add(offer);
                        snSize.Offers = snSize.Offers.OrderBy(x => x.price_usd_with_delivery_to_usa_and_minus_vat)
                            .ToList();
                    }
                }
                else
                {
                    //если юс размера нет, то надо добисать будет код (например для асфальтгольда)
                    throw new Exception("size us is null. Cannot update db sneaker.");
                }                  
            }       
        }

        private void AddDbSneaker(DbRoot db, Listing listing, RootParsingObject shop, Shop shopCatalogItem)
        {
            log += "Add Sneaker. sku:" + listing.sku + " Title:" + listing.title + br;
            var sneaker = new DbSneaker();
            sneaker.Id = db.Sneakers.Count + 1;
            sneaker.Brand = listing.brand;
            sneaker.Sku = listing.sku;
            sneaker.Titles.Add(listing.title);
            sneaker.Color = listing.colorbrand;
            sneaker.Category = listing.category;
            sneaker.Links.Add(listing.url);
            if (listing.images.Count > 0)
            {
                var imageCollection = new DbImageCollection();
                imageCollection.ShopName = shop.market_info.name;
                foreach (var image in listing.images)
                {
                    var dbimg = new DbImage();
                    dbimg.SiteUrl = image;
                    imageCollection.Images.Add(dbimg);
                }
                //imageCollection.Images = listing.images; 
                //todo менять ссылки из магазины на ссылки с нашего сервака
                //todo качать фотки в момент парсинга
                sneaker.ImageCollectionList.Add(imageCollection);
            }
            foreach (var size in listing.sizes)
            {
                var dbSize = GetDbSize(sneaker, size, listing, shop, shopCatalogItem);
                sneaker.Sizes.Add(dbSize);
            }
            db.Sneakers.Add(sneaker);
        }

        private static DbSize GetDbSize(DbSneaker sneaker, ListingSize size, Listing listing, RootParsingObject shop, Shop shopCatalogItem)
        {
            var dbSize = new DbSize();
            dbSize.Us = size.us;
            dbSize.Eu = size.eu;
            dbSize.Uk = size.uk;
            dbSize.Cm = size.cm;
            dbSize.Ru = size.ru;
            dbSize.Upc = size.upc;

            var offer = CreateDbOffer(sneaker, size, listing, shop, shopCatalogItem);
            dbSize.Offers.Add(offer);

            return dbSize;
        }

        private static DbOffer CreateDbOffer(DbSneaker sneaker, ListingSize size, Listing listing, RootParsingObject shop, Shop shopCatalogItem)
        {
            var deliveryUs = shopCatalogItem.delivery.Find(x => x.location == "US");
            var offer = new DbOffer();
            offer.url = listing.url;
            offer.price = listing.price;
            offer.old_price = listing.old_price;
            offer.currency = shopCatalogItem.currency;
            offer.stock_name = shop.market_info.name; 
            offer.price_usd_with_delivery_to_usa_and_minus_vat = AllStockExporter.GetAllStockPrice(offer.price, offer.currency, deliveryUs.value, deliveryUs.vat);
            return offer;
        }

        private static void DeleteOffers(DbRoot db)
        {
            foreach (var sneaker in db.Sneakers)
            {
                foreach (var size in sneaker.Sizes)
                {
                    size.Offers.Clear();
                }
            }
        }

        public static void DownloadImages()
        {
            //ImageDownload imgLoader = new ImageDownload();
            //imgLoader.DownLoadDbImages();
            DownLoadDbImages();
        }

        public static void DownLoadDbImages()
        {
            var db = DbController.LoadLocalFile();
            var shopCatalog = ShopCatalogController.LoadShopCatalogFromFtp();
            db.UpdateTime = DateTime.Now;
            var serverImageUrl = Config.GetConfig().ServerImageUrl;

            var ftpHostContabo = Config.GetConfig().FtpHostContabo;
            var ftpUserContabo = Config.GetConfig().FtpUserContabo;
            var ftpPassContabo = Config.GetConfig().FtpPassContabo;

            foreach (var sneaker in db.Sneakers)
            {
                foreach (var imageCollection in sneaker.ImageCollectionList)
                {
                    var shopCatalogItem = shopCatalog.markets.Find(x => x.name == imageCollection.ShopName);
                    var shopNumber = shopCatalogItem.number;
                    for (int i = 0; i < imageCollection.Images.Count; i++)
                    {
                        var imageNumber = i + 1;
                        var image = imageCollection.Images[i];

                        var ftpPath = "Shops/" + shopNumber + "/Images/" + sneaker.Sku + "-" + imageNumber + ".jpg";

                        //смотрим, есть ли уже в базе картинка с нашим юрл

                        bool isDownloaded = false;
                        if (image.OurUrl != null)
                        {
                            if (image.OurUrl.Contains(serverImageUrl))
                                isDownloaded = true;
                            _logger.Info("Image already exist in db. img:" + imageCollection.Images[i].OurUrl);
                        }


                        //если нет смотрим, может быть мы ее уже загружали?
                        if (!isDownloaded)
                        {
                            string host = serverImageUrl;
                            string url = host + "/shops/" + shopNumber + "/images/" + sneaker.Sku + "-" + imageNumber + ".jpg";
                            if (NetworkUtils.UrlExists(url))
                            {
                                isDownloaded = true;
                                imageCollection.Images[i].OurUrl = serverImageUrl + "/" + ftpPath;
                                _logger.Info("Image already exist on ftp server. img:" + imageCollection.Images[i].OurUrl);
                            }                               
                        }
                        
                        //если нет в базе и нет на фтп, то грузим фотку
                        if (!isDownloaded)
                        {
                            var localFileName = "image.jpg";
                            isDownloaded = Helper.DownloadImage(image.SiteUrl,localFileName,5);

                            if (isDownloaded)
                            {
                                //загружаем файл             
                                try
                                {
                                    Helper.LoadFileToFtp(localFileName, ftpPath, ftpHostContabo, ftpUserContabo,
                                        ftpPassContabo);
                                }
                                catch (Exception e)
                                {
                                    //если ошибка значит скорее всего папки не существует
                                    //создаем папку и пробуем снова загрузить файл
                                    string directory = "Shops/" + shopNumber;
                                    Helper.CreateDirectoryFtp(directory, ftpHostContabo, ftpUserContabo,
                                        ftpPassContabo);
                                    directory = "Shops/" + shopNumber + "/Images";
                                    Helper.CreateDirectoryFtp(directory, ftpHostContabo, ftpUserContabo,
                                        ftpPassContabo);
                                    Helper.LoadFileToFtp(localFileName, ftpPath, ftpHostContabo, ftpUserContabo,
                                        ftpPassContabo);
                                }
                                File.Delete(localFileName);
                                imageCollection.Images[i].OurUrl = serverImageUrl + "/" + ftpPath;
                                _logger.Info("load image:" + serverImageUrl + "/" + ftpPath);
                            }
                            else
                            {
                                bool test = true;
                            }
                            System.Threading.Thread.Sleep(1000);
                        }                
                    }
                    DbController.SaveLocalFile(db);
                }
            }
            DbController.SaveLocalFile(db);
        }
    }
}
