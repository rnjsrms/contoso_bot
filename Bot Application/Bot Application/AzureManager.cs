using Bot_Application.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bot_Application
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<bankdetails> bankTable;
        private IMobileServiceTable<userdetails> userTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("https://rnjsrms-contoso.azurewebsites.net");
            this.bankTable = this.client.GetTable<bankdetails>();
            this.userTable = this.client.GetTable<userdetails>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<bankdetails>> GetBankList()
        {
            return await this.bankTable.ToListAsync();
        }

        public async Task<List<userdetails>> GetUserList()
        {
            return await this.userTable.ToListAsync();
        }

        public async Task PostUser(userdetails user)
        {
            await this.userTable.InsertAsync(user);
        }

        public async Task UpdateUser(userdetails user)
        {
            await this.userTable.UpdateAsync(user);
        }

        public async Task DeleteUser(userdetails user)
        {
            await this.userTable.DeleteAsync(user);
        }
    }
}