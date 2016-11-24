using GrandiosoContoso.DataModels;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GrandiosoContoso
{
    public class AzureManager
    {
        private static AzureManager azureInstance;
        private MobileServiceClient mobileClient;
        private IMobileServiceTable<GrandiosoContosoReview> reviewTable;

        private AzureManager()
        {
            this.mobileClient = new MobileServiceClient("https://lif.azurewebsites.net/");
            this.reviewTable = this.mobileClient.GetTable<GrandiosoContosoReview>();
        }

        public MobileServiceClient AzureClient
        {
            get
            {
                return mobileClient;
            }
        }

        public static AzureManager AzureInstance
        {
            get
            {
                if (azureInstance == null)
                {
                    azureInstance = new AzureManager();
                }

                return azureInstance;
            }
        }

        public async Task<List<GrandiosoContosoReview>> GetReviews()
        {
            return await this.reviewTable.ToListAsync();
        }

        public async Task AddReview(GrandiosoContosoReview review)
        {
            await this.reviewTable.InsertAsync(review);
        }

        public async Task UpdateReview(GrandiosoContosoReview review)
        {
            await this.reviewTable.UpdateAsync(review);
        }

        public async Task DeleteReview(GrandiosoContosoReview review)
        {
            await this.reviewTable.DeleteAsync(review);
        }
    }
}