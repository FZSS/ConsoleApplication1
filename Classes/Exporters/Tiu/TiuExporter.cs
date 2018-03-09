using CsvHelper;
using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SneakerIcon.Classes.Exporters
{
    public class TiuExporter : Exporter
    {
        public static readonly string DIRECTORY_PATH = Exporter.DIRECTORY_PATH + @"Tiu\";
        public static readonly string FILENAME = DIRECTORY_PATH + "Tiu.csv";
        public static readonly string XML_FILENAME = DIRECTORY_PATH + "Tiu.yml";
        public const int marzha = 1500;
        List<TiuRecord> Records { get; set; }
        public TiuExporter()
            : base()
        {
            Records = new List<TiuRecord>();
        }

        public void Run()
        {
            CreateRecords();
            ExportToCSV(FILENAME);
            ExportToXML();
            RenameTitles(FILENAME);
            GoToFTP(FILENAME);
        }

        public void CreateRecords()
        {
            foreach (var stockRecord in NashStock1.records)
            {
                if (stockRecord.quantity > 0)
                {
                    var tiuRecord = new TiuRecord();
                    //tiuRecord.SetParameters(stockRecord);
                    tiuRecord.kod_tovara = stockRecord.sku + "-" + stockRecord.size;
                    tiuRecord.identifikator_tovara = tiuRecord.kod_tovara;
                    tiuRecord.id_gruppi_raznovidnostej = stockRecord.sku.Replace("-", "");
                    tiuRecord.znachenie_harakteristiki_razmer = stockRecord.size;
                    tiuRecord.proizvoditel = stockRecord.brand;
                    if (stockRecord.sellPrice != 0)
                    {
                        tiuRecord.cena = stockRecord.sellPrice.ToString();
                    }
                    else
                    {
                        int price = (int)stockRecord.price + marzha;
                        tiuRecord.cena = price.ToString();
                    }
                    Records.Add(tiuRecord);
                }
            }

            foreach (var Discont in DiscontStocks)
            {
                foreach (var stockRecord in Discont.records)
                {
                    if (stockRecord.quantity > 0)
                    {
                        string kod_tovara = stockRecord.sku + "-" + stockRecord.size;
                        //double price = GetPrice(stockRecord, "RUB");
                        var tiuRecord = @GetRecordFromId(kod_tovara);
                        if (tiuRecord == null)
                        {
                            tiuRecord = new TiuRecord();
                            tiuRecord.SetParameters(stockRecord);
                            int price = (int)stockRecord.price + marzha;
                            tiuRecord.cena = price.ToString();
                            //tiuRecord.id = id;
                            //tiuRecord.sizeUS = stockRecord.sizeUS;
                            //tiuRecord.upc = stockRecord.upc;
                            //tiuRecord.price = price.ToString();
                            Records.Add(tiuRecord);
                        }
                        else //если уже есть то добавляем только нужное
                        {
                            //if (String.IsNullOrWhiteSpace(tiuRecord.upc) && !String.IsNullOrWhiteSpace(stockRecord.upc))
                            //    tiuRecord.upc = stockRecord.upc;
                            int price = (int)stockRecord.price + marzha;
                            if (price < Int32.Parse(tiuRecord.cena))
                                tiuRecord.cena = price.ToString();
                        }
                    }
                }
            }

            foreach (var shopStock in ShopStocks)
            {
                foreach (var stockRecord in shopStock.records)
                {
                    double price = GetPrice(stockRecord, shopStock.Currency, shopStock.Marzha);
                    
                    
                    string kod_tovara = stockRecord.sku + "-" + stockRecord.size;
                    var tiuRecord = @GetRecordFromId(kod_tovara);

                    if (tiuRecord == null)
                    {
                            tiuRecord = new TiuRecord();
                            tiuRecord.SetParameters(stockRecord);
                            tiuRecord.cena = Math.Round(price, 0).ToString();
                            Records.Add(tiuRecord);
                    }
                    else //если уже есть то добавляем только нужное
                    {
                        if (price < Double.Parse(tiuRecord.cena))
                            tiuRecord.cena = Math.Round(price, 0).ToString();
                    }
                }
            }

            List<TiuRecord> newRecords = new List<TiuRecord>();
            foreach (var record in Records)
            {
                var sneaker = GetCatalogRecordFromId(record.kod_tovara);
                if (sneaker != null)
                {
                    //sneaker.brand = "Nike";
                    bool result = record.SetParametersFromSneaker(sneaker);
                    if (result)
                    {
                        var snSize = new SneakerSize();
                        result = snSize.GetAllSizesFromUS(sneaker, record.znachenie_harakteristiki_razmer);
                        if (result)
                        {
                            record.znachenie_harakteristiki_razmer = snSize.allSize + " / " + snSize.sizeRU + " RUS";
                            //record.znachenie_harakteristiki_razmerRUS = snSize.sizeRU.Replace(".",",");
                            //record.znachenie_harakteristiki_razmerRUS2 = snSize.sizeRU.Replace(".", ",");
                            //record.znachenie_harakteristiki_razmerUK = snSize.sizeUK.Replace(".", ",");
                            //record.znachenie_harakteristiki_razmerUS = record.znachenie_harakteristiki_razmerUS.Replace(".", ",");
                            //record.znachenie_harakteristiki_razmerCM = snSize.sizeCM.Replace(".", ",");
                            //record.znachenie_harakteristiki_razmerEUR = snSize.sizeEU.Replace(".", ",");
                            newRecords.Add(record);
                        }
                        else
                        {
                            Program.Logger.Warn("wrong size. sku:" + sneaker.sku + " category: " + sneaker.category + " size: " + record.znachenie_harakteristiki_razmer);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("record is not exist in catalog. id: " + record.kod_tovara);
                }
            }
            Records = newRecords;
        }

        /// <summary>
        /// получаем конечную цену (маржа и доставка уже включены)
        /// </summary>
        /// <param ftpFolder="price">себестоимость</param>
        /// <param ftpFolder="currency">валюта</param>
        /// <returns>возвращает конечную цену продажи</returns>
        public double GetPrice(StockRecord record, string currency, int Marzha = 0)
        {
            if (record.sku == "852395-018")
            {
                bool test = true;
            }

            if (currency == "RUB" && Marzha == 0)
            {
                Marzha = marzha; //это для дисконта, там без маржи вызов функции, тут исправил а в других экспортерах возможно это осталось
                //но возможно у каких-то магазинов тоже это осталось
            }
            if (currency == "RUB")
            {
                Marzha = marzha;
            }
            double price = Exporter.GetPrice(record, currency, "RUB", Marzha);
            return price;

            //double resultPrice;
            //if (currency == "USD")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice;
            //        return resultPrice * Settings.USD_SELL;
            //    }
            //    else
            //    {
            //        if (Marzha == 0)
            //            throw new Exception("marzha = 0. sku: " + record.sku);
            //        resultPrice = record.price + Marzha;
            //        return resultPrice * Settings.USD_BUY;
            //        //throw new Exception("нет цены продажи sku:" + record.sku);
            //    }
            //}
            //if (currency == "EUR")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice * Settings.EURO_BUY;
            //        return resultPrice;
            //    }
            //    else
            //    {
            //        if (Marzha == 0)
            //            throw new Exception("marzha = 0. sku: " + record.sku);
            //        record.sellPrice = record.price + Marzha;
            //        resultPrice = record.sellPrice * Settings.EURO_BUY;
            //        return resultPrice;
            //    }
            //}
            //if (currency == "CHF")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice * Settings.CHF_BUY;
            //        return resultPrice;
            //    }
            //    else
            //    {
            //        throw new Exception("нет цены продажи sku:" + record.sku);
            //    }
            //}
            //else if (currency == "RUB")
            //{
            //    var sneaker = catalog.GetSneakerFromSKU(record.sku);
            //    if (sneaker != null)
            //    {
            //        if (sneaker.type == "Сланцы")
            //        {
            //            resultPrice = record.price + marzha;
            //            return resultPrice;
            //        }
            //    }
            //    resultPrice = record.price + marzha;
            //    return resultPrice;
            //}
            //throw new NotImplementedException();
        }

        public Sneaker GetCatalogRecordFromId(string id)
        {
            string sku = id.Split('-')[0] + "-" + id.Split('-')[1];
            foreach (var sneaker in catalog.sneakers)
            {
                if (sku == sneaker.sku)
                    return sneaker;
            }
            Program.Logger.Warn("sku does not exist in catalog " + sku);
            return null;
        }

        public TiuRecord GetRecordFromId(string kod_tovara)
        {
            for (int i = 0; i < this.Records.Count; i++)
            {
                if (this.Records[i].kod_tovara == kod_tovara) return this.Records[i];
            }
            return null;
        }

        public void ExportToCSV(string filename)
        {

            //using (var sw = new StreamWriter(JSON_FILENAME, false, Encoding.GetEncoding(1251)))
            using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(Records);
            }
        }

        private void GoToFTP(string FILENAME)
        {
            // Создаем объект FtpWebRequest - он указывает на файл, который будет создан
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://s07.webhost1.ru/tiu.csv");
            // устанавливаем метод на загрузку файлов
            request.Method = WebRequestMethods.Ftp.UploadFile;
            //login pass
            request.Credentials = new NetworkCredential("amaro_update", "38273827");

            // создаем поток для загрузки файла
            FileStream fs = new FileStream(FILENAME.Replace("Tiu.csv", "TiuRus.csv"), FileMode.Open);
            byte[] fileContents = new byte[fs.Length];
            fs.Read(fileContents, 0, fileContents.Length);
            fs.Close();
            request.ContentLength = fileContents.Length;

            // пишем считанный в массив байтов файл в выходной поток
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            // получаем ответ от сервера в виде объекта FtpWebResponse
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }

        private void RenameTitles(string FILENAME)
        {
            FileStream fi = new FileStream(FILENAME, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            StreamReader rea = new StreamReader(fi);

            string str = rea.ReadLine();
            str = str.Replace("kod_tovara", "Код_товара");
            str = str.Replace("nazvanie_pozicii", "Название_позиции");
            str = str.Replace("kluchevie_slova", "Ключевые_слова");
            str = str.Replace("opisanie", "Описание");
            str = str.Replace("tip_tovara", "Тип_товара");
            str = str.Replace(";cena;", ";Цена;");
            str = str.Replace("valuta", "Валюта");
            str = str.Replace("edinica_izmerenia", "Единица_измерения");
            str = str.Replace("minimalnij_objem_zakaza", "Минимальный_объем_заказа");
            str = str.Replace("optovaya_cena", "Оптовая_цена");
            str = str.Replace("minimalnij_zakaz_opt", "Минимальный_заказ_опт");
            str = str.Replace("ssilka_izobrazheniya", "Ссылка_изображения");
            str = str.Replace("nalichie", "Наличие");
            str = str.Replace("kolichestvo", "Количество");
            str = str.Replace("nomer_gruppi", "Номер_группы");
            str = str.Replace("nazvanie_gruppi", "Название_группы");
            str = str.Replace("adres_podrazdela", "Адрес_подраздела");
            str = str.Replace("vozmozhnost_postavki", "Возможность_поставки");
            str = str.Replace("srok_postavki", "Срок_поставки");
            str = str.Replace("sbosob_upakovki", "Способ_упаковки");
            str = str.Replace("unikalnij_identifikator", "Уникальный_идентификатор");
            str = str.Replace("identifikator_tovara", "Идентификатор_товара");
            str = str.Replace("identifikator_podrazdela", "Идентификатор_подраздела");
            str = str.Replace("identifikator_gruppi", "Идентификатор_группы");
            str = str.Replace(";proizvoditel;", ";Производитель;");
            str = str.Replace("garantijnij_srok", "Гарантийный_срок");
            str = str.Replace("strana_proizvoditel", "Страна_производитель");
            str = str.Replace("skidka", "Скидка");
            str = str.Replace("id_gruppi_raznovidnostej", "ID_группы_разновидностей");
            str = str.Replace("nazvanie_harakteristiki_razmer", "Название_Характеристики");
            str = str.Replace("izmerenie_harakteristiki_razmer", "Измерение_Характеристики");
            str = str.Replace("znachenie_harakteristiki_razmer", "Значение_Характеристики");
            str += "\r\n";

            string str2 = rea.ReadToEnd();
            string str3 = str + str2;

            rea.Close();

            System.IO.File.WriteAllText(FILENAME.Replace("Tiu.csv","TiuRus.csv"), str3);
        }

        private void ExportToXML()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(@"AdditionalFiles/tiu.yml");
            // получим корневой элемент
            XmlElement yml_catalog = xDoc.DocumentElement;
            yml_catalog.SetAttribute("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            var Shop = yml_catalog.LastChild;
            var Offers = Shop.LastChild;
            //var Offer = Offers.FirstChild;
            xDoc.Save(XML_FILENAME);
        }
    }
}
