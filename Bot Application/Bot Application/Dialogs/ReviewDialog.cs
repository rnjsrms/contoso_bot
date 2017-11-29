using Bot_Application.Models;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bot_Application.Dialogs
{
    [Serializable]
    public class ReviewDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Thank you for choosing to review our bot.\n\nPlease tell us about your experience (~2 sentences).");
            context.Wait(DoTask);
        }

        public virtual async Task DoTask(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var response = await activity;
            var responseText = response.Text;

            // Create a client.
            ITextAnalyticsAPI client = new TextAnalyticsAPI();
            client.AzureRegion = AzureRegions.Westcentralus;
            client.SubscriptionKey = "af0b5df9f3e34a45b7eb149b8a911782";

            SentimentBatchResult result3 = client.Sentiment(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput("en", "0", responseText)
                        }));

            // Printing language results.
            var score = result3.Documents.First().Score;

            reviewdetails review = new reviewdetails()
            {
                Review = responseText,
                SentimentScore = (double) score
            };

            await AzureManager.AzureManagerInstance.PostReview(review);

            var message = "Thank you for reviewing our bot.\n\nWe will continue to improve this bot further.";
            await context.PostAsync(message);

            context.Done(this);
        }
    }
}