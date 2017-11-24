﻿using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Linq;
using Bot_Application.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Bot_Application.Dialogs
{
    [LuisModel("bc5ca548-9a6b-47b1-b003-e76f03c41ede", "4d8f8aeed14f4e90a526fa12711136bf", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        public async Task All(IDialogContext context, LuisResult result)
        {
            var entities = result.Entities;
            
            string message = $"Detected intent: " + string.Join(", ", result.Intents.Select(i => i.Intent));

            if (entities.Count() > 0)
            {
                foreach (var ent in entities)
                {
                    message += "\n\n" + ent.Entity;
                }
            }
            
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = "Sorry, I could not understand your message.\nCould you please try rephrasing that?";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("ExchangeRate")]
        public async Task ExchangeRate(IDialogContext context, LuisResult result)
        {
            string currency = result.Entities.First().Entity.ToUpper();
            string url = "https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency=NZD&to_currency=" + currency + "&apikey=HZLXQ37YR6PGEMW7";

            var client = new HttpClient();
            HttpResponseMessage response;
            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                CurrencyExchangeModel responseModel = JsonConvert.DeserializeObject<CurrencyExchangeModel>(responseString);

                double exchangerate = responseModel.RealtimeCurrencyExchangeRate.ExchangeRate;
                string from_currency = responseModel.RealtimeCurrencyExchangeRate.From_CurrencyName;
                string to_currency = responseModel.RealtimeCurrencyExchangeRate.To_CurrencyName;

                string message = "1 " + from_currency + " equals " + exchangerate + " " + to_currency + "\n\n(data provided by www.alphavantage.co)";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
            else
            {
                var message = "Unable to fetch exchange rate data.\nPlease try again.";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("Stock")]
        public async Task Stock(IDialogContext context, LuisResult result)
        {
            string company = result.Entities.First().Entity.ToUpper();
            string url = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=" + company + "&apikey=HZLXQ37YR6PGEMW7";

            var client = new HttpClient();
            HttpResponseMessage response;
            response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var index1 = responseString.IndexOf("close", 286) + 4 + "close".Length;
                var index2 = responseString.IndexOf('"', index1);
                var length = index2 - index1;

                var stock = responseString.Substring(index1, length);

                string message = "Stock price for " + company + " is " + stock + " USD.\n\n(data provided by www.alphavantage.co)";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
            else
            {
                var message = "Unable to find stock data.\nPlease try again.";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }


        }
    }
}