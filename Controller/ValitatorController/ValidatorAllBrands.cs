using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Controller.AllBrands;

namespace SneakerIcon.Controller.ValitatorController
{
    public class ValidatorAllBrands : Validator
    {
        public static new void ValidateAllShops()
        {
            ShopValidatorAllBrands shopValidatorAllBrands = new ShopValidatorAllBrands();
            shopValidatorAllBrands.ValidateAllShops();
        }

        internal string DetectCategoryFromSize(Listing listing, SizeChartAllBrands sizeChart)
        {
            //log += "Detecting categoy..." + br;
            var sizes = sizeChart.Sizes.FindAll(x => x.Brand.ToLower() == listing.brand.ToLower());
            if (sizes.Count == 0)
            {
                log += "Error: brand did not find in sizechart: " + listing.brand + br;
                return null;
            }
                

            //пока что сделаю определение категории по первому размеру, но нужно будет доработать

            if (listing.sizes.Count == 0)
            {
                log += "Error: listing.sizes.count = 0" + br;
                return null;
            }

            //поиск размера по us/eu, самое частое что встречается
            var lSz = listing.sizes.First();

            if (!string.IsNullOrWhiteSpace(lSz.us))
                sizes = sizes.FindAll(x => x.SizeUs == lSz.us);
            if (sizes.Count == 0)
            {
                log += "Error: wrong us size: " + lSz.us + br;  
                return null;
            }
                

            lSz.eu = lSz.eu.Replace("1/3",".3").Replace("2/3",".7").Replace(" ","").Trim();
            if (!string.IsNullOrWhiteSpace(lSz.eu))
                sizes = sizes.FindAll(x => x.SizeEu == lSz.eu);
            if (sizes.Count == 0)
            {
                log += "Error: wrong eu size: " + lSz.eu + br;
                return null;
            }
                

            if (!string.IsNullOrWhiteSpace(lSz.uk))
                sizes = sizes.FindAll(x => x.SizeUk == lSz.uk);
            if (sizes.Count == 0)
            {
                log += "Error: wrong uk size: " + lSz.uk + br;
                return null;
            }

            if (!string.IsNullOrWhiteSpace(lSz.cm))
                sizes = sizes.FindAll(x => x.SizeCm == lSz.cm);
            if (sizes.Count == 0)
            {
                log += "Error: wrong cm size: " + lSz.cm + br;
                return null;
            }

            if (sizes.Count > 0)
                return sizes[0].Category;
            else
                return null;

        }
    }
}
