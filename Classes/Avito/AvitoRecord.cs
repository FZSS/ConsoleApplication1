using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Avito
{
    public class AvitoRecord
    {
        public string login { get; set; }
        public string pass { get; set; }
        public string category { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string price { get; set; }
        public string phone { get; set; }
        public string images { get; set; }
        public string sku { get; set; }

        public AvitoRecord()
        {

        }
        public AvitoRecord(string login, string pass, string phone)
        {
            this.login = login;
            this.pass = pass;
            this.phone = phone;
        }
        
    }
}
