using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes
{
    public class CurrencyConverter
    {
        public Dictionary<String, Double> RateCurrency;

        public CurrencyConverter()
        {
            RateCurrency = new Dictionary<string, double>();
            UpdateRateCurrency();
        }

        public double ConvertCurrency(Double value, string inputCurrency, string outputCurrency)
        {
            if (inputCurrency == "EUR" && outputCurrency == "USD")
            {
                double result = RateCurrency[inputCurrency] * value / RateCurrency[outputCurrency];
                return result;
            }
            throw new NotImplementedException();
        }

        private void UpdateRateCurrency()
        {
            RateCurrency.Add("EUR", 68);
            RateCurrency.Add("USD", 65);
        }
    }
}
