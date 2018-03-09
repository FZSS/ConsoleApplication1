using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing;
using SneakerIcon.Classes.Stocks;
using SneakerIcon.Classes.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Exporters
{
    public class Exporter
    {
        public static readonly string DIRECTORY_PATH = Config.GetConfig().DirectoryPathExport;
        public FullCatalog catalog { get; set; }
        public DiscontStock DiscontSamara { get; set; }
        public DiscontStock Kuzminki { get; set; }
        public NashStock NashStock1 { get; set; }
        public List<OnlineShopStock> ShopStocks { get; set; }
        public List<DiscontStock> DiscontStocks { get; set; }
        public OnlineShopStock Titolo { get; set; }
        public OnlineShopStock Sivas { get; set; }
        public OnlineShopStock Einhalb { get; set; }
        public OnlineShopStock StreetBeat { get; set; }
        public OnlineShopStock Queens { get; set; }
        public OnlineShopStock BasketShop { get; set; }
        public OnlineShopStock Sneakersnstuff { get; set; }
        public OnlineShopStock Overkillshop { get; set; }
        public OnlineShopStock Solehaven { get; set; }
        public OnlineShopStock Afew { get; set; }
        public OnlineShopStock SuppaStore { get; set; }
        public OnlineShopStock Chmielna { get; set; }
        public OnlineShopStock BdgaStore { get; set; }
        public OnlineShopStock AsfaltGold { get; set; }

        public Exporter()
        {
            catalog = new FullCatalog();
            DiscontSamara = new DiscontStock(Config.GetConfig().DiscontStockFilename);
            DiscontSamara.AddSaleToRecords();
            Kuzminki = new DiscontStock(Config.GetConfig().DiscontKuzminkyFilename);
            DiscontStocks = new List<DiscontStock>();
            DiscontStocks.Add(DiscontSamara);
            //DiscontStocks.Add(Kuzminki);
            NashStock1 = new NashStock(Config.GetConfig().NashStockFilename);
            ShopStocks = new List<OnlineShopStock>();
            
            //queens
            Queens = new OnlineShopStock(QueensParser.NAME, QueensParser.STOCK_FILENAME, QueensParser.CURRENCY);
            Queens.Marzha = QueensParser.MARZHA;
            Queens.DeliveryToUSA = QueensParser.DELIVERY_TO_USA;
            Queens.VatValue = QueensParser.VAT_VALUE;
            ShopStocks.Add(Queens);
            
            //streetbeat
            StreetBeat = new OnlineShopStock(StreetBeatParser.NAME, StreetBeatParser.STOCK_FILENAME, StreetBeatParser.CURRENCY);
            StreetBeat.Marzha = StreetBeatParser.MARZHA;
            StreetBeat.DeliveryToUSA = StreetBeatParser.DELIVERY_TO_USA;
            StreetBeat.VatValue = StreetBeatParser.VAT_VALUE;
            ShopStocks.Add(StreetBeat);
            
            //einhalb
            Einhalb = new OnlineShopStock(EinhalbParser.NAME, EinhalbParser.STOCK_FILENAME, EinhalbParser.CURRENCY);
            Einhalb.Marzha = EinhalbParser.MARZHA;
            Einhalb.DeliveryToUSA = EinhalbParser.DELIVERY_TO_USA;
            Einhalb.VatValue = EinhalbParser.VAT_VALUE;
            ShopStocks.Add(Einhalb);
            
            //basketshop
            BasketShop = new OnlineShopStock(BasketShopParser.NAME, BasketShopParser.STOCK_FILENAME, BasketShopParser.CURRENCY);
            BasketShop.Marzha = BasketShopParser.MARZHA;
            BasketShop.DeliveryToUSA = BasketShopParser.DELIVERY_TO_USA;
            ShopStocks.Add(BasketShop);
            
            //sivas
            Sivas = new OnlineShopStock(SivasParser.NAME, SivasParser.STOCK_FILENAME, SivasParser.CURRENCY);
            Sivas.Marzha = SivasParser.MARZHA;
            Sivas.DeliveryToUSA = SivasParser.DELIVERY_TO_USA;
            ShopStocks.Add(Sivas);
            
            //titolo
            Titolo = new OnlineShopStock(TitoloParser.NAME, TitoloParser.STOCK_FILENAME, TitoloParser.CURRENCY);
            Titolo.Marzha = TitoloParser.MARZHA;
            Titolo.DeliveryToUSA = TitoloParser.DELIVERY_TO_USA;
            Titolo.VatValue = TitoloParser.VAT_VALUE;
            ShopStocks.Add(Titolo);
            
            //sns
            Sneakersnstuff = new OnlineShopStock(SneakersnstuffParser.NAME, SneakersnstuffParser.STOCK_FILENAME, SneakersnstuffParser.CURRENCY);
            Sneakersnstuff.Marzha = SneakersnstuffParser.MARZHA;
            Sneakersnstuff.DeliveryToUSA = SneakersnstuffParser.DELIVERY_TO_USA;
            Sneakersnstuff.VatValue = SneakersnstuffParser.VAT_VALUE;
            ShopStocks.Add(Sneakersnstuff);
            
            //overkill
            Overkillshop = new OnlineShopStock(OverkillshopParser.NAME, OverkillshopParser.STOCK_FILENAME, OverkillshopParser.CURRENCY);
            Overkillshop.Marzha = OverkillshopParser.MARZHA;
            Overkillshop.DeliveryToUSA = OverkillshopParser.DELIVERY_TO_USA;
            Overkillshop.VatValue = OverkillshopParser.VAT_VALUE;
            ShopStocks.Add(Overkillshop);

            //solehaven
            Solehaven = new OnlineShopStock(SolehavenParser.NAME, SolehavenParser.STOCK_FILENAME, SolehavenParser.CURRENCY);
            Solehaven.Marzha = SolehavenParser.MARZHA;
            Solehaven.DeliveryToUSA = SolehavenParser.DELIVERY_TO_USA;
            Solehaven.VatValue = SolehavenParser.VAT_VALUE;
            ShopStocks.Add(Solehaven);

            //afew
            Afew = new OnlineShopStock(AfewStoreParser.NAME, AfewStoreParser.STOCK_FILENAME, AfewStoreParser.CURRENCY);
            Afew.Marzha = AfewStoreParser.MARZHA;
            Afew.DeliveryToUSA = AfewStoreParser.DELIVERY_TO_USA;
            Afew.VatValue = AfewStoreParser.VAT_VALUE;
            ShopStocks.Add(Afew);

            SuppaStore = new OnlineShopStock(SuppaStoreParser.NAME, SuppaStoreParser.STOCK_FILENAME, SuppaStoreParser.CURRENCY);
            SuppaStore.Marzha = SuppaStoreParser.MARZHA;
            SuppaStore.DeliveryToUSA = SuppaStoreParser.DELIVERY_TO_USA;
            SuppaStore.VatValue = SuppaStoreParser.VAT_VALUE;
            ShopStocks.Add(SuppaStore);

            Chmielna = new OnlineShopStock(ChmielnaParser.NAME, ChmielnaParser.STOCK_FILENAME, ChmielnaParser.CURRENCY);
            Chmielna.Marzha = ChmielnaParser.MARZHA;
            Chmielna.DeliveryToUSA = ChmielnaParser.DELIVERY_TO_USA;
            Chmielna.VatValue = ChmielnaParser.VAT_VALUE;
            ShopStocks.Add(Chmielna);

            BdgaStore = new OnlineShopStock(BdgastoreParser.NAME, BdgastoreParser.STOCK_FILENAME, BdgastoreParser.CURRENCY);
            BdgaStore.Marzha = BdgastoreParser.MARZHA;
            BdgaStore.DeliveryToUSA = BdgastoreParser.DELIVERY_TO_USA;
            BdgaStore.VatValue = BdgastoreParser.VAT_VALUE;
            ShopStocks.Add(BdgaStore);

            AsfaltGold = new OnlineShopStock(AsphaltgoldParser.NAME, AsphaltgoldParser.STOCK_FILENAME, AsphaltgoldParser.CURRENCY);
            AsfaltGold.Marzha = AsphaltgoldParser.MARZHA;
            AsfaltGold.DeliveryToUSA = AsphaltgoldParser.DELIVERY_TO_USA;
            AsfaltGold.VatValue = AsphaltgoldParser.VAT_VALUE;
            ShopStocks.Add(AsfaltGold);
        }

        public Sneaker GetCatalogRecordFromId(string id)
        {
            string sku = id.Split('-')[0] + "-" + id.Split('-')[1];
            foreach (var sneaker in catalog.sneakers)
            {
                if (sku == sneaker.sku)
                    return sneaker;
            }
            Program.Logger.Warn("sku does not exist in catalog " + sku);
            return null;
        }

        public static double GetPrice(StockRecord record, string currencyInput, string currencyOutput, int Marzha = 0)
        {
            if (Marzha == 0)
            {
                //у еинхалба маржа 0 была замечена, надо поправить
                if (record.sellPrice == 0)
                {
                    throw new Exception("Marzha = 0 and SellPrice = 0");
                }
                //throw new Exception ("Marzha = 0");
            }
            if (currencyInput == "RUB")
            {
                bool test = true;
            }
            if (currencyInput == "USD")
            {
                bool test = true;
            }
            if (currencyInput == "EUR")
            {
                bool test = true;
            }
            if (currencyInput == "PLN")
            {
                bool test = true;
            }
            CurrencyRate currate = CurrencyRate.ReadObjectFromJsonFile();
            double curInputRate = currate.GetCurrencyRate(currencyInput);
            double curOutputRate = currate.GetCurrencyRate(currencyOutput);

            double curInputRateBuy = curInputRate * 1.035;
            double curOutputRateSell = curOutputRate * 0.965;
            if (currencyInput == "RUB")
            {
                curInputRateBuy = 1;
                //curOutputRateSell = 1;
            }
            if (currencyOutput == "RUB")
            {
                curOutputRateSell = 1;
            }

            double sellPrice;
            if (record.sellPrice > 0)
            {
                sellPrice = record.sellPrice;
            }
            else {
                sellPrice = record.price + Marzha;
            }
            
            double resultPrice;
            if (currencyInput == currencyOutput)
            {
                resultPrice = sellPrice;
            }
            else
            {
                resultPrice = sellPrice * curInputRateBuy / curOutputRateSell;
            }

            resultPrice = Math.Round(resultPrice, 2);
            return resultPrice;
            //if (currency == "USD")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice;
            //        return resultPrice;
            //    }
            //    else
            //    {
            //        resultPrice = record.price + Marzha;
            //        return resultPrice;
            //        //throw new Exception("нет цены продажи sku:" + record.sku);
            //    }
            //}
            //if (currency == "EUR")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice * Settings.EURO_BUY;
            //        return resultPrice / Settings.USD_SELL;
            //    }
            //    else
            //    {
            //        if (Marzha == 0)
            //            throw new Exception("marzha = 0. sku: " + record.sku);
            //        record.sellPrice = record.price + Marzha;
            //        resultPrice = record.sellPrice * Settings.EURO_BUY;
            //        return resultPrice / Settings.USD_SELL;
            //    }
            //}
            //if (currency == "CHF")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice * Settings.CHF_BUY;
            //        return resultPrice / Settings.USD_SELL;
            //    }
            //    else
            //    {
            //        throw new Exception("нет цены продажи sku:" + record.sku);
            //    }
            //}
            //else if (currency == "RUB")
            //{
            //    var sneaker = catalog.GetSneakerFromSKU(record.sku);
            //    if (sneaker != null)
            //    {
            //        if (sneaker.type == "Сланцы")
            //        {
            //            resultPrice = record.price + marzha + 200;
            //            return resultPrice / Settings.USD_SELL;
            //        }
            //    }
            //    resultPrice = record.price + marzha + dostavkaFromRussia;
            //    return resultPrice / Settings.USD_SELL;
            //}
        }

        public static double GetPrice2(double price, double vat_value, int delivery_to_usa, int marzha, string currencyInput, string currencyOutput)
        {
            double resultprice;

            var sebestoimost = ( (price) * (1 - vat_value) ) + delivery_to_usa;
            var sebestWithMarzha = sebestoimost + marzha - delivery_to_usa; //маржа идет уже плюсом с доставкой, поэтому вычитаем ее

            var stockRecord = new StockRecord();
            stockRecord.sellPrice = sebestWithMarzha;

            resultprice = Exporter.GetPrice(stockRecord, currencyInput, currencyOutput);

            return resultprice;
        }
    }
}
