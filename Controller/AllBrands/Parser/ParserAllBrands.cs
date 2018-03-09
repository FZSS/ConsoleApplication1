using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Parsing.Model;
using System.Configuration;
using System.IO;
using SneakerIcon.Controller.AllBrands;
using SneakerIcon.Sys;

namespace SneakerIcon.Controller.Parser
{
    public class ParserAllBrands : Classes.Parsing.Parser
    {
        public const string ParsersFolder = "AllBrands";
        public SizeChartAllBrands SizeChart { get; set; }

        public ParserAllBrands()
        {
            SizeChart = new SizeChartAllBrands();
            SizeChart.LoadSizeChartFromFtp();
        }

        protected new void SaveJson(RootParsingObject json, string filename, string folder, string name)
        {
            //подгружаем из конфига данные фтп
            var ftpHost = Config.GetConfig().FtpHostAllBrands;
            var ftpUser = Config.GetConfig().FtpUserAllBrands;
            var ftpPass = Config.GetConfig().FtpPassAllBrands;

            if (!Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
                Helper.CreateDirectoryFtp(name, ftpHost, ftpUser, ftpPass);
            }
                
            var localFileName = folder + filename;
            //сохраняем на яндекс.диск файл
            var textJson = JsonConvert.SerializeObject(json);
            System.IO.File.WriteAllText(localFileName, textJson);

            

            //загружаем на ftp файл
            
            var ftpFileName = name + "/" + filename;
            Helper.LoadFileToFtp(localFileName, ftpFileName, ftpHost, ftpUser, ftpPass);
        }

        protected void SaveNikeJson(RootParsingObject json, string filename, string folder, string name)
        {
            var nikeJson = new RootParsingObject();
            nikeJson.market_info = json.market_info;
            nikeJson.listings = json.listings.FindAll(x => x.brand.ToLower() == "nike" ||
                                                           x.brand.ToLower() == "jordan");
            base.SaveJson(nikeJson, filename, folder, name);
        }

    }
}
