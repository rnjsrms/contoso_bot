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
        userdetails user;
        List<userdetails> userList;

        private static IEnumerable<string> cancelTerms = new[] { "cancel", "back", "abort" };

        public LoginDialog(List<userdetails> UserList)
        {
            userList = UserList;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("What is your username?\n\n(to cancel say: 'cancel', 'back', or 'abort')");
            context.Wait(AskPassword);
        }

        public virtual bool CheckLogin()
        {
            foreach (userdetails u in userList)
            {
                if ((u.Username == username) && (u.Password == password))
                {
                    user = u;
                    return true;
                }
            }
            return false;
        }

        public virtual bool CheckUsername()
        {
            var nameList = new List<string>();

            foreach (userdetails user in userList)
            {
                nameList.Add(user.Username);
            }
            
            if (nameList.Contains(username)) {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual async Task AskPassword(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var response = await activity;
            username = response.Text;

            if (cancelTerms.Contains(username.ToLower()))
            {
                await context.PostAsync("Login cancelled!");
                context.EndConversation("Login cancelled by user.");
            }

            PromptDialog.Text(
                context: context,
                resume: Final,
                prompt: $"Hi {username}!\n\nWhat is your password?\n\n(to cancel say: 'cancel', 'back', or 'abort')",
                retry: "Sorry, I didn't understand that. Could you please try again?\n\nWhat is your password?"
            );
        }

        public virtual async Task Final(IDialogContext context, IAwaitable<string> Password)
        {
            password = await Password;
            
            if (cancelTerms.Contains(password.ToLower()))
            {
                await context.PostAsync("Login cancelled!");
                context.EndConversation("Login cancelled by user.");
            }
            else if (CheckLogin())
            {
                LuisDialog.user = user;
                await context.PostAsync("You are now logged in!");
                context.Done(this);
            }
            else if (!CheckUsername())
            {
                await context.PostAsync("Username does not exist!\n\nPlease try again...");
                context.Call<object>(new LoginDialog(userList), LoginComplete);
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