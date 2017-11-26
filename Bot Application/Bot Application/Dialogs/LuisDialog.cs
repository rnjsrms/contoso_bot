using System;
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
using System.Collections.Generic;

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
                    message += "\n" + ent.Entity;
                }
            }

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = "Sorry, I could not understand your message.\n\nCould you please try rephrasing that?";
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

            try
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
            catch
            {
                var message = "Unable to fetch exchange rate data.\n\nPlease try again.";
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

            try
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
            catch
            {
                var message = "Unable to fetch stock data.\n\nPlease try again.";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("CurrencyCalculation")]
        public async Task CurrencyCalculation(IDialogContext context, LuisResult result)
        {
            String currency_to = "";
            String currency_from = "";
            double amount = 0.0;

            foreach (var ent in result.Entities)
            {
                if (ent.Type == "currency_to")
                {
                    currency_to = ent.Entity.ToUpper();
                }
                else if (ent.Type == "currency_from")
                {
                    currency_from = ent.Entity.ToUpper();
                }
                else if (ent.Type == "amount")
                {
                    amount = Convert.ToDouble(ent.Entity);
                }
            }

            if ((currency_from == "") || (currency_to == "") || (amount == 0.0))
            {
                var message = "Could not process currency conversion.\n\nPlease try rephrasing that.";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }

            string url = "https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency=" + currency_from + "&to_currency=" + currency_to + "&apikey=HZLXQ37YR6PGEMW7";

            var client = new HttpClient();
            HttpResponseMessage response;
            response = await client.GetAsync(url);

            try
            {
                var responseString = await response.Content.ReadAsStringAsync();

                CurrencyExchangeModel responseModel = JsonConvert.DeserializeObject<CurrencyExchangeModel>(responseString);

                double exchangerate = responseModel.RealtimeCurrencyExchangeRate.ExchangeRate;
                string from_currency = responseModel.RealtimeCurrencyExchangeRate.From_CurrencyName;
                string to_currency = responseModel.RealtimeCurrencyExchangeRate.To_CurrencyName;

                double currency_calc = exchangerate * amount;

                string message = amount + " " + from_currency + "(s) equal " + currency_calc + " " + to_currency + "(s)\n\n(data provided by www.alphavantage.co)";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
            catch
            {
                var message = "Unable to fetch exchange rate data.\n\nPlease try again.";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("RealPerson")]
        public async Task RealPerson(IDialogContext context, LuisResult result)
        {
            var message = "Sorry, there are no available representatives at the moment.\n\nPlease try again.";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("News")]
        public async Task News(IDialogContext context, LuisResult result)
        {
            string url = "https://newsapi.org/v2/top-headlines?sources=australian-financial-review&apiKey=7542fec653094fa9a76f4ade5e186572";

            var client = new HttpClient();
            HttpResponseMessage response;
            response = await client.GetAsync(url);

            try
            {
                var responseString = await response.Content.ReadAsStringAsync();
                NewsModel responseModel = JsonConvert.DeserializeObject<NewsModel>(responseString);

                var message = context.MakeMessage();
                await context.PostAsync("Here are some of the top headlines for today.\n\n(news provided by Australian Financial Review)");
                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = new List<Attachment>();

                foreach (var news in responseModel.Articles)
                {
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: news.UrlToImage));

                    List<CardAction> cardButtons = new List<CardAction>();

                    CardAction plButton = new CardAction()
                    {
                        Value = news.Url,
                        Type = "openUrl",
                        Title = "Link To Article"
                    };

                    cardButtons.Add(plButton);

                    HeroCard plCard = new HeroCard()
                    {
                        Title = news.Title,
                        Subtitle = news.Description,
                        Images = cardImages,
                        Buttons = cardButtons
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    message.Attachments.Add(plAttachment);
                }

                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
            catch
            {
                var message = "Unable to fetch news data.\n\nPlease try again.";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("Login")]
        public async Task Login(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();

            List<CardAction> cardButtons = new List<CardAction>();

            CardAction plButton = new CardAction()
            {
                Value = $"https://<OAuthSignInURL",
                Type = "signin",
                Title = "Connect"
            };

            cardButtons.Add(plButton);

            SigninCard plCard = new SigninCard(text: "Please sign in here", buttons: cardButtons);

            Attachment plAttachment = plCard.ToAttachment();
            message.Attachments.Add(plAttachment);

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Contact")]
        public async Task Contact(IDialogContext context, LuisResult result)
        {
            List<bankdetails> bankList = await AzureManager.AzureManagerInstance.GetBankList();

            var message = "";

            foreach (bankdetails bank in bankList)
            {
                message += bank.Name + "\n\n";
            }

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
    }
}