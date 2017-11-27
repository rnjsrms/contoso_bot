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
    public class LoginDialog : IDialog<object>
    {
        string username;
        string password;
        List<userdetails> userList;

        public LoginDialog(List<userdetails> UserList)
        {
            userList = (List<userdetails>) UserList;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("What is your username?");
            context.Wait(AskPassword);
        }

        public virtual bool CheckLogin()
        {
            foreach (userdetails user in userList)
            {
                if ((user.Username == username) && (user.Password == password))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual async Task AskPassword(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var response = await activity;
            username = response.Text;

            PromptDialog.Text(
                context: context,
                resume: Final,
                prompt: $"Hi {username}!\n\nWhat is your password?",
                retry: "Sorry, I didn't understand that. Could you please try again?\n\nWhat is your password?"
            );
        }

        public virtual async Task Final(IDialogContext context, IAwaitable<string> Password)
        {
            password = await Password;

            if (CheckLogin())
            {
                context.Done(this);
            }
            else
            {
                await context.PostAsync("Username and password do not match!\n\nPlease try again...");
                context.Call<object>(new LoginDialog(userList), LoginComplete);
            }
        }

        public virtual async Task LoginComplete(IDialogContext context, IAwaitable<object> response)
        {
            context.Done(this);
        }
    }
}