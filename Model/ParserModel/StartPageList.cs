using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.ParserModel
{
    public class StartPageList
    {
        public List<StartPage> records { get; set; }

        public StartPageList()
        {
            records = new List<StartPage>();
        }

        public void AddPage(string url, string brand)
        {
            var page = new StartPage();
            page.url = url;
            page.brand = brand;
        }
    }
}
