using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.Parsing;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Model.AvitoModel;
using SneakerIcon.Model.FullCatalog;
using SneakerIcon.Model.ShopifyModel;

namespace SneakerIcon.Controller.ShopifyController
{
    public class ExportFromSamaraDiscontToShopify
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public new const string FolderName = "ExportFromSamaraDiscontToShopify";
        public const int margin = 1500;
        private string Log { get; set; }
        private static string Br = "\n"; 

        public FullCatalogRoot fullCatalog { get; set; }

        public ExportFromSamaraDiscontToShopify()
        {
            Log = String.Empty;
        }

        public void Run()
        {
            var discontSamara = DiscontSamaraParser.LoadLocalFileJson();
            fullCatalog = FullCatalog2.LoadLocalFile();

            var shopifyRecords = CreateShopifyRecords(discontSamara);

        }

        private List<ShopifyRecord> CreateShopifyRecords(RootParsingObject discontSamara)
        {
            var shopifyRecords = new List<ShopifyRecord>();
            foreach (var listing in discontSamara.listings)
            {
                var shopifyRecordsListing = CreateShopifyListingRecords(listing);
                if (shopifyRecordsListing != null)
                {
                    shopifyRecords.AddRange(shopifyRecordsListing);
                }
            }
            return shopifyRecords;
        }

        private List<ShopifyRecord> CreateShopifyListingRecords(Listing listing)
        {
            Log += "sku: " + listing.sku + " title: " + listing.title + Br;

            if (listing.sizes.Count == 0)
            {
                Log += "sneaker sizes is empty. sku: " + listing.sku;
                return null;
            }

            var fcSneaker = fullCatalog.records.Find(x => x.sku == listing.sku);
            if (fcSneaker == null)
            {
                Log += "sneaker is not exist in fullCatalog. sku: " + listing.sku;
                return null;
            }

            var shopifyRecordsListing = new List<ShopifyRecord>();

            var mainRecord = new ShopifyRecord();
            mainRecord.Handle = listing.sku;
            mainRecord.Title = fcSneaker.title + " " + listing.sku;
            mainRecord.Body = "<h3><strong>100% AUTHENTIC</strong></h3>";
            mainRecord.Body += "<h3><strong>WORLDWIDE SHIPPING FOR 5 - 10 DAYS IN DOUBLE BOX WITH TRACKING NUMBER.<br></strong></h3>";
            mainRecord.Body += "<h3><strong>SHIP IN 2 BUSINESS DAY.</strong></h3>";
            mainRecord.Vendor = fcSneaker.brand;
            //mainRecord.Tags = fcSneaker.category;
            mainRecord.Published = true;
            
            mainRecord.Option_1_Name = "Size";
            var sizeUs = listing.sizes[0].us;
            mainRecord.Option_1_Value = sizeUs;
            mainRecord.Variant_SKU = listing.sku + "-" + sizeUs;
            mainRecord.Variant_Grams = "1000";
            mainRecord.Variant_Inventory_Tracker = "shopify";
            mainRecord.Variant_Inventory_Qty = listing.sizes[0].quantity;
            mainRecord.Variant_Inventory_Policy = "deny";
            mainRecord.Variant_Fulfillment_Service = "manual";

            //price 
            var price = listing.price + margin;
            mainRecord.Variant_Price = price.ToString("F", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

            mainRecord.Variant_Requires_Shipping = "true"; //todo change to boolean type
            mainRecord.Variant_Taxable = true;

            return shopifyRecordsListing;
        }
    }
}
