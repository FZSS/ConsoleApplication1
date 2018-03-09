using SneakerIcon.Classes.Catalogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Exporters
{
    public class WithoutPhotoExporter
    {
        public FullCatalog Catalog { get; set; }
        public const string IMAGES_FOLDER_NAME = @"C:\SneakerIcon\images\";
        public Catalog WithoutPhotoCatalog { get; set; }

        public WithoutPhotoExporter()
        {
            Catalog = new FullCatalog();
            WithoutPhotoCatalog = new Catalog();
            //CheckExistPhoto();
            CreateCatalog();
            WithoutPhotoCatalog.SaveCatalogToCSV(@"C:\Users\Администратор\YandexDisk\sneaker-icon\Export\Work\withoutphoto.csv");
        }

        public void CreateCatalog() {
            foreach (var sneaker in Catalog.sneakers)
            {
                if (sneaker.images.Count == 1 && String.IsNullOrWhiteSpace(sneaker.images[0]))
                {
                    sneaker.images = new List<string>();
                    for (int i = 1; i <= 10; i++)
                    {
                        string imageFileName = sneaker.sku + "-" + i + ".jpg";
                        if (File.Exists(IMAGES_FOLDER_NAME + imageFileName))
                        {
                            sneaker.images.Add(imageFileName);
                        }
                    }
                    if (sneaker.images.Count == 0)
                        WithoutPhotoCatalog.AddUniqueSneaker(sneaker);    
                }
            
            }
        }

        public void CheckExistPhoto()
        {
            foreach (var sneaker in Catalog.sneakers)
            {
                if (sneaker.images.Count > 1 && !String.IsNullOrWhiteSpace(sneaker.images[0]))
                {
                    var images = new List<string>();
                    bool isNotExistHave = false;
                    for (int i = 0; i <= sneaker.images.Count-1; i++)
                    {
                        string imageFileName = sneaker.images[i];
                        if (NetworkUtils.UrlExists(imageFileName))
                        {                       
                            images.Add(imageFileName);
                            Console.WriteLine(imageFileName);
                        }
                        else 
                        {
                            isNotExistHave = true;
                        }
                    }
                    if (isNotExistHave)
                    {
                        sneaker.images = images;
                    }
                    if (sneaker.images.Count == 0)
                        WithoutPhotoCatalog.AddUniqueSneaker(sneaker);
                }

            }
        }
    }
}
