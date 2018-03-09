using Newtonsoft.Json;
using SneakerIcon.Classes.Catalogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.UPCDB
{
    public class UPCDB2
    {
        public MyUPCDBRootJSONObject myUPCDB { get; set; }
        public FullCatalog fullCatalog = new FullCatalog();

        public void Initialize()
        {
            myUPCDB = ReadJson();
        }

        public static MyUPCDBRootJSONObject ReadJson()
        {
            var json = new MyUPCDBRootJSONObject();

            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            string ftpHost = appSettings["ftpHostUPCDB"];
            string ftpUser = appSettings["ftpUserUPCDB"];
            string ftpPass = appSettings["ftpPassUPCDB"];

            var Folder = "";
            var Filename = "upcdb.json";

            //Helper.GetFileFromFtp(Filename, Folder+Filename, ftpHost, ftpUser, ftpPass);
            var textJson = Helper.GetFileFromFtpIntoString(Filename, Folder + Filename, ftpHost, ftpUser, ftpPass);
            return JsonConvert.DeserializeObject<MyUPCDBRootJSONObject>(textJson);
        }

        public SneakerJson AddSneakerToUpcdb(string brand, string sku)
        {
            SneakerJson sneaker = new SneakerJson();
            var sneakerFullCatalog = fullCatalog.sneakers.Find(x => x.sku == sku);



            return sneaker;
        }

    }
}
