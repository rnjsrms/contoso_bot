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
    public class ManageAccountDialog : IDialog<object>
    {
        string accountdetail;
        string value;
        userdetails user;

        public ManageAccountDialog(string AccountDetail)
        {
            accountdetail = AccountDetail;
            user = LuisDialog.user;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(AskChange);
        }

        public virtual async Task AskChange(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var response = await activity;
            var yesno = response.Text;

            if (yesno.ToLower() == "no")
            {
                await context.PostAsync($"{accountdetail} change cancelled!");
                context.EndConversation($"{accountdetail} change cancelled by user.");
            }
            else
            {
                PromptDialog.Text(
                    context: context,
                    resume: Final,
                    prompt: $"What would you like your new {accountdetail} to be?",
                    retry: $"Sorry, I didn't understand that. Could you please try again?\n\nWhat would you like your new {accountdetail} to be?");
            }
        }

        public virtual async Task Final(IDialogContext context, IAwaitable<string> Value)
        {
            value = await Value;

            switch (accountdetail.ToLower())
            {
                case "email":
                    user.Email = value;
                    break;
                case "address":
                    user.Address = value;
                    break;
                case "mobile":
                    user.Mobile = value;
                    break;
                case "home":
                    user.Home = value;
                    break;
                case "password":
                    user.Password = value;
                    break;
                default:
                    string msg = $"Sorry, you cannot change {accountdetail}.";
                    await context.PostAsync(msg);
                    context.EndConversation($"{accountdetail} change cancelled by system.");
                    break;
            }

            await AzureManager.AzureManagerInstance.UpdateUser(user);

            var message = "";
            message = $"Updated Account Details\n\nName: {user.FullName}\n\nAddress: {user.Address}\n\nMobile Phone: {user.Mobile}\n\nHome Phone: {user.Home}\n\nDate of Birth: {user.DateOfBirth.ToString("dd-MM-yyyy")}\n\nEmail: {user.Email}";
            await context.PostAsync(message);

            LuisDialog.user = user;
            context.Done(this);
        }
    }
}