using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.Models
{
    [Serializable]
    public class CurrencyExchangeModel
    {
        [JsonProperty(PropertyName = "Realtime Currency Exchange Rate")]
        public RealtimeCurrencyExchangeRate RealtimeCurrencyExchangeRate { get; set; }
    }

    public class RealtimeCurrencyExchangeRate
    {
        [JsonProperty(PropertyName = "1. From_Currency Code")]
        public string From_CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "2. From_Currency Name")]
        public string From_CurrencyName { get; set; }

        [JsonProperty(PropertyName = "3. To_Currency Code")]
        public string To_CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "4. To_Currency Name")]
        public string To_CurrencyName { get; set; }

        [JsonProperty(PropertyName = "5. Exchange Rate")]
        public double ExchangeRate { get; set; }

        [JsonProperty(PropertyName = "6. Last Refreshed")]
        public string LastRefreshed { get; set; }

        [JsonProperty(PropertyName = "7. Time Zone")]
        public string TimeZone { get; set; }
    }
}