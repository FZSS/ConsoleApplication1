using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Exporters.Fancy
{
    public class FancyExporter : Exporter
    {
        public static readonly string DIRECTORY_PATH = Exporter.DIRECTORY_PATH + @"Fancy\";
        public static readonly string FILENAME = DIRECTORY_PATH + "Fancy.csv";
        List<FancyRecord> Records { get; set; }

        public FancyExporter()
            : base()
        {
            Records = new List<FancyRecord>();
        }

        public void Run()
        {
            CreateRecords();
            ExportToCSV(FILENAME);
        }

        private void CreateRecords()
        {
            throw new NotImplementedException();
        }

        private void ExportToCSV(string FILENAME)
        {
            throw new NotImplementedException();
        }
    }
}
