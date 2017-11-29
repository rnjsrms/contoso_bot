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
    public class RegistrationDialog : IDialog<object>
    {
        string username;
        string fullName;
        string address;
        string mobile;
        string home;
        DateTime dateOfBirth;
        string email;
        string password;
        private static IEnumerable<string> cancelTerms = new[] { "cancel", "back", "abort" };

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Great! We can register you as a new user.\n\nWhat is your full name?");
            context.Wait(AskUsername);
        }

        public virtual async Task AskUsername(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var response = await activity;
            fullName = response.Text;

            if (cancelTerms.Contains(fullName.ToLower()))
            {
                await context.PostAsync("Registration cancelled!");
                context.EndConversation("Registration cancelled by user.");
            }

            PromptDialog.Text(
                context: context,
                resume: AskAddress,
                prompt: $"Hello {fullName}!\n\nWhat should your username be?",
                retry: "Sorry, I didn't understand that. Could you please try again?\n\nWhat should your username be?"
            );
        }

        public virtual async Task AskAddress(IDialogContext context, IAwaitable<string> Username)
        {
            username = await Username;

            if (cancelTerms.Contains(username.ToLower()))
            {
                await context.PostAsync("Registration cancelled!");
                context.EndConversation("Registration cancelled by user.");
            }

            PromptDialog.Text(
                context: context,
                resume: AskMobile,
                prompt: "What is your home address?\n\n(example: 100 Symonds St, Auckland Central, Auckland)",
                retry: "Sorry, I didn't understand that. Please try again.\n\nWhat is your home address?\n\n(example: 100 Symonds St, Auckland Central, Auckland)"
            );
        }

        public virtual async Task AskMobile(IDialogContext context, IAwaitable<string> Address)
        {
            address = await Address;

            if (cancelTerms.Contains(address.ToLower()))
            {
                await context.PostAsync("Registration cancelled!");
                context.EndConversation("Registration cancelled by user.");
            }

            PromptDialog.Text(
                context: context,
                resume: AskHome,
                prompt: "(OPTIONAL) What is your mobile phone number?\n\n(example: 020-1234567)",
                retry: "Sorry, I didn't understand that. Please try again.\n\n(OPTIONAL) What is your mobile phone number?\n\n(example: 020-1234567)"
            );
        }

        public virtual async Task AskHome(IDialogContext context, IAwaitable<string> Mobile)
        {
            mobile = await Mobile;

            if (cancelTerms.Contains(mobile.ToLower()))
            {
                await context.PostAsync("Registration cancelled!");
                context.EndConversation("Registration cancelled by user.");
            }

            PromptDialog.Text(
                context: context,
                resume: AskDOB,
                prompt: "(OPTIONAL) What is your home phone number?\n\n(example: 09-1234567)",
                retry: "Sorry, I didn't understand that. Please try again.\n\n(OPTIONAL) What is your home phone number?\n\n(example: 09-1234567)"
            );
        }

        public virtual async Task AskDOB(IDialogContext context, IAwaitable<string> Home)
        {
            home = await Home;

            if (cancelTerms.Contains(home.ToLower()))
            {
                await context.PostAsync("Registration cancelled!");
                context.EndConversation("Registration cancelled by user.");
            }

            PromptDialog.Text(
                context: context,
                resume: AskEmail,
                prompt: "What is your date of birth?\n\n(example: 1995-12-03)",
                retry: "Sorry, I didn't understand that. Please try again.\n\nWhat is your date of birth?\n\n(example: 1995-12-03)"
            );
        }

        public virtual async Task AskEmail(IDialogContext context, IAwaitable<string> DOB)
        {
            dateOfBirth = Convert.ToDateTime(await DOB);

            PromptDialog.Text(
                context: context,
                resume: AskPassword,
                prompt: "What is your email address?\n\n(example: rep@contoso.com)",
                retry: "Sorry, I didn't understand that. Please try again.\n\nWhat is your email address?\n\n(example: rep@contoso.com)"
            );
        }

        public virtual async Task AskPassword(IDialogContext context, IAwaitable<string> Email)
        {
            email = await Email;

            PromptDialog.Text(
                context: context,
                resume: Final,
                prompt: "Please enter your password.",
                retry: "Sorry, I didn't understand that. Please try again.\n\nPlease enter your password."
            );
        }

        public virtual async Task Final(IDialogContext context, IAwaitable<string> Password)
        {
            password = await Password;

            userdetails user = new userdetails()
            {
                Username = username,
                FullName = fullName,
                Address = address,
                Mobile = mobile,
                Home = home,
                DateOfBirth = dateOfBirth,
                Email = email,
                Password = password
            };
            var message = $"Thanks {fullName}!\n\nYour account is now being created.\n\nPlease wait a few moments as this may take a while...";
            await context.PostAsync(message);

            await AzureManager.AzureManagerInstance.PostUser(user);

            message = $"Account Details\n\nUsername: {username}\n\nName: {fullName}\n\nAddress: {address}\n\nMobile Phone: {mobile}\n\nHome Phone: {home}\n\nDate of Birth: {dateOfBirth.ToString("dd-MM-yyyy")}\n\nEmail: {email}";
            await context.PostAsync(message);

            await context.PostAsync("Thank you for registering with Contoso Bank.\n\nYou can now log in by entering: 'login' and following the steps.");
            context.Done(this);
        }
    }
}