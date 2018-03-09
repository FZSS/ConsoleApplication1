using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Parsing;
using SneakerIcon.Classes.Stocks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Catalogs
{
    public class CreateCatalog
    {
        public FullCatalog fullCatalog = new FullCatalog();
        public BasketShopCatalog basketShopCatalog = new BasketShopCatalog();
        public Catalog einhalbCatalog = new Catalog(EinhalbParser.CATALOG_FILENAME);
        public Catalog streetBeatCatalog = new Catalog(StreetBeatParser.CATALOG_FILENAME);
        public Catalog queensCatalog = new Catalog(QueensParser.CATALOG_FILENAME);
        public Catalog sivasCatalog = new Catalog(SivasParser.CATALOG_FILENAME);
        public Catalog titoloCatalog = new Catalog(TitoloParser.CATALOG_FILENAME);
        public Catalog sneakerstuffCatalog = new Catalog(SneakersnstuffParser.CATALOG_FILENAME);
        public Catalog overkillCatalog = new Catalog(OverkillshopParser.CATALOG_FILENAME);
        public Catalog solehaven = new Catalog(SolehavenParser.CATALOG_FILENAME);
        public Catalog afew = new Catalog(AfewStoreParser.CATALOG_FILENAME);
        public Catalog suppa = new Catalog(SuppaStoreParser.CATALOG_FILENAME);
        public Catalog chmielna = new Catalog(ChmielnaParser.CATALOG_FILENAME);
        public Catalog bdgastore = new Catalog(BdgastoreParser.CATALOG_FILENAME);
        //public Catalog asfaltgold = new Catalog(AsphaltgoldParser.CATALOG_FILENAME);
        public NashStock nashStock = new NashStock();
        public DiscontStock discontStock = new DiscontStock();
        public DiscontStock discontKuzminki = new DiscontStock();
        public CreateCatalog()
        {
            nashStock.ReadStockFromCSV(Config.GetConfig().NashStockFilename);
            
            //discontStock.ReadStockFromCSV2(@"C:\Users\Администратор\YandexDisk\sneaker-icon\Parsing\discont\StockDiscont2.csv");
            discontStock.ReadStockFromCSV2(Config.GetConfig().DirectoryPathParsing + @"discont\StockDiscont2.csv");
            //discontKuzminki.ReadStockFromCSV2(@"C:\Users\Администратор\YandexDisk\sneaker-icon\Parsing\discont_msk_kuzminki\StockDiscont2.csv");
            discontKuzminki.ReadStockFromCSV2(Config.GetConfig().DirectoryPathParsing + @"discont_msk_kuzminki\StockDiscont2.csv");
            MergeCatalog();
            //DeleteWrongLinks();
            fullCatalog.SaveCatalogToCSV(fullCatalog.FileNameCatalog);
            //fullCatalog.SaveCatalogToJson();
        }

        public void DeleteWrongLinks()
        {
            foreach (var sneaker in fullCatalog.sneakers)
            {
                for (int i = 0; i < sneaker.images.Count; i++)
                {
                    if (sneaker.images[i].Contains("http://street-beat.ruhttp://"))
                    {
                        sneaker.images[i] = sneaker.images[i].Replace("http://street-beat.ruhttp://", "http://");
                    }

                }
            }
        }

        public void MergeCatalog()
        {
            //queens
            foreach (var stockSneaker in queensCatalog.sneakers)
            {
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //brand
                    if (!String.IsNullOrWhiteSpace(stockSneaker.brand))
                        fullCatalogSneaker.brand = stockSneaker.brand; //у queens более точно проставлены бренды
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //basketshop
            foreach (var stockSneaker in basketShopCatalog.sneakers)
            {
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //brand
                    if (!String.IsNullOrWhiteSpace(stockSneaker.brand))
                        fullCatalogSneaker.brand = stockSneaker.brand; //у баскетшопа более точно проставлены бренды
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //streetbeat
            foreach (var stockSneaker in streetBeatCatalog.sneakers)
            {
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //43einhalb
            foreach (var stockSneaker in einhalbCatalog.sneakers)
            {
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //sivas
            foreach (var stockSneaker in sivasCatalog.sneakers)
            {
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //titolo
            foreach (var stockSneaker in titoloCatalog.sneakers)
            {
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //sneakerstuff
            foreach (var stockSneaker in sneakerstuffCatalog.sneakers)
            {
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //overkill
            foreach (var stockSneaker in overkillCatalog.sneakers)
            {
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //solehaven
            foreach (var stockSneaker in solehaven.sneakers)
            {
                stockSneaker.images = new List<string>(); //чистим изображения с солихэвен
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //afe
            foreach (var stockSneaker in afew.sneakers)
            {
                stockSneaker.images = new List<string>(); //чистим изображения с солихэвен
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //suppastor
            foreach (var stockSneaker in suppa.sneakers)
            {
                stockSneaker.images = new List<string>(); //чистим изображения с солихэвен
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //chmielna
            foreach (var stockSneaker in chmielna.sneakers)
            {
                stockSneaker.images = new List<string>(); //чистим изображения с солихэвен
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //bdgastore
            foreach (var stockSneaker in bdgastore.sneakers)
            {
                stockSneaker.images = new List<string>(); //чистим изображения с солихэвен
                int index = fullCatalog.GetIndexFromSKU(stockSneaker.sku);
                if (index >= 0) //Если этот артикул уже есть в каталоге
                {
                    Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
                    MergeSneaker(fullCatalogSneaker, stockSneaker);
                    //Console.WriteLine("Артикул объединен: " + stockSneaker.sku);
                }
                else //если в каталоге этого артикула нет, то добавляем его
                {
                    bool isAdded = fullCatalog.AddMaskedSneaker(stockSneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockSneaker.sku);
                }
            }

            //discont 
            foreach (var stockRecord in discontStock.Records2)
            {
                int index = fullCatalog.GetIndexFromSKU(stockRecord.sku);
                if (index == -1) //если в каталоге этого артикула нет, то добавляем его
                {
                    Sneaker sneaker = new Sneaker();
                    sneaker.sku = stockRecord.sku;
                    sneaker.category = stockRecord.category;
                    if (sneaker.category == Settings.CATEGORY_MEN)
                    {
                        sneaker.sex = Settings.GENDER_MAN;
                    }
                    if (sneaker.category == Settings.CATEGORY_WOMEN)
                    {
                        sneaker.sex = Settings.GENDER_WOMAN;
                    }

                    //title 
                    sneaker.title = stockRecord.title;
                    sneaker.title = sneaker.title.Replace("*", "");
                    sneaker.title = sneaker.title.Replace("JRD", "JORDAN");

                    //brand
                    if (sneaker.title.Contains("JORDAN"))
                    {
                        sneaker.brand = "Jordan";
                    }
                    else
                    {
                        sneaker.brand = "Nike";
                    }

                    bool isAdded = fullCatalog.AddMaskedSneaker(sneaker);
                    //fullCatalog.sneakers.Add(sneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockRecord.sku);
                }
            }

            //discont kuzminki
            foreach (var stockRecord in discontKuzminki.Records2)
            {
                int index = fullCatalog.GetIndexFromSKU(stockRecord.sku);
                if (index == -1) //если в каталоге этого артикула нет, то добавляем его
                {
                    Sneaker sneaker = new Sneaker();
                    sneaker.sku = stockRecord.sku;
                    sneaker.category = stockRecord.category;
                    if (sneaker.category == Settings.CATEGORY_MEN)
                    {
                        sneaker.sex = Settings.GENDER_MAN;
                    }
                    if (sneaker.category == Settings.CATEGORY_WOMEN)
                    {
                        sneaker.sex = Settings.GENDER_WOMAN;
                    }
                    
                    //title 
                    sneaker.title = stockRecord.title;
                    sneaker.title = sneaker.title.Replace("*", "");
                    sneaker.title = sneaker.title.Replace("JRD", "JORDAN");
                    
                    //brand
                    if (sneaker.title.Contains("JORDAN"))
                    {
                        sneaker.brand = "Jordan";
                    }
                    else
                    {
                        sneaker.brand = "Nike";
                    }

                    bool isAdded = fullCatalog.AddMaskedSneaker(sneaker);
                    //fullCatalog.sneakers.Add(sneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockRecord.sku);
                }
            }

            //nashstock
            foreach (var stockRecord in nashStock.records)
            {
                int index = fullCatalog.GetIndexFromSKU(stockRecord.sku);
                if (index == -1) //если в каталоге этого артикула нет, то добавляем его
                {
                    Sneaker sneaker = new Sneaker();
                    sneaker.sku = stockRecord.sku;
                    sneaker.title = stockRecord.title;
                    sneaker.brand = stockRecord.brand;
                    sneaker.category = stockRecord.category;

                    bool isAdded = fullCatalog.AddMaskedSneaker(sneaker);
                    //fullCatalog.sneakers.Add(sneaker);
                    if (isAdded) Console.WriteLine("Артикул добавлен: " + stockRecord.sku);
                }
            }
        }

        public void MergeSneaker(Sneaker fullCatalogSneaker, Sneaker stockSneaker)
        {
            //title
            //Sneaker fullCatalogSneaker = @fullCatalog.sneakers[index];
            if (stockSneaker.title.Length > fullCatalogSneaker.title.Length)
                fullCatalogSneaker.title = stockSneaker.title;

            //color
            if (String.IsNullOrWhiteSpace(fullCatalogSneaker.color) && !String.IsNullOrWhiteSpace(stockSneaker.color))
                fullCatalogSneaker.color = stockSneaker.color;

            //collection
            if (String.IsNullOrWhiteSpace(fullCatalogSneaker.collection) && !String.IsNullOrWhiteSpace(stockSneaker.collection))
                fullCatalogSneaker.collection = stockSneaker.collection;

            //type
            if (String.IsNullOrWhiteSpace(fullCatalogSneaker.type) && !String.IsNullOrWhiteSpace(stockSneaker.type))
                fullCatalogSneaker.type = stockSneaker.type;

            //MergeParameter(fullCatalogSneaker.type, stockSneaker.type);
            //height
            if (String.IsNullOrWhiteSpace(fullCatalogSneaker.height) && !String.IsNullOrWhiteSpace(stockSneaker.height))
                fullCatalogSneaker.height = stockSneaker.height;

            //MergeParameter(fullCatalogSneaker.height, stockSneaker.height);
            //destination
            if (String.IsNullOrWhiteSpace(fullCatalogSneaker.destination) && !String.IsNullOrWhiteSpace(stockSneaker.destination))
                fullCatalogSneaker.destination = stockSneaker.destination;
            //MergeParameter(fullCatalogSneaker.destination, stockSneaker.destination);

            //MergeParameter(fullCatalogSneaker.description, stockSneaker.description);
            //description
            if (String.IsNullOrWhiteSpace(fullCatalogSneaker.description) && !String.IsNullOrWhiteSpace(stockSneaker.description))
                fullCatalogSneaker.description = stockSneaker.description;

            //MergeParameter(fullCatalogSneaker.categorySneakerFullCatalog, stockSneaker.categorySneakerFullCatalog);
            //categorySneakerFullCatalog
            if (String.IsNullOrWhiteSpace(fullCatalogSneaker.category) && !String.IsNullOrWhiteSpace(stockSneaker.category))
                fullCatalogSneaker.category = stockSneaker.category;

            //MergeParameter(fullCatalogSneaker.sex, stockSneaker.sex);
            //sex
            if (String.IsNullOrWhiteSpace(fullCatalogSneaker.sex) && !String.IsNullOrWhiteSpace(stockSneaker.sex))
                fullCatalogSneaker.sex = stockSneaker.sex;
       
            //MergeParameter(fullCatalogSneaker.link, stockSneaker.link);
            //link
            if (String.IsNullOrWhiteSpace(fullCatalogSneaker.link) && !String.IsNullOrWhiteSpace(stockSneaker.link))
                fullCatalogSneaker.link = stockSneaker.link;

            //images
            if (fullCatalogSneaker.images.Count == 1 && stockSneaker.images.Count > 1)
            {
                if (String.IsNullOrWhiteSpace(fullCatalogSneaker.images[0]))
                {
                    fullCatalogSneaker.images = stockSneaker.images;
                }
            }
            if (fullCatalogSneaker.images == null)
            {
                bool test = true;
            }
        }

        public void MergeParameter(string fullCatalogSneakerParam, string @stockCatalogSneakerParam)
        {
            if (String.IsNullOrWhiteSpace(fullCatalogSneakerParam) && !String.IsNullOrWhiteSpace(stockCatalogSneakerParam))
                fullCatalogSneakerParam = stockCatalogSneakerParam;
        }

        public void AddImagesFromFtpServer()
        {

        }
    }
}
