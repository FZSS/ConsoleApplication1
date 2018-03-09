using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Parsing;

namespace SneakerIcon.Classes
{
    public class Sneaker
    {
        public string style { get; set; }
        public string colorcode { get; set; }
        public string sku { get; set; }
        public string title { get; set; }
        public string color { get; set; }
        public string brand { get; set; }
        public string collection { get; set; }
        /// <summary>
        /// Тип обуви: кроссовки, кеды, слипоны и т.д.
        /// </summary>
        public string type { get; set; }
        public string height { get; set; }
        /// <summary>
        /// для чего: баскетбольные, футбол, повседневные и т.д.
        /// </summary>
        public string destination { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public string sex { get; set; }
        public string link { get; set; }
        public double price { get; set; }
        /// <summary>
        /// Цена без учета распродажи. Если есть распродажа есть старая цена
        /// </summary>
        public double oldPrice { get; set; }
        public double sellPrice { get; set; }
        public List<SneakerSize> sizes { get; set; }
        public List<String> images { get; set; }
        public List<string> sizeListUS { get; set; }
        public List<string> sizeListEU { get; set; }
        public List<string> sizeListUK { get; set; }
        public List<string> sizeListCM { get; set; }
        public List<string> sizeListRU { get; set; }
        public List<StockRecord> StockRecords { get; set; }

        public Sneaker()
        {
            Initialization();
        }

        public Sneaker(CatalogRecord record)
        {
            Initialization();       
            GetSneakerFromCatalogRecord(record);
        }

        public void GetSneakerFromCatalogRecord(CatalogRecord record) {
            this.style = record.style;
            this.colorcode = record.colorcode;
            this.sku = record.sku.Trim();
            this.title = record.title;
            //TextInfo ti = new CultureInfo("en-us", false).TextInfo;
            //this.title = ti.ToTitleCase(record.title.ToLower()); //перевожу название В Верхние Регистр Первая Буква Слова Для Яндекс Маркета
            this.color = record.color;
            this.brand = record.brand;
            this.collection = record.collection;
            this.type = record.type;
            this.height = record.height;
            this.destination = record.destination;
            this.description = record.description;
            this.category = record.category;
            this.sex = record.sex;
            this.link = record.link;
            double price;
            double.TryParse(record.price, out price);
            this.price = price;
            double.TryParse(record.oldPrice, out price);
            this.oldPrice = price;
            this.images = record.images.Split('|').ToList();
            //this.price = Int32.Parse(record.streetbeat);
            
        }

        private void Initialization()
        {
            sizes = new List<SneakerSize>();
            images = new List<string>();
            StockRecords = new List<StockRecord>();
        }

        public int FindSize(string sizeUS)
        {
            for (int i = 0; i < this.sizes.Count; i++)
            {
                if (this.sizes[i].sizeUS == sizeUS)
                {
                    return i;
                }
            }
            return -1;
        }

        public SneakerSize GetSize(string sizeUS)
        {
            int index = FindSize(sizeUS);
            if (index == -1)
            {
                return null;
            }
            else
            {
                return @sizes[index];
            }
        }

        public StockRecord GetStockRecord(StockRecord stockRecord)
        {
            for (int i = 0; i < this.StockRecords.Count; i++)
            {
                if (StockRecords[i].sku == stockRecord.sku && StockRecords[i].size == stockRecord.size)
                {
                    return StockRecords[i];
                }
            }
            return null;
        }

        public void SetSizesList()
        {
            this.sizeListUS = new List<string>();
            this.sizeListUK = new List<string>();
            this.sizeListEU = new List<string>();
            this.sizeListCM = new List<string>();
            this.sizeListRU = new List<string>();

            for (int i = 0; i < this.sizes.Count; i++)
            {
                var sneakerSize = this.sizes[i];
                sizeListUS.Add(sneakerSize.sizeUS);
                sizeListUK.Add(sneakerSize.sizeUK);
                sizeListEU.Add(sneakerSize.sizeEU);
                sizeListCM.Add(sneakerSize.sizeCM);
                sizeListRU.Add(sneakerSize.sizeRU);
            }
        }

        public List<String> GetImagesStringForSneakerIcon()
        {
            string url = "http://photo.sneakersfamily.ru/";
            int count = 10;
            //if (this.images.Count > 0)
            //{
            //    count = this.images.Count;
            //}
            //else
            //{
            //    count = 10;
            //}
            this.images = new List<string>();
            for (int i = 1; i < count+1; i++)
            {
                this.images.Add(url + sku + "-" + i + ".jpg");
            }
            return this.images;
        }

        public String GetVozrast()
        {
            if (this.category == "Детская") return "";
            if (this.category == "" || this.category == "") return "";
            throw new Exception("");
        }

        /// <summary>
        /// Убирает из названия бренд (Nike, Jordan) и тип обуви (Кроссовки, кеды, и т.д.)
        /// </summary>
        /// <param ftpFolder="stockSneaker"></param>
        public void ParseTitle()
        {
            ParseType();
            ParseCollection();
            ParseHeight();
            ParseCategory();
            this.title = this.title.Trim().Replace("  ", " ");
        }

        private void ParseCategory()
        {
            if (title.ToLower().Contains("Десткие"))
                this.title = title.Replace("Десткие", "");
        }

        private void ParseCollection()
        {
            string parseCollection = "air force 1";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "air max";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "hyperdunk";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "huarache";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "roshe";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "presto";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "free";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "zoom";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "air jordan";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "kobe";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "benassi";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "tennis classic";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "lebron";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();

            parseCollection = "internationalist";
            if (title.ToLower().Contains(parseCollection))
                this.collection = parseCollection.ToUpper();
        }

        public void ParseType()
        {
            string type = "Кроссовкие";
            if (this.title.Contains(type))
            {
                this.type = "Кроссовки";
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Кроссовки";
            if (this.title.Contains(type))
            {
                this.type = type;
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Кроссоки";
            if (this.title.Contains(type))
            {
                this.type = "Кроссовки";
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Ботинки";
            if (this.title.Contains(type))
            {
                this.type = type;
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Шлёпанцы";
            if (this.title.Contains(type))
            {
                this.type = "Сланцы";
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Сланцы";
            if (this.title.Contains(type))
            {
                this.type = "Сланцы";
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Сапоги";
            if (this.title.Contains(type))
            {
                this.type = type;
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Кеды";
            if (this.title.Contains(type))
            {
                this.type = type;
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Сандалии";
            if (this.title.Contains(type))
            {
                this.type = type;
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Сандали";
            if (this.title.Contains(type))
            {
                this.type = "Сандалии";
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Пантолеты";
            if (this.title.Contains(type))
            {
                this.type = "Сланцы";
                this.title = this.title.Replace(type, "").Trim();
            }

            type = "Слипоны";
            if (this.title.Contains(type))
            {
                this.type = type;
                this.title = this.title.Replace(type, "").Trim();
            }
        }

        public void ParseHeight()
        {
            string height = "высокие";
            if (this.title.Contains(height))
            {
                this.height = "Высокие";
                this.title = this.title.Replace(height, "").Trim();
            }

            height = "низкие";
            if (this.title.Contains(height))
            {
                this.height = "Низкие";
                this.title = this.title.Replace(height, "").Trim();
            }

            height = "низкая";
            if (this.title.Contains(height))
            {
                this.height = "Низкие";
                this.title = this.title.Replace(height, "").Trim();
            }
        }

        public void AddDestination(string newDestination)
        {
            if (String.IsNullOrWhiteSpace(this.destination))
            {
                this.destination = newDestination;
            }
            else
            if (!this.destination.Contains(newDestination))
            {
                List<string> destinationList = new List<string>();
                    destinationList = this.destination.Split('|').ToList();
                    destinationList.Add(newDestination);
                    this.destination = String.Join("|", destinationList);
            }
        }

        public String GetImageString()
        {
            return String.Join("|", this.images);
        }

        public void DeleteDuplicateSizes()
        {
            List<SneakerSize> newSizes = new List<SneakerSize>();
            for (int i = 0; i < sizes.Count; i++)
            {
                var size = sizes[i];
                int index = FindSize(size.sizeUS);
                if (index == i) newSizes.Add(size);
            }
            sizes = newSizes;
        }

        public void SortSizes()
        {
            var sneaker = this;
            SneakerSize minSize;
            for (int ii = 0; ii < sneaker.sizes.Count; ii++)
            {
                for (int jj = ii + 1; jj < sneaker.sizes.Count; jj++)
                {
                    Double sizeI = 0;
                    Double sizeJ = 0;

                    sizeI = Double.Parse(sneaker.sizes[ii].sizeEU.Replace('.', ','));
                    sizeJ = Double.Parse(sneaker.sizes[jj].sizeEU.Replace('.', ','));


                    if (sizeJ < sizeI)
                    {
                        minSize = sneaker.sizes[jj];
                        sneaker.sizes[jj] = sneaker.sizes[ii];
                        sneaker.sizes[ii] = minSize;
                    }
                }
            }
        }

        public void AddSize(string sizeUS, int quantity, string UPC = null)
        {
            SneakerSize size = new SneakerSize(sizeUS);
            if (!String.IsNullOrWhiteSpace(UPC))
            {
                size.upc = UPC;
            }
            if (quantity > 0)
            {
                size.quantity = quantity;                
            }
            else
            {
                throw new Exception("попытка добавить товар с нулевым количеством");
            }
            this.sizes.Add(size);
        }
    }
}

