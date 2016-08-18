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
    public class UpdateOrderTotalPrice : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Connection
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            // Debug.Log
            ITracingService trace = serviceProvider.GetService(typeof(ITracingService)) as ITracingService;

            // if this is in fact an Associate event
            if (String.Equals("Associate", context.MessageName, StringComparison.OrdinalIgnoreCase) &&
                context.Stage == 40 &&
                context.InputParameters.Contains("Target") &&
                context.InputParameters.Contains("Relationship"))
            {
                // Get the parent entity reference from the Target context
                EntityReference target = context.InputParameters["Target"] as EntityReference;
                // Get the related child entity reference from the "RelatedEntities" context.InputParameters
                // EntityReferenceCollection related = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;
                // Now get the "Relationship" from context.
                Relationship relationship = context.InputParameters["Relationship"] as Relationship;

                if(relationship.SchemaName == "new_order_babikov_storeproduct_babikov")
                {
                    if (target.LogicalName == "new_order_babikov")
                    {
                        UpdateContent(target.Id, service, trace);
                    }
                }
            }
        }
        /// <summary>
        /// Update Order record based on linked products
        /// </summary>
        /// <param name="orderid">Current order Id</param>
        /// <param name="service">IOrganizationService</param>
        /// <param name="trace">Debug Log tracking</param>
        private void UpdateContent(Guid orderid, IOrganizationService service, ITracingService trace)
        {
            // Get products associated with order
            QueryExpression query = new QueryExpression("new_storeproduct_babikov")
            {
                ColumnSet = new ColumnSet("new_name","new_price"),
                LinkEntities =
                {
                    new LinkEntity("new_storeproduct_babikov","new_order_babikov_storeproduct_babikov","new_storeproduct_babikovid","new_storeproduct_babikovid", JoinOperator.Inner)
                    {
                        LinkEntities =
                        {
                            new LinkEntity("new_order_babikov_storeproduct_babikov","new_order_babikov","new_order_babikovid","new_order_babikovid", JoinOperator.Inner)
                            {
                                LinkCriteria = new FilterExpression(LogicalOperator.And)
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("new_order_babikovid",ConditionOperator.Equal, orderid)
                                    }
                                }
                            }
                        }
                    }
                }
            };
            IReadOnlyCollection<Entity> products = service.RetrieveMultiple(query).Entities;

            // If we get something
            if(products.Count > 0)
            {
                // Debug Log
                foreach (var item in products)
                {
                    trace.Trace("Product name: " + item.GetAttributeValue<string>("new_name"));
                }
                // Create new order type entity
                Entity newOrder = new Entity()
                {
                    Id = orderid,
                    LogicalName = "new_order_babikov"
                };
                // Initialize discount and totalprice
                int discount = 0;
                decimal price = 0m;

                // Get discount based on product qty
                if (products.Count < 2)
                    discount = 0;
                else if (products.Count > 11)
                    discount = 50;
                else
                    discount = 5 * (products.Count - 1);

                // Get total price
                foreach (var item in products)
                {
                    // Always make this checks with Money, OptionSet and EntityReference
                    Money priceObj = item.GetAttributeValue<Money>("new_price");
                    if(priceObj != null)
                    {
                        price += priceObj.Value;
                    }
                    trace.Trace("Price: " + price.ToString());
                }
                // Set attributes
                newOrder["new_discount"] = discount;
                newOrder["new_totalprice"] = new Money() { Value = price - price * discount / 100 };//{ Value = price }; // 
                // Update current order
                service.Update(newOrder);
            }
        }
    }
}
