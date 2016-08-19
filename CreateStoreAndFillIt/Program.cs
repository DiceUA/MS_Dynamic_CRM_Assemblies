using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateStoreAndFillIt
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Initialization
            RandomGenerator.Initialize();
            var connection = new CrmConnection("Crm");
            IOrganizationService service = new OrganizationService(connection);

            int storeCount = 1;
            int productCount = 2;
            #endregion
            // How to check if connection established or there is internet problems? Or incorrect user or password

            Console.WriteLine("Service initialized");


            // Create 10 stores
            for (int i = 0; i < storeCount; i++)
            {
                var store = CreateNewStore(service);
                // Create 20 products for each store
                if (store != null)
                {
                    for (int j = 0; j < productCount; j++)
                    {
                        CreateNewProductForStore(store, service);
                    }
                }
            }                        
            Console.WriteLine("All done. Refresh CRM to see changes");
            Console.ReadKey();
        }

        // Create random store new_store_babikov
        /// <summary>
        /// Creates new store with random GUID 
        /// and name Store+RandomNumber
        /// </summary>
        /// <param name="store"></param>
        /// <returns> Created Store Entity </returns>
        static Entity CreateNewStore(IOrganizationService service)
        {
            Entity newStore = new Entity("new_store_babikov")
            {
                Id = Guid.NewGuid(),
                Attributes =
                {
                    {
                        "new_name",
                        String.Format("{0}-{1}", "Store", RandomGenerator.RandomString(5))
                    }
                    // if showcase product type needed
                    // { "new_showcaseproductstype", new OptionSetValue(RandomGenerator.Next(3) + 100000000) }
                }
            };
            service.Create(newStore);
            return newStore;
        }
        /// <summary>
        /// Creates new product with random GUID for store
        /// </summary>
        /// <param name="store">Store Entity</param>
        /// <param name="service">Connection</param>
        /// <returns></returns>
        static void CreateNewProductForStore(Entity store, IOrganizationService service)
        {
            Entity newProduct = new Entity("new_storeproduct_babikov")
            {
                Id = Guid.NewGuid(),
                Attributes =
                {
                    {
                        "new_name",
                        String.Format("{0}-{1}", "Product",RandomGenerator.RandomString(8))
                    },
                    { "new_price", new Money(RandomGenerator.NextDecimal(15000)) },
                    { "new_storeid", new EntityReference("new_store_babikov", store.Id) },
                    { "new_producttype", new OptionSetValue(RandomGenerator.Next(3) + 100000000) }
                }
            };
            service.Create(newProduct);
        }
    }
}
