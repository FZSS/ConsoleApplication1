using SneakerIcon.Classes.Catalogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Photos
{
    public class AddPhotos
    {
        public AddPhotos()
        {

        }

        public void Run()
        {
            AddPhotosInFullCatalog();
        }

        public void AddPhotosInFullCatalog()
        {
            //Добавить фотографии с моего сервера в файл каталога (в основном для товаров из дисконта)
            FullCatalog fullCatalog = new FullCatalog();
            foreach (var fullCatalogSneaker in fullCatalog.sneakers)
            {
                if (fullCatalogSneaker.images == null || fullCatalogSneaker.images.Count == 0)
                {
                    bool test = true;
                }
                if (fullCatalogSneaker.images.Count == 1)
                {
                    if (String.IsNullOrWhiteSpace(fullCatalogSneaker.images[0]))
                    {
                        List<string> images = new List<string>();
                        Console.WriteLine("check " + fullCatalogSneaker.sku);
                        for (int i = 1; i < 10; i++)
                        {
                            string url = "http://80.241.220.50/images2/" + fullCatalogSneaker.sku + "-" + i + ".jpg";

                            if (NetworkUtils.UrlExists(url))
                            {
                                images.Add(url);
                            }
                        }
                        if (images.Count > 0)
                        {
                            Console.WriteLine("add " + fullCatalogSneaker.sku);
                            fullCatalogSneaker.images = images;
                        }
                    }
                }
            }

            foreach (var fullCatalogSneaker in fullCatalog.sneakers)
            {
                if (fullCatalogSneaker.images == null || fullCatalogSneaker.images.Count == 0)
                {
                    bool test = true;
                }
                if (fullCatalogSneaker.images.Count == 1)
                {
                    if (String.IsNullOrWhiteSpace(fullCatalogSneaker.images[0]))
                    {
                        List<string> images = new List<string>();
                        Console.WriteLine("check " + fullCatalogSneaker.sku);
                        for (int i = 1; i < 10; i++)
                        {
                            string url = "http://80.241.220.50/images/" + fullCatalogSneaker.sku + "-" + i + ".jpg";

                            if (NetworkUtils.UrlExists(url))
                            {
                                images.Add(url);
                            }
                        }
                        if (images.Count > 0)
                        {
                            Console.WriteLine("add " + fullCatalogSneaker.sku);
                            fullCatalogSneaker.images = images;
                        }
                    }
                }
            }

            fullCatalog.SaveCatalogToCSV(fullCatalog.FileNameCatalog);
        }
    }
}
