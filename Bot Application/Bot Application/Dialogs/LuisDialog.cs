using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Linq;

namespace Bot_Application.Dialogs
{
    [LuisModel("bc5ca548-9a6b-47b1-b003-e76f03c41ede", "4d8f8aeed14f4e90a526fa12711136bf", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        public async Task All(IDialogContext context, LuisResult result)
        {
            string message = $"Detected intent: " + string.Join(", ", result.Intents.Select(i => i.Intent));
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





    }
}