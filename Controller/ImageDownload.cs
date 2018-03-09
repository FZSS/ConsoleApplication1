using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.Utils;
using SneakerIcon.Controller.AllBrands.Db;
using SneakerIcon.Model.ShopCatalogModel;
using SneakerIcon.Sys;

namespace SneakerIcon.Controller
{
    public class ImageDownload
    {
        private readonly ShopCatalogRoot _marketCatalogRoot = ShopCatalogRoot.ReadFromFtp();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ChromeDriver _driver;
        
        private readonly Ftp _ftp;

        public ImageDownload()
        {
            _ftp = new Ftp("ftp://" + Config.GetConfig().FtpHostContabo, Config.GetConfig().FtpUserContabo, Config.GetConfig().FtpPassContabo);
        }

        public void RunForStock(int number)
        {
            try
            {
                var option = new ChromeOptions();
                option.AddArgument("--incognito");
                _driver = new ChromeDriver(option);

                Logger.Debug("Начало обновления картинок стока");

                //todo не оптимально выдёргивать сразу все стоки. По-возможности разобраться
                var listStockObj = GetRootParsingObjects();
                var nameStock = _marketCatalogRoot.markets.FirstOrDefault(x => x.number == number);

                if (nameStock == null)
                {
                    throw new NullReferenceException("Не обнаружен сток с номером в каталоге с номерами" + number);
                }

                var rootParsingObj = listStockObj.FirstOrDefault(x => x.market_info.name.Equals(nameStock.name));

                if (rootParsingObj == null)
                {
                    throw new NullReferenceException("Не обнаружен сток с номером в каталоге с json файлами " + number);
                }

                WorkForStock(rootParsingObj, true, number);

                _driver.Quit();

                Logger.Debug("Окончание обновления картинок стока");
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Console.ReadLine();
            }
        }

        public void Run()
        {
            try
            {
                Logger.Debug("Начало работы обработчика картинок");

                var listStockObj = GetRootParsingObjects();

                var option = new ChromeOptions();
                option.AddArgument("--incognito");
                _driver = new ChromeDriver(option);

                foreach (var item in listStockObj)
                {
                    var marketCatalog = _marketCatalogRoot.markets.FirstOrDefault(x => x.name.Equals(item.market_info.name));
                    if (marketCatalog == null)
                    {
                        continue;
                    }

                    var folder = "Shops/" + marketCatalog.number + "/Images/";
                    var ftpFileList = _ftp.DirectoryListSimple(folder);

                    WorkForStock(item, false, marketCatalog.number, ftpFileList);
                }

                _driver.Quit();

                Logger.Debug("Окончание работы обработчика картинок");
            }
            catch (Exception e )
            {
                Logger.Error(e);
                Console.ReadLine();
            }
        }

        public void DownLoadDbImages()
        {
            var db = DbController.LoadLocalFile();
            var shopCatalog = ShopCatalogController.LoadShopCatalogFromFtp();
            db.UpdateTime = DateTime.Now;
            var serverImageUrl = Config.GetConfig().ServerImageUrl;

            var ftpHostContabo = Config.GetConfig().FtpHostContabo;
            var ftpUserContabo = Config.GetConfig().FtpUserContabo;
            var ftpPassContabo = Config.GetConfig().FtpPassContabo;

            var option = new ChromeOptions();
            option.AddArgument("--incognito");
            _driver = new ChromeDriver(option);

            foreach (var sneaker in db.Sneakers)
            {
                foreach (var imageCollection in sneaker.ImageCollectionList)
                {
                    var shopCatalogItem = shopCatalog.markets.Find(x => x.name == imageCollection.ShopName);
                    var shopNumber = shopCatalogItem.number;
                    for (int i = 0; i < imageCollection.Images.Count; i++)
                    {
                        var imageNumber = i + 1;
                        var image = imageCollection.Images[i].SiteUrl;
                        //if (!image.Contains(serverImageUrl))
                        //{
                            bool isDownloaded = DownloadImageFile(shopNumber, sneaker.Sku, imageNumber, image);
                            if (isDownloaded)
                            {
                                var ftpPath = "Shops/" + shopNumber + "/Images/" + sneaker.Sku + "-" + imageNumber + ".jpg";
                                Helper.LoadFileToFtp("image.jpg", ftpPath, ftpHostContabo, ftpUserContabo, ftpPassContabo);
                                File.Delete("image.jpg");
                                imageCollection.Images[i].OurUrl = serverImageUrl + "/" + ftpPath;
                            }                          
                        //}                  
                    }
                    //DbController.SaveLocalFile(db);
                }
            }
            DbController.SaveLocalFile(db);
            _driver.Quit();
        }

        private void WorkForStock(RootParsingObject rootParsingObject, bool isRewrite, int numberCatalog , string[] containsImage = null)
        {
            foreach (var listing in rootParsingObject.listings)
            {
                for (var i = 0; i < listing.images.Count; i++)
                {
                    var filename = listing.sku + "-" + (i + 1) + ".jpg";

                    if (isRewrite)
                    {
                        DownloadImageFile(numberCatalog, filename, listing.images[i]);
                    }
                    else
                    {
                        if (containsImage == null)
                        {
                            throw new NullReferenceException("нет данных по containsImage или numberCatalog");
                        }

                        if (!containsImage.Contains(filename))
                        {
                            DownloadImageFile(numberCatalog, filename, listing.images[i]);
                            
                        }
                        else
                        {
                            Logger.Debug("Изображение " + filename + " актуально");
                        }
                    }

                    
                }
            }
        }

        private bool DownloadImageFile(int number, string filename, string url)
        {
            if (url.Equals(""))
            {
                Logger.Error("Пришла пустая строка в виде url");
                return false;
            }

            _driver.Navigate().GoToUrl(url);
            //пауза для полной загрузки изображения
            System.Threading.Thread.Sleep(Config.GetConfig().TimeoutImageLoad);

            bool isSaved = SaveImage(number, filename);
            return isSaved;
        }

        public bool DownloadImageFile(int shopNumber, string sku, int imageNumber, string url)
        {
            var fileName = sku + "-" + imageNumber + ".jpg";
            bool isDownload = DownloadImageFile(shopNumber,fileName,url);
            return isDownload;

        }

        public bool SaveImage(int number, string filename, int countDownload = 0)
        {
            try
            {
                var myScreenhot = Common.TakeScreenshot(_driver, "screenshot.png");

                var element = _driver.FindElement(By.TagName("img"));
                if (element == null)
                {
                    return false;
                }

                // Устанавливаем координаты изображения и ее размер
                var parSection = new Rectangle(element.Location.X, element.Location.Y, element.Size.Width - 3, element.Size.Height - 3);
                // Создаем изображение с заданым размером
                var bmpCaptcha = new Bitmap(parSection.Width - 3, parSection.Height - 3);

                // Вырезаем область изображения
                var g = Graphics.FromImage(bmpCaptcha);
                g.DrawImage(myScreenhot, -3, -3, parSection, GraphicsUnit.Pixel);
                g.Dispose();

                var count = 0;


                for (var i = 0; i < bmpCaptcha.Width - 1; i++)
                {
                    var color = bmpCaptcha.GetPixel(i, 0);

                    if (color.B == 0 && color.G == 0 && color.R == 0)
                    {
                        count++;
                    }

                }

                if (count > bmpCaptcha.Width * 0.8 && countDownload < 3)
                {
                    return SaveImage(number, filename, count + 1);
                }

                bmpCaptcha.Save("image.jpg", ImageFormat.Jpeg);

                SaveFileIntoFtp(number, filename);

                File.Delete("image.jpg");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number">number shop</param>
        /// <param name="filename"></param>
        private void SaveFileIntoFtp(int number, string filename)
        {
            var nameFolder = number.ToString();

            var isfolders = _ftp.DirectoryListSimple("Shops").Contains(nameFolder);

            if (!isfolders)
            {
                _ftp.CreateDirectory(nameFolder);
                _ftp.CreateDirectory(nameFolder + "/Images/");
            }

            _ftp.Upload("Shops/" + nameFolder + "/Images/" + filename, "image.jpg");
            Logger.Debug("Изображение " + filename + " в сток " + nameFolder + " добавлено");

        }

        

        private IEnumerable<RootParsingObject> GetRootParsingObjects()
        {
            var appSettings = ConfigurationManager.AppSettings;

            var localFtp = new Ftp("ftp://" + appSettings["ftpHostParsing"], appSettings["ftpUserParsing"], appSettings["ftpPassParsing"]);

            //todo жуткий костыль.Разгрести это нечто
               // var dirs = Helper.GetListDirectoryFromFtp(
               //"ftp://" + appSettings["ftpHostParsing"],
               //appSettings["ftpUserParsing"],
               //appSettings["ftpPassParsing"]);

            var dirs = localFtp.DirectoryListSimple();

            var rootParsingObj = new List<RootParsingObject>();

            foreach (var dir in dirs)
            {
                //var outerDir = Helper.GetListDirectoryFromFtp(Helper.Combine("ftp://" + appSettings["ftpHostParsing"], dir),
                //    appSettings["ftpUserParsing"], appSettings["ftpPassParsing"]); 
                var outerDir = localFtp.DirectoryListSimple(dir);
                if (outerDir.Length == 0)
                {
                    continue;
                }

                var filename = outerDir.SingleOrDefault(x => !x.Contains("_old.json") && x.EndsWith(".json") && !x.Contains("sales"));
                if (filename == null)
                {
                    Logger.Warn("В ftp папке " + dir + " не обнаружен json файл");
                    continue;
                }

                var parsObj = localFtp.GetJsonObjFromFtp<RootParsingObject>(filename);

                if (parsObj?.market_info?.name == null)
                {
                    Logger.Warn("Ошибка при десериализации " + filename + ". Не пройдена проверка на нулл");
                    continue;
                }

                var market = _marketCatalogRoot.markets.FirstOrDefault(x => x.name.Equals(parsObj.market_info.name));
                if (market == null)
                {
                    continue;
                }

                rootParsingObj.Add(parsObj);
                Logger.Debug("Провалидирован сток " + dir);
            }

            return rootParsingObj;
        }
    }
}
