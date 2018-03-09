using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.DAO;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.Parsing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using SneakerIcon.Classes.Utils;
using SneakerIcon.Classes.UPCDB;
using SneakerIcon.Classes.Parsing.SivasSale;
using SneakerIcon.Controller;
using SneakerIcon.Controller.Exporter;
using SneakerIcon.Classes.Exporters2;
using SneakerIcon.Classes.Stocks;
using SneakerIcon.Controller.AllBrands;
using SneakerIcon.Controller.AllBrands.Db;
using SneakerIcon.Controller.AllBrands.Exporter;
using SneakerIcon.Controller.AllBrands.Parser.ShopsParsers;
using SneakerIcon.Controller.Exporter.Avito;
using SneakerIcon.Controller.Exporter.VK;
using SneakerIcon.Controller.Parser.ShopsParsers;
using SneakerIcon.Controller.TelegramController;
using SneakerIcon.Controller.Upwork;
using SneakerIcon.Controller.ValitatorController;
using SneakerIcon.Sys;

namespace SneakerIcon
{
    public class Program
    {
        public static DAO DAO;

        public static Logger Logger = LogManager.GetCurrentClassLogger();
        // ReSharper disable once RedundantAssignment
        public static void Main(string[] args)
        {
#if Feanor
            Logger.Info("Feanor mode!");
            args = new[] { "-DownloadImage" };
            //args = new string[] { "-afew" };
            //args = new string[] { "-bonanza_export" };
            //args = new string[] { "-upcdb" };

            //args = new string[] { "-nashStockParseCSVinJSONandGoToFTP" };
            //args = new string[] { "-basketshop_einhalb_queens" }; 
#elif DEBUG
            Logger.Info("Debug mode!");
            args = new[] { "-debug" };
            //args = new[] { "-snkrs.com" };
            //args = new[] { "-ValidateAllShops" };
            //args = new[] { "-DbCreate" };
            //args = new[] { "-DbController.DownloadImages" };
            //args = new[] { "-BonanzaExport3OnlyBonanza" };
            //args = new[] { "-BonanzaExport3" };
            //args = new[] { "-LoadVkGoods" };
            //args = new[] { "-SivasAB" };
            //args = new[] { "-PostHotOfferVk" };
            //args = new[] { "-SivasAB" }; 
            //args = new[] { "-snsAllBrands" };   
            //args = new[] { "-OnlyBonanzaExport2" };
            //args = new[] { "-BonanzaExport2" };
            //args = new[] { "-AvitoNovoslob" };
            
#else
            Console.WriteLine("Release mode!");
#endif

            var startTime = DateTime.Now;

            if (args.Count() > 0)
            {
                Logger.Info("Параметр:" + args[0]);

                if (args[0] == "-AvitoNovoslob")
                {

                    //парсим файл дисконта, создаем csv и json файлы
                    //DiscontMskNovoslobParser novoslobParser = new DiscontMskNovoslobParser();
                    //novoslobParser.Run();

                    //добавляем инфу в фулкаталог
                    //FullCatalog2.Run();
                    Logger.Info("FullCatalog2 created");

                    //подгружаем фотки
                    //FullCatalog2.UpdateFullCatalogImages();
                    //FullCatalog2.LoadImagesToFullCatalogFromImagesFolder();
                    Logger.Info("FullCatalog2 images updated");

                    //делаем фид ля авито
                    AvitoMskNovoslobExporter.Run();
                    Logger.Info("Avito Feed created");
                }

                if (args[0] == "-SivasAB")
                {
                    SivasParserAB sivasParserAb = new SivasParserAB();
                    sivasParserAb.Run();
                }

                if (args[0] == "-snkrs.com")
                {
                    SnkrsComParserAllBrands snrks = new SnkrsComParserAllBrands();
                    snrks.Run();
                }

                if (args[0] == "-snsAllBrands")
                {
                    SneakersnstuffParserAllBrands snsAllBrands = new SneakersnstuffParserAllBrands();
                    //snsAllBrands.ParseOneSneaker("https://www.sneakersnstuff.com/en/product/25821/reebok-question-lux");
                    snsAllBrands.Run();
                }

                if (args[0] == "-LoadVkGoods")
                {
                    var vkExporter = new VkExporter();
                    vkExporter.Run();
                }

                if (args[0].Equals("-feanor"))
                {
                    //Bitmap image1 = (Bitmap)Image.FromFile(@"C:\test\test.jpg", true);

                    //var color0 = image1.GetPixel(0, 0);
                    //var color1 = image1.GetPixel(1, 0);
                    //var color2 = image1.GetPixel(0, image1.Height - 1);
                    //var color3 = image1.GetPixel(1, image1.Height - 1);

                    




                    //var blackLine = true;
                    ////string hex = "#FFFFFF";
                    ////Color _color = System.Drawing.ColorTranslator.FromHtml(hex);

                    //for (var i = 0; i < image1.Width; i++)
                    //{
                    //    var color = image1.GetPixel(i, image1.Height - 1);

                    //    if (color.B < 253 && color.G < 253 && color.R < 253)
                    //    {
                    //        blackLine = false;
                    //    }

                    //}

                    //Console.WriteLine("blackLine");





                    //var vkExporter = new VkExporter();
                    //vkExporter.Run();


                    //var id = 798354;

                    //var array = new List<string>
                    //{
                    //    "http://img.sneaker-icon.ru/shops/16/images/860558-001-1.jpg",
                    //    "http://img.sneaker-icon.ru/shops/16/images/860558-001-2.jpg",
                    //    "http://img.sneaker-icon.ru/shops/16/images/860558-001-3.jpg",
                    //    "http://img.sneaker-icon.ru/shops/16/images/860558-001-4.jpg"
                    //};

                    //var sizeArray = new List<Size>
                    //{
                    //    new Size("brand", "category", "us1", "eu1", "uk1", "cm1", "ru1"),
                    //    new Size("brand", "category", "us2", "eu2", "uk2", "cm2", "ru2"),
                    //    new Size("brand", "category", "us3", "eu3", "uk3", "cm3", "ru3"),
                    //    new Size("brand", "category", "us4", "eu4", "uk4", "cm4", "ru4"),
                    //    new Size("brand", "category", "us5", "eu5", "uk5", "cm5", "ru5")
                    //};

                    //var vkGood = new VkGoodsItem()
                    //{
                    //    Name = "NoName",
                    //    Description = "Other Description",
                    //    Price = 500,
                    //    Images = array,
                    //    Sizes = sizeArray
                    //};

                    //var vk = new VkPosting();
                    ////var idAdd =  vk.AddOrEditVkGoods(vkGood, 798354);
                    //vk.DeleteVkGoods(id);



                }

                if (args[0] == "-bonanza_export")
                {
                    // ReSharper disable once UnusedVariable
                    CreateCatalog createCatalog = new CreateCatalog();
                    FullCatalog.CleanFullCatalog(); //очищаем каталог от левых артикулов, поправляем названия и бренды. костыль
                    Logger.Info("FullCatalog created");                 

                    CurrencyRate currate = new CurrencyRate();
                    currate.Run();
                    AllStockExporter exporter = new AllStockExporter();                   
                    exporter.CreateAllStockList();
                    Logger.Info("AllStock created");

                    BonanzaExporter bonanza = new BonanzaExporter();
                    bonanza.Run();
                    Logger.Info("Bonanza created");
                    TiuExporter tiu = new TiuExporter();
                    tiu.Run();
                    Logger.Info("Tiu created");
                    SneakerIconExporter sneakerIconExporter = new SneakerIconExporter();
                    sneakerIconExporter.Run();
                    Logger.Info("Sneaker-icon.ru.csv created");

                    //FullCatalog2.Run();
                    //Console.WriteLine("FullCatalog2 created");

                    //FullCatalog2.PostNewTodayItems();

                    //AllStockExporter2.Run();
                    //Console.WriteLine("AllStock2 created");
                }

                if (args[0] == "-BonanzaExport2")
                {

                    FullCatalog2.Run();
                    Logger.Info("FullCatalog2 created");

                    FullCatalog2.UpdateFullCatalogImages();
                    FullCatalog2.LoadImagesToFullCatalogFromImagesFolder();
                    Logger.Info("FullCatalog2 images updated");

                    AllStockExporter2.Run();
                    Logger.Info("AllStock2 created");
                    
                    BonanzaExporter2.Run();
                    Logger.Info("Bonanza2 created");
                }

                if (args[0] == "-OnlyBonanzaExport2")
                {
                    BonanzaExporter2.Run();
                    Logger.Info("Bonanza2 created");
                }

                if (args[0] == "-ValidateAllShops")
                {
                    ValidatorAllBrands.ValidateAllShops();
                }

                if (args[0] == "-DbCreate")
                {
                    var db = new DbController();
                    db.Create();
                    Logger.Info("DB created");
                }

                if (args[0] == "-DbController.DownloadImages")
                {
                    DbController.DownloadImages();
                }

                if (args[0] == "-BonanzaExport3OnlyBonanza")
                {
                    BonanzaExporterAllBrands bnnzAllBrands = new BonanzaExporterAllBrands();
                    bnnzAllBrands.Run();
                }

                if (args[0] == "-BonanzaExport3")
                {
                    ValidatorAllBrands.ValidateAllShops();

                    DbController db = new DbController();
                    db.Create();
                    Logger.Info("DB created");

                    DbController.DownloadImages();                   

                    BonanzaExporterAllBrands bnnzAllBrands = new BonanzaExporterAllBrands();
                    bnnzAllBrands.Run();                 
                }

                if (args[0] == "-sivas_sale_parser")
                {
                    CurrencyRate currate = new CurrencyRate();
                    currate.Run();
                    SivasSaleParser sivasSale = new SivasSaleParser();
                    sivasSale.Run();
                }

                if (args[0] == "-basketshop")
                {
                    BasketShopParser basketShopParser = new BasketShopParser();
                    basketShopParser.Update();
                }

                if (args[0] == "-einhalb")
                {
                    //BasketShopParser basketShopParser = new BasketShopParser();
                    EinhalbParser einhalpParser = new EinhalbParser();
                    //QueensParser queensParser = new QueensParser();
                    //basketShopParser.Update();
                    einhalpParser.Update();
                    //queensParser.Update();
                }

                if (args[0] == "-queens")
                {
                    //BasketShopParser basketShopParser = new BasketShopParser();
                    //EinhalbParser einhalpParser = new EinhalbParser();
                    QueensParser queensParser = new QueensParser();
                    //basketShopParser.Update();
                    //einhalpParser.Update();
                    queensParser.Update();
                }

                if (args[0] == "-basketshop_einhalb_queens")
                {
                    BasketShopParser basketShopParser = new BasketShopParser();
                    EinhalbParser einhalpParser = new EinhalbParser();
                    QueensParser queensParser = new QueensParser();
                    basketShopParser.Update();
                    einhalpParser.Update();
                    queensParser.Update();
                }

                if (args[0] == "-streetbeat")
                {
                    //парсится 1 минуту
                    StreetBeatParser strbeatParse = new StreetBeatParser();
                    strbeatParse.Update();
                }

                if (args[0] == "-sneakersnstuff")
                {
                    //70 минут парсится
                    SneakersnstuffParser sneakerStuff = new SneakersnstuffParser();
                    sneakerStuff.Run();
                }

                if (args[0] == "-SivasAllBrands")
                {
                    SivasParserAB siavParserAb = new SivasParserAB();
                    siavParserAb.Run();
                }

                if (args[0] == "-overkillshop")
                {
                    //14 минут парсится
                    OverkillshopParser overkillshop = new OverkillshopParser();
                    overkillshop.Run();
                }

                if (args[0] == "-afew")
                {

                    AfewStoreParser afew = new AfewStoreParser();
                    afew.Run();
                }

                if (args[0] == "-suppa")
                {
                    //7 минут парсится
                    SuppaStoreParser suppa = new SuppaStoreParser();
                    suppa.Run();
                }

                if (args[0] == "-chmielna")
                {
                    //3 минуты парсится
                    ChmielnaParser chmielna = new ChmielnaParser();
                    chmielna.Run();
                }

                if (args[0] == "-viktor")
                {
                    SivasParser sivas = new SivasParser();
                    sivas.Run2();
                    SolehavenParser solehaven = new SolehavenParser();
                    solehaven.Run();
                    TitoloParser titolo = new TitoloParser();
                    titolo.Run2();
                    BdgastoreParser bdgastore = new BdgastoreParser();
                    bdgastore.Run();
                    AsphaltgoldParser asfaltgold = new AsphaltgoldParser();
                    asfaltgold.Run();

                    NashStockParser nashStockParser = new NashStockParser();
                    nashStockParser.Run();
                    DiscontSamaraParser discont = new DiscontSamaraParser();
                    discont.Run();
                }

                if (args[0] == "-upcdb")
                {
                    for (var i = 0; i < 300; i++)
                    {
                        var upcdb = new UPCDB();
                        upcdb.run3(10);
                    }
                }

                if (args[0] == "-UpdateFullCatalogImages")
                {
                    FullCatalog2.UpdateFullCatalogImages();
                    FullCatalog2.LoadImagesToFullCatalogFromImagesFolder();
                }

                if (args[0] == "-HotOffers")
                {
                    //HotOffers.Run();
                    FullCatalog2.PostNewTodayItems();
                    HotOffers.Run2(5);                    
                }

                if (args[0] == "-NewToday")
                {
                    FullCatalog2.PostNewTodayItems();
                }

                if (args[0] == "-DownloadImage")
                {
                    var imageDownload = new ImageDownload();
                    imageDownload.Run();
                }

                if (args[0] == "-DownloadImageForStock")
                {
                    var imageDownload = new ImageDownload();
                    try
                    {

                        Logger.Debug("Введите номер стока");
                        var numberString = Console.ReadLine();
                        Logger.Debug("Номер стока " + numberString);

                        if (numberString == null)
                        {
                            throw new NullReferenceException("numberString = null");
                        }

                        var number = int.Parse(numberString);

                        imageDownload.RunForStock(number);
                    }
                    catch (Exception e )
                    {
                        Logger.Error(e, "Ошибка при распознавании номера стока");
                    }
                    
                }

                if (args[0] == "-Avito")
                {
                    AvitoExporter3.Run();
                }

                if (args[0] == "-AvitoDiscontSamaraToSamara")
                {
                    AvitoDiscontSamaraToSamara.Run();
                }

                if (args[0] == "-PostHotOfferVk")
                {
                    var rnd = new Random().Next(61); //случайная задержка в минутах
                    Logger.Info("Задержка: " + rnd);
                    Thread.Sleep(rnd*60*1000);
                    HotOffers.PostVkRandomOffer();

                }

                if (args[0] == "-DiscontParsing")
                {
                    //из файла самарского дисконта делаем лист стокрекорд
                    //csv файл должен быть в формате utf-8 и разделен точкой с запятой
                    DiscontStock dStock = new DiscontStock();
                    dStock.ParseStockOnHand(Config.GetConfig().DirectoryPath + @"Parsing\discont\Stock On Hand.csv");
                    dStock.SaveStockToCSV(Config.GetConfig().DirectoryPath + @"Parsing\discont\StockDiscont.csv");
                    dStock.ParseStockOnHand2(Config.GetConfig().DirectoryPath + @"Parsing\discont\Stock On Hand.csv");
                    dStock.SaveStockToCSV2(Config.GetConfig().DirectoryPath + @"Parsing\discont\StockDiscont2.csv");
                }

                if (args[0] == "-AllBrandsDownloadImages")
                {
                    DbController.DownloadImages();
                }

                if (args[0] == "-debug")
                {
                    var lodkiLarisa = new LarisaLodki();
                    var lodki = lodkiLarisa.ParseLodki();

                    //var upwork = new UpworkCarelulu();
                    //upwork.Run();

                    //PortfolioMobiInterestComUpworkParser.Run();

                    //var d = Parser.GetHtmlPagePhantomJs("http://httpbin.org/ip");

                    //SizeChartAllBrands sizeChartAllBrands = new SizeChartAllBrands();
                    //sizeChartAllBrands.LoadSizeChartFromFtp();

                    //ShopValidatorAllBrands validator = new ShopValidatorAllBrands();
                    //validator.ValidateShop("snkrs.com");

                    //MyTelegram.PostMessageWaitRelease("test");
                    //BonanzaExporterAllBrands bnnzAllBrands = new BonanzaExporterAllBrands();
                    //bnnzAllBrands.Run();

                    //AvitoDiscontSamaraToSamara.Run();

                    //ValidatorAllBrands.ValidateAllShops();
                    //DbController.Update();


                    //var upcdb = new upcdb();
                    //upcdb.run2();

                    //Test.Crawlera();
                    //Test.Upcdb();

                    //AllStockExporter2.Run();

                    //FullCatalog.CleanFullCatalog();

                    //CreateCatalog createCatalog = new CreateCatalog();
                    //Console.WriteLine("FullCatalog created");

                    //FullCatalog.SaveCatalogToJson();

                    //FullCatalog2.Run();
                    //FullCatalog2.DeleteDuplicateItems();
                    //FullCatalog2.UpdateFullCatalogImages();

                    //NashStockParser nashStockParser = new NashStockParser();
                    //nashStockParser.Run();


                    //HotOffers.Run2(1);

                    //FullCatalog2.Run();
                    //Console.WriteLine("FullCatalog2 created");
                    //AllStockExporter2.Run();
                    //Console.WriteLine("AllStock2 created");

                    //FullCatalog2.PostNewTodayItems();

                    //AllStockExporter2.TestForDima();

                    // var imageDownload = new ImageDownload();
                    //imageDownload.Run();

                    //Helper.TelegramPost("test message", "-1001116441282");

                    //AvitoExporter3.SaveToXml(null);


                    //AvitoExporter3.Run();

                    //BonanzaExporter2.Run();
                    //BonanzaExporter2.Run2_MultiListing();

                    //Validator.ValidateAllShops();

                    //EinhalbParser einhalpParser = new EinhalbParser();
                    //einhalpParser.Run();

                    //Test.SortAllStock();

                    //Test.PostVk();

                    //FullCatalog2.DetectCategoryFromTitleAndUpdateFullCatalog();

                    //var hotOffers = HotOffers.CreateHotOffers();

                    //HotOffers.PostVkRandomOffer();

                    //Test.LoadVkGoods();
                    //FullCatalog2.LoadImagesToFullCatalogFromImagesFolder();

                    //SneakersnstuffParserAllBrands snsAllBrands = new SneakersnstuffParserAllBrands();
                    //snsAllBrands.ParseOneSneaker("https://www.sneakersnstuff.com/en/product/25821/reebok-question-lux");
                    //snsAllBrands.Run();
                }
            }
            else
            {
                Logger.Warn("Программа запущена без параметров");
            }


            //SivasParser sivas = new SivasParser();
            //sivas.Update();

            //TitoloParser titolo = new TitoloParser();
            //titolo.Run();

            //basketShopParser.Run();
            //einhalpParser.Run();
            //queensParser.Run();      
            //strbeatParse.Run();
            //sivas.Run();
            

            //DiscontStock dStock = new DiscontStock();
            //dStock.ReadStockFromCSV();
            //dStock.AddSaleToRecords();
            //dStock.SaveStockToCSV();

            //из файла кузьминок делает лист стокрекорд
            //DiscontStock dStock = new DiscontStock();
            //dStock.ParseStockOnHand(@"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Parsing\discont_msk_kuzminki\Stock On Hand.csv");
            //dStock.SaveStockToCSV(@"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Parsing\discont_msk_kuzminki\StockDiscont.csv");
            //dStock.ParseStockOnHand2(@"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Parsing\discont_msk_kuzminki\Stock On Hand.csv");
            //dStock.SaveStockToCSV2(@"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Parsing\discont_msk_kuzminki\StockDiscont2.csv");

            //из файла самарского дисконта делаем лист стокрекорд
            //csv файл должен быть в формате utf-8 и разделен точкой с запятой
            //DiscontStock dStock = new DiscontStock();
            //dStock.ParseStockOnHand(Settings.DIRECTORY_PATH + @"Parsing\discont\Stock On Hand.csv");
            //dStock.SaveStockToCSV(Settings.DIRECTORY_PATH + @"Parsing\discont\StockDiscont.csv");
            //dStock.ParseStockOnHand2(Settings.DIRECTORY_PATH + @"Parsing\discont\Stock On Hand.csv");
            //dStock.SaveStockToCSV2(Settings.DIRECTORY_PATH + @"Parsing\discont\StockDiscont2.csv");

            //string StockDiscont2 = @"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Parsing\discont\StockDiscont2.csv";
            //var discont = new DiscontStock();
            //discont.ReadStockFromCSV2(StockDiscont2);
            //bool isTrue = discont.AddStandtoRecords();
            //if (isTrue) discont.SaveStockToCSV2(StockDiscont2);

            //AmazonExporter amazonExporter = new AmazonExporter();
            //amazonExporter.run2();

            //AmazonExporter amazonExporter = new AmazonExporter();
            //amazonExporter.run3();


            //AmazonExporter amazonExporter = new AmazonExporter();
            //amazonExporter.run3();
            

            //BonanzaExporterAndrew bonanzaAndrew = new BonanzaExporterAndrew();
            //bonanzaAndrew.Run();

            //AvitoExporter avito = new AvitoExporter();

            //AvitoExporter2 avito2 = new AvitoExporter2();
            //avito2.Run();

            //Test test = new Test();

            //WithoutPhotoExporter withoutPhotoExporter = new WithoutPhotoExporter();


            //C:\SneakerIcon\repository\SneakerIcon\ConsoleApplication1\Classes\Utils\NetworkUtils.cs

            //SizeConverter sc = new SizeConverter("8", "Мужская");
            //string sizeEUR = sc.sizeEUR;

            //NashStock nashStock = new NashStock();
            //nashStock.ReadStockFromCSV();



            //var upcdb = new UPCDB();
            //upcdb.run2();

            //var addPhotos = new AddPhotos();
            //addPhotos.Run();

            //ModemReboot.Run();

            //SizeConverter.GenerateNikeSizeChart();

            if (args.Count() > 0)
            {
                Logger.Info("Параметр:" + args[0]);
            }
            var endTime = DateTime.Now;
            var dlit = endTime - startTime;
            Logger.Info("Начало выполнения программы: " + startTime);
            Logger.Info("Конец выполения программы: " + endTime);
            Logger.Info("Длительность выполения, минут: " + dlit.TotalMinutes);
            Logger.Info("Выполнение программы завершено, нажмите любую клавишу");

            #if DEBUG
                Console.ReadLine();
            #elif Feanor
                Console.ReadLine();
            #else

            #endif

        }
    }
}
