using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.ShopifyModel
{
    // url: https://help.shopify.com/manual/products/import-export
    public class ShopifyRecord
    {
        /// <summary>
        /// analog sku or id 
        /// 
        /// Handles are unique names for each product. 
        /// They can contain letters, dashes and numbers, but no spaces. 
        /// A handle is used in the URL for each product. 
        /// For example, the handle for a "Women's Snowboard" should be womens-snowboard, 
        /// and the product's URL would be https://yourstore.myshopify.com/product/womens-snowboard.
        /// Every line in the CSV starting with a different handle is treated as a new product.
        /// To add multiple images to a product, you should have multiple lines with the same handle.
        /// </summary>
        public string Handle { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string Vendor { get; set; }
        public string Type { get; set; }
        public string Tags { get; set; }
        public bool Published { get; set; }
        public string Option_1_Name { get; set; }
        public string Option_1_Value { get; set; }
        public string Option_2_Name { get; set; }
        public string Option_2_Value { get; set; }
        public string Option_3_Name { get; set; }
        public string Option_3_Value { get; set; }
        public string Variant_SKU { get; set; }
        public string Variant_Grams { get; set; }
        public string Variant_Inventory_Tracker { get; set; }
        public int Variant_Inventory_Qty { get; set; }
        public string Variant_Inventory_Policy { get; set; }
        public string Variant_Fulfillment_Service { get; set; }
        public string Variant_Price { get; set; }
        public string Variant_Compare_at_Price { get; set; }
        public string Variant_Requires_Shipping { get; set; }
        public bool Variant_Taxable { get; set; }
        public string Variant_Barcode { get; set; }
        public string Image_Src { get; set; }
        public string Image_Position { get; set; }
        public string Image_Alt_Text { get; set; }
        public string Gift_Card { get; set; }
        public string Variant_Image { get; set; }
        public string Variant_Weight_Unit { get; set; }
        public string Variant_Tax_Code { get; set; }
    }
}
