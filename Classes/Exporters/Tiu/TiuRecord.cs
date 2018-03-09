using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Stocks;

namespace SneakerIcon.Classes.Exporters
{
    public class TiuRecord
    {
        public string kod_tovara { get; set; }
        public string nazvanie_pozicii { get; set; }
        public string kluchevie_slova { get; set; }
        public string opisanie { get; set; }
        public string tip_tovara { get; set; }
        public string cena { get; set; }
        public string valuta { get; set; }
        public string edinica_izmerenia { get; set; }
        public string minimalnij_objem_zakaza { get; set; }
        public string optovaya_cena { get; set; }
        public string minimalnij_zakaz_opt { get; set; }
        public string ssilka_izobrazheniya { get; set; }
        public string nalichie { get; set; }
        public string kolichestvo { get; set; }
        public string nomer_gruppi { get; set; }
        public string nazvanie_gruppi { get; set; }
        public string adres_podrazdela { get; set; }
        public string vozmozhnost_postavki { get; set; }
        public string srok_postavki { get; set; }
        public string sbosob_upakovki { get; set; }
        public string unikalnij_identifikator { get; set; }
        public string identifikator_tovara { get; set; }
        public string identifikator_podrazdela { get; set; }
        public string identifikator_gruppi { get; set; }
        public string proizvoditel { get; set; }
        public string garantijnij_srok { get; set; }
        public string strana_proizvoditel { get; set; }
        public string skidka { get; set; }
        public string id_gruppi_raznovidnostej { get; set; }
        //public string nazvanie_harakteristiki_razmerRUS { get; set; }
        //public string izmerenie_harakteristiki_razmerRUS { get; set; }
        //public string znachenie_harakteristiki_razmerRUS { get; set; }
        public string nazvanie_harakteristiki_razmer { get; set; }
        public string izmerenie_harakteristiki_razmer { get; set; }
        public string znachenie_harakteristiki_razmer { get; set; }
        //public string nazvanie_harakteristiki_razmerUS { get; set; }
        //public string izmerenie_harakteristiki_razmerUS { get; set; }
        //public string znachenie_harakteristiki_razmerUS { get; set; }
        //public string nazvanie_harakteristiki_razmerUK { get; set; }
        //public string izmerenie_harakteristiki_razmerUK { get; set; }
        //public string znachenie_harakteristiki_razmerUK { get; set; }
        //public string nazvanie_harakteristiki_razmerEUR { get; set; }
        //public string izmerenie_harakteristiki_razmerEUR { get; set; }
        //public string znachenie_harakteristiki_razmerEUR { get; set; }
        //public string nazvanie_harakteristiki_razmerRUS2 { get; set; }
        //public string izmerenie_harakteristiki_razmerRUS2 { get; set; }
        //public string znachenie_harakteristiki_razmerRUS2 { get; set; }
        //public string nazvanie_harakteristiki_razmerCM { get; set; }
        //public string izmerenie_harakteristiki_razmerCM { get; set; }
        //public string znachenie_harakteristiki_razmerCM { get; set; }

        public TiuRecord()
        {
            Initialize();
        }

        public TiuRecord(StockRecord stockRecord)
        {
            Initialize();
            SetParameters(stockRecord);
        }

        public void SetParameters(StockRecord stockRecord)
        {
            kod_tovara = stockRecord.sku + "-" + stockRecord.size;
            identifikator_tovara = kod_tovara;
            id_gruppi_raznovidnostej = stockRecord.sku.Replace("-", "");
            znachenie_harakteristiki_razmer = stockRecord.size;
            proizvoditel = "Nike";
        }

        public void Initialize()
        {
            this.tip_tovara = "r";
            this.valuta = "RUB";
            this.edinica_izmerenia = "шт.";
            this.nalichie = "+";
            this.nazvanie_harakteristiki_razmer = "Размер";
            //this.nazvanie_harakteristiki_razmerUS = "Американский размер";
            //this.nazvanie_harakteristiki_razmerUK = "Английский размер";
            //this.nazvanie_harakteristiki_razmerEUR = "Евройпейский размер";
            //this.nazvanie_harakteristiki_razmerRUS2 = "Российский размер";
            //this.nazvanie_harakteristiki_razmerCM = "Размер в сантиметрах";
        }

        public bool SetParametersFromSneaker(Sneaker sneaker)
        {
            //title
            nazvanie_pozicii = sneaker.title;
            if (!String.IsNullOrWhiteSpace(sneaker.type))
            {
                nazvanie_pozicii = sneaker.type + " " + nazvanie_pozicii;

            }

            //description
            string desc = nazvanie_pozicii + "\r\n";
            desc += "Артикул: " + sneaker.sku + "\r\n";
            desc += "По поводу размеров звоните или оставляйте заявку.\r\n";
            desc += "------------------------\r\n";
            desc += "Вся обувь оригинальная, новая, в упаковке\r\n";
            desc += "------------------------\r\n";
            desc += "Доставка по России 2-3 дня компанией СДЭК.\r\n";
            desc += "Стоимость доставки по РФ - 300 рублей.\r\n";
            opisanie = desc;

            //categorySneakerFullCatalog
            switch (sneaker.category)
            {
                case "Мужская":
                    nomer_gruppi = "15210736";
                    nazvanie_gruppi = "Мужская обувь";
                    //nazvanie_harakteristiki_razmerRUS = "Размер мужской обуви";
                    break;
                case "Женская":
                    nomer_gruppi = "15210737";
                    nazvanie_gruppi = "Женская обувь";
                    //nazvanie_harakteristiki_razmerRUS = "Размер женской обуви";
                    break;
                case "Детская":
                    nomer_gruppi = "15210738";
                    nazvanie_gruppi = "Детская обувь";
                    //nazvanie_harakteristiki_razmerRUS = "Размер женской обуви";
                    break;
                default:
                    Program.Logger.Warn("sneaker without category. sku:" + sneaker.sku);
                    return false;
                    //this.categorySneakerFullCatalog = "93427"; //по умолчанию тоже мужская обувь
                    break;
            }

            //images
            var images = sneaker.images;
            if (images == null)
            {
                return false;
            }
            else
            {
                if (images.Count == 1 && String.IsNullOrWhiteSpace(images[0]))
                {
                    return false;
                }
                else
                {
                    ssilka_izobrazheniya = String.Join(", ",images);
                }
            }

            //sizes


            return true;
            //throw new NotImplementedException();
        }
    }
}
