using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using SneakerIcon.Classes;
using SneakerIcon.Model.AllBrands.SizeChartAllBrandsModel;
using SneakerIcon.Sys;

namespace SneakerIcon.Controller.AllBrands
{
    public class SizeChartAllBrands
    {
        public List<SizeChartAllBrandsRecord> Sizes { get; set; }

        private static string FtpHost = Config.GetConfig().FtpHostSneakerIcon;
        private static string FtpUser = Config.GetConfig().FtpUserSneakerIcon;
        private static string FtpPass = Config.GetConfig().FtpPassSneakerIcon;
        private static string FtpPath = "SizeChart/SizeChartAllBrands.csv";

        public SizeChartAllBrands()
        {
            Sizes = new List<SizeChartAllBrandsRecord>();
        }

        public void LoadSizeChartFromFtp()
        {
            var filename = "SizeChartAllBrands.csv";
            Helper.GetFileFromFtp(filename, FtpPath, FtpHost, FtpUser, FtpPass);

            using (var sr = new StreamReader(filename, Encoding.UTF8))
            {
                var reader = new CsvReader(sr);
                reader.Configuration.Delimiter = ",";
                IEnumerable<SizeChartAllBrandsRecord> records = reader.GetRecords<SizeChartAllBrandsRecord>();
                Sizes = records.ToList();
            }

            foreach (var size in Sizes)
            {
                size.SizeUs = size.SizeUs.Replace(",", ".");
                size.SizeEu = size.SizeEu.Replace(",", ".");
                size.SizeUk = size.SizeUk.Replace(",", ".");
                size.SizeCm = size.SizeCm.Replace(",", ".");
                size.Category = size.Category.ToLower();
            }
        }
    }
}
