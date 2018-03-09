using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Catalogs
{
    public class FullCatalog : Catalog
    {
        public FullCatalog() : base()
        {
            this.Name = "FullCatalog";
            this.FileNameCatalog = Config.GetConfig().DirectoryPathParsing + @"FullCatalog.csv";
            ReadCatalogFromCSV(FileNameCatalog);
        }

        public FullCatalog(string filename)
            : base()
        {
            this.Name = "FullCatalog";
            this.FileNameCatalog = filename;
            ReadCatalogFromCSV(FileNameCatalog);
        }

        /// <summary>
        /// добавляет в каталог только кроссовки с правильной маской и брендом (пока что только Nike и Jordan с маской 123456-123
        /// </summary>
        /// <param ftpFolder="sneaker">Добавляемый кроссовок в каталог</param>
        /// <returns>возвращает true если кроссовок добавился и false если нет</returns>
        public bool AddMaskedSneaker(Sneaker sneaker) {
            if (sneaker.brand.ToLower() == "nike" || sneaker.brand.ToLower() == "jordan")
            {
                if (isTrueMaskSKUForNike(sneaker.sku))
                {
                    this.sneakers.Add(sneaker);
                    return true;
                }
                else
                {
                    Program.Logger.Warn("wrong sku:" + sneaker.sku);
                    return false;
                }
            }
            Program.Logger.Warn("wrong brand: " + sneaker.brand + " sku:" + sneaker.sku);
            return false;
        }

        public static bool isTrueMaskSKUForNike(string SKU)
        {
            string sPattern = "^\\d{6}-\\d{3}$";
            if (System.Text.RegularExpressions.Regex.IsMatch(SKU, sPattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void CleanFullCatalog()
        {
            //Очистим файл фулкаталога от левых артикулов
            FullCatalog fullcatalog = new FullCatalog();
            List<Sneaker> newSneakers = new List<Sneaker>();
            foreach (Sneaker sneaker in fullcatalog.sneakers)
            {
                //если маска артикула правильная 123456-123
                if (FullCatalog.isTrueMaskSKUForNike(sneaker.sku))
                {

                    //исправляем неправильные бренды
                    if (sneaker.brand.ToLower() != "nike" && sneaker.brand.ToLower() != "jordan")
                    {
                        if (sneaker.brand.ToLower() == "nike sportswear" || sneaker.brand.ToLower() == "nike sb")
                        {
                            sneaker.brand = "Nike";
                        }
                        else if (sneaker.title.ToUpper().Contains("NIKE"))
                        {
                            sneaker.brand = "Nike";
                        }
                        else
                        {
                            Program.Logger.Debug("wrong brand: " + sneaker.brand + " sku:" + sneaker.sku);
                        }
                    }

                    //исправляем заголовок
                    if (sneaker.title.ToUpper().Contains("NIKE NIKE"))
                    {
                        sneaker.title = sneaker.title.ToUpper().Replace("NIKE NIKE", "NIKE");
                    }

                    //исправляем заголовок
                    if (sneaker.title.ToUpper().Contains("JORDAN JORDAN"))
                    {
                        sneaker.title = sneaker.title.ToUpper().Replace("JORDAN JORDAN", "JORDAN");
                    }

                    //исправляем заголовок
                    if (sneaker.title.ToUpper().Contains("JORDAN AIR JORDAN"))
                    {
                        sneaker.title = sneaker.title.ToUpper().Replace("JORDAN AIR JORDAN", "AIR JORDAN");
                    }

                    if (!sneaker.title.ToUpper().Contains(sneaker.brand.ToUpper()))
                    {
                        sneaker.title = sneaker.brand + " " + sneaker.title;
                    }

                    sneaker.title = sneaker.title.ToUpper();
                    sneaker.color = sneaker.color.ToUpper();

                    newSneakers.Add(sneaker);
                }
                else Program.Logger.Debug("wrong sku: " + sneaker.sku);
            }
            fullcatalog.sneakers = newSneakers;
            //string filename = fullcatalog.FileNameCatalog.Replace(".csv", "2.csv");
            fullcatalog.SaveCatalogToCSV(fullcatalog.FileNameCatalog);
        }

        public static void SaveCatalogToJson()
        {
            var fullCatalog = new FullCatalog();
            var sneakers = fullCatalog.sneakers;

            var jsonFullCatalog = new Model.FullCatalog.FullCatalogRoot();
            foreach (var sneaker in sneakers)
            {
                var record = new Model.FullCatalog.FullCatalogRecord();
                record.sku = sneaker.sku;
                record.brand = sneaker.brand;
                record.title = sneaker.title;
                record.color = sneaker.color;
                record.collection = sneaker.collection;
                record.category = Helper.ConvertCategoryRusToEng(sneaker.category);
                record.sex = Helper.ConvertSexRusToEng(sneaker.sex);
                record.height = sneaker.height;
                record.destination = sneaker.destination;
                record.description = sneaker.description;
                record.link = sneaker.link;
                record.images = sneaker.images;
                record.type = sneaker.type;
                jsonFullCatalog.records.Add(record);                   
            }
            jsonFullCatalog.update_time = DateTime.Now;

            var folder = "FullCatalog";
            var filename = "FullCatalog.json";
            var localFileName = Config.GetConfig().DirectoryPathParsing + folder + @"\" + filename;
            //сохраняем на яндекс.диск файл
            var textJson = JsonConvert.SerializeObject(jsonFullCatalog);
            System.IO.File.WriteAllText(localFileName, textJson);

            ////подгружаем из конфига данные фтп
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            var ftpHost = appSettings["ftpHostSneakerIcon"];
            var ftpUser = appSettings["ftpUserSneakerIcon"];
            var ftpPass = appSettings["ftpPassSneakerIcon"];

            ////загружаем на ftp файл
            var ftpFileName = folder + "/" + filename;
            Helper.LoadFileToFtp(localFileName, ftpFileName, ftpHost, ftpUser, ftpPass);

            //throw new NotImplementedException();
        }
    }
}
