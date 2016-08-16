using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Education.Babikov.Plugins
{
    public class UpdateStoreShowCaseFromStore : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            if (String.Equals("Update", context.MessageName, StringComparison.OrdinalIgnoreCase) && 
                context.Stage == 40 && 
                context.PrimaryEntityName == "new_store_babikov")
            {
                Entity target = context.InputParameters["Target"] as Entity;
                OptionSetValue chosenOption = target.GetAttributeValue<OptionSetValue>("new_showcaseproductstype");

                if (chosenOption == null)
                    return;

                // Make query to get products filtered by shop and type
                QueryExpression query = new QueryExpression("new_storeproduct_babikov")
                {
                    // Get needed colums Name and Price
                    ColumnSet = new ColumnSet("new_name","new_price"),
                    Distinct = true,
                    Criteria = new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            // Filter by type and store
                            // DO NOT get records that belong to another store
                            new ConditionExpression("new_producttype", ConditionOperator.Equal, chosenOption.Value),
                            new ConditionExpression("new_storeid",ConditionOperator.Equal, target.Id)
                        }
                    },
                    Orders =
                    {
                        new OrderExpression("new_price", OrderType.Ascending)
                    }
                };

                // Get collection of products products
                IReadOnlyCollection<Entity> products = service.RetrieveMultiple(query).Entities;

                Entity newEnt = new Entity("new_store_babikov")
                {
                    Id = target.Id
                };
                // clear text box, to make += String.Format working
                newEnt["new_showcaseproducts"] = "";

                // Fill result string with records
                foreach (var item in products)
                {                    
                    newEnt["new_showcaseproducts"] += String.Format("{0} - {1}; ", item.GetAttributeValue<string>("new_name"), item.GetAttributeValue<Money>("new_price").Value.ToString());
                }
                service.Update(newEnt);
            }
        }
    }
}
