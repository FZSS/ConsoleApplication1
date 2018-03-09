using CsvHelper;
using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Catalogs
{
    public class Catalog
    {
        public string Name { get; set; }
        public string FileNameCatalog { get; set; }
        public string FileNameStock { get; set; }
        public List<Sneaker> sneakers { get; set; }
        public Stock stock { get; set; }

        public Catalog()
        {
            sneakers = new List<Sneaker>();
            stock = new OnlineShopStock();
        }

        public Catalog(string FileName)
        {
            sneakers = new List<Sneaker>();
            stock = new OnlineShopStock();
            //this.Name = Name;
            this.FileNameCatalog = FileName;
            ReadCatalogFromCSV(this.FileNameCatalog);
        }

        public Catalog(string FileNameCatalog, string FileNameStock)
        {
            sneakers = new List<Sneaker>();
            stock = new OnlineShopStock(FileNameStock);
            //this.Name = Name;
            this.FileNameCatalog = FileNameCatalog;
            this.FileNameStock = FileNameStock;
            ReadCatalogFromCSV(this.FileNameCatalog);
            //ReadStockFromCSV(FileNameStock);
        }

        public void ReadCatalogFromFtpJson()
        {

        }

        public void ReadStockFromCSV(string FileNameStock)
        {
            stock.ReadStockFromCSV(FileNameStock);
        }

        public void ReadCatalogFromCSV(string filename)
        {
            using (var sr = new StreamReader(filename, Encoding.GetEncoding(1251)))
            {
                var reader = new CsvReader(sr);

                reader.Configuration.Delimiter = ";";

                IEnumerable<CatalogRecord> records = reader.GetRecords<CatalogRecord>();

                foreach (var record in records)
                {
                    Sneaker sneaker = new Sneaker(record);
                    sneakers.Add(sneaker);
                }
            }
        }

        public int GetIndexFromSKU(string sku)
        {
            if (sku == null) throw new Exception("sku = null");
            for (int i = 0; i < sneakers.Count; i++)
            {
                if (sku == sneakers[i].sku)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetIndexFromLink(string link)
        {
            if (link == null) throw new Exception("link = null");
            for (int i = 0; i < sneakers.Count; i++)
            {
                if (link == sneakers[i].link)
                {
                    return i;
                }
            }
            return -1;
        }

        public Sneaker GetSneakerFromLink(string link)
        {
            int index = GetIndexFromLink(link);
            if (index == -1)
            {
                return null;
            }
            else
            {
                return @sneakers[index];
            }
        }

        public Sneaker GetSneakerFromSKU(string sku)
        {
            if (sku == null) throw new Exception("sku = null");
            for (int i = 0; i < sneakers.Count; i++)
            {
                if (sku == sneakers[i].sku)
                {
                    return @sneakers[i];
                }
            }
            return null;
        }

        public void SaveCatalogToCSV(string filename)
        {
            string filenameCatalog = filename;
            int count = sneakers.Count;
            List<CatalogRecord> catalog = new List<CatalogRecord>();
            for (int i = 0; i < count; i++)
            {
                CatalogRecord record = new CatalogRecord(sneakers[i]);
                catalog.Add(record);
            }
            using (var sw = new StreamWriter(filenameCatalog, false, Encoding.GetEncoding(1251)))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                writer.WriteRecords(catalog);
            }
        }

        public bool isExistSneakerInCatalog(Sneaker sneaker)
        {
            if (sneaker.sku == null) //если sku null то ищем дубликаты по ссылке
            {
                if (GetIndexFromLink(sneaker.link) == -1)
                {
                    return false;
                }
            }
            else //если есть sku то ищем дубликаты по нему
                if (GetIndexFromSKU(sneaker.sku) == -1)
                {
                    return false;
                }
            return true;
        }

        public bool isNotExistSneakerInCatalog(Sneaker sneaker)
        {
            return !isExistSneakerInCatalog(sneaker);
        }

        public void AddUniqueSneaker(Sneaker sneaker)
        {
            //if (Validator.ValidateSku(sneaker.sku, sneaker.brand))
            //{
                if (!this.isExistSneakerInCatalog(sneaker))
                    this.sneakers.Add(sneaker);
            //}
            //else
            //{
                //Program.Logger.Warn("Catalog.AddUniqueSneaker. Artikul is Invalid. sku: " + sneaker.sku);
            //}
        }

        public void SaveStockToCSV(string FileName)
        {
            string filenameCatalog = FileName;
            int count = this.sneakers.Count;
            var stock = new OnlineShopStock();
            //List<StockRecord> stock = new List<StockRecord>();
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < this.sneakers[i].sizes.Count; j++)
                {
                    StockRecord record = new StockRecord();
                    Sneaker sneaker = this.sneakers[i];
                    record.sku = sneaker.sku;
                    record.upc = sneaker.sizes[j].upc;
                    record.title = sneaker.title;
                    record.price = sneaker.price;
                    record.oldPrice = sneaker.oldPrice;
                    record.sellPrice = sneaker.sellPrice;
                    record.quantity = 1;
                    record.link = sneaker.link;
                    record.size = sneaker.sizes[j].sizeUS;
                    stock.records.Add(record);
                }
            }
            stock.SaveStockToCSV(FileName);
        }

        public void RemoveDuplicate()
        {
            List<Sneaker> newSneakers = new List<Sneaker>();
            for (int i = 0; i < sneakers.Count; i++)
            {
                var sneaker = sneakers[i];
                if (sneaker.sku == null) throw new Exception("null sku");
                else
                {
                    if (GetIndexFromSKU(sneaker.sku) == i) newSneakers.Add(sneaker);
                }
            }
            sneakers = newSneakers;
        }

        public void RemoveDuplicateSizesFromSneakers()
        {
            foreach (var sneaker in sneakers)
            {
                sneaker.DeleteDuplicateSizes();
            }
        }
    }
}
