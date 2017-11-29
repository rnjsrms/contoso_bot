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
    public class LogoutDialog : IDialog<object>
    {
        string value;
        userdetails user;

        public async Task StartAsync(IDialogContext context)
        {
            user = LuisDialog.user;
            context.Wait(AskLogout);
        }

        public virtual async Task AskLogout(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var response = await activity;
            var yesno = response.Text;

            if (yesno.ToLower() == "no")
            {
                await context.PostAsync($"Log out cancelled!");
                context.EndConversation($"Log out cancelled by user.");
            }
            else
            {
                LuisDialog.user = null;

                var message = "You have logged out.";
                await context.PostAsync(message);

                context.Done(this);
            }
        }
    }
}