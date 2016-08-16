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
    public class UpdateStoreShowCaseFromProduct : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            if (String.Equals("Create", context.MessageName, StringComparison.OrdinalIgnoreCase) &&
                context.Stage == 40 &&
                context.PrimaryEntityName == "new_storeproduct_babikov")
            {
                Entity target = context.InputParameters["Target"] as Entity;
                OptionSetValue chosenOption = target.GetAttributeValue<OptionSetValue>("new_producttype");
                string currentName = target.GetAttributeValue<string>("new_name");
                EntityReference currentShop = target.GetAttributeValue<EntityReference>("new_storeid");
                string currentPrice = target.GetAttributeValue<Money>("new_price").Value.ToString();

                // Make exception if current shop or option not selected - end plugin execution
                if (currentShop == null || chosenOption == null)
                    return;

                QueryExpression query = new QueryExpression("new_store_babikov")
                {
                    // Get needed string of showcase products
                    ColumnSet = new ColumnSet("new_showcaseproducts"),
                    Distinct = false,
                    Criteria = new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("new_showcaseproductstype", ConditionOperator.Equal, chosenOption.Value),
                            // Select only store that selected in Store field
                            new ConditionExpression("new_store_babikovid",ConditionOperator.Equal,target.GetAttributeValue<EntityReference>("new_storeid").Id)                            
                        }
                    }
                };
                IReadOnlyCollection<Entity> stores = service.RetrieveMultiple(query).Entities;                
                if (stores != null)
                {

                    Entity newProduct = new Entity("new_storeproduct_babikov")
                    {
                        Id = target.Id
                    };
                    newProduct["new_debuglog"] = currentName + "\n" + currentPrice + "\n" + currentShop.Id+ "\n";
                    //String.Format("Name: {0}\nShop: {1}\nPrice: {3}\n", currentName, currentShop, currentPrice); // Y not working?

                    foreach (var item in stores)
                    {
                        Entity newEnt = new Entity("new_store_babikov")
                        {
                            Id = item.Id
                        };
                        if (currentPrice != null && currentName != null && currentShop != null)
                        {
                            // resultString will be filled if ColumnSet in Query filled with correct field eg. new_showcaseproducts 
                            string resultString = item.GetAttributeValue<string>("new_showcaseproducts");                            
                            newEnt["new_showcaseproducts"] = resultString + currentName + " - " + currentPrice+"; ";  
                            // += not working here coz this parameter null when new entity created DO NOT FORGET!
                            //String.Format("{0} - {1}; ", target.GetAttributeValue<string>("new_name"), target.GetAttributeValue<Money>("new_price").Value.ToString());                            
                        }                            
                        // Update record
                        service.Update(newEnt);
                    }
                    
                    service.Update(newProduct);
                }
            }
        }
    }
}
