using Bot_Application.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bot_Application.Dialogs
{
    [Serializable]
    public class DeleteAccountDialog : IDialog<object>
    {
        string value;
        userdetails user;

        public async Task StartAsync(IDialogContext context)
        {
            user = LuisDialog.user;
            context.Wait(AskDelete);
        }

        public virtual async Task AskDelete(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var response = await activity;
            var yesno = response.Text;

            if (yesno.ToLower() == "no")
            {
                await context.PostAsync($"Account deletion cancelled!");
                context.EndConversation($"Account deletion cancelled by user.");
            }
            else
            {
                await AzureManager.AzureManagerInstance.DeleteUser(user);

                var message = "Account has been permanently deleted.";
                await context.PostAsync(message);

                LuisDialog.user = null;
                context.Done(this);
            }
        }
    }
}