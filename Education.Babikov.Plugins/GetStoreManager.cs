using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;

namespace Education.Babikov.Plugins
{
    public class GetStoreManager : CodeActivity
    {
        // Declare parameters
        [RequiredArgument]
        [Input("Store Reference")]
        [ReferenceTarget("new_store_babikov")]
        public InArgument<EntityReference> StoreRef { get; set; }
        [Output("Contact Reference")]
        [ReferenceTarget("contact")]
        public OutArgument<EntityReference> ContactRef { get; set; }



        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory factory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = factory.CreateOrganizationService(workflowContext.UserId);
            // Debug Log ? Will it work here?
            //ITracingService trace = context.GetExtension<ITracingService>();

            EntityReference storeRef = StoreRef.Get(context);            

            if(storeRef == null)
            {
                throw new InvalidPluginExecutionException("Store not found in workflow settings");
            }

            // Get all contacts attached to store
            QueryExpression query = new QueryExpression("contact")
            {
                // preferredcontactmethodcode - optionset
                // where email value = 2
                // emailaddress1 - string
                ColumnSet = new ColumnSet("emailaddress1", "preferredcontactmethodcode"),
                LinkEntities =
                {
                    new LinkEntity("contact","new_store_babikov_contact","contactid","contactid", JoinOperator.Inner)
                    {
                        LinkEntities =
                        {
                            new LinkEntity("new_store_babikov_contact","new_store_babikov","new_store_babikovid","new_store_babikovid", JoinOperator.Inner)
                            {
                                LinkCriteria = new FilterExpression(LogicalOperator.And)
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("new_store_babikovid",ConditionOperator.Equal, storeRef.Id)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            IReadOnlyCollection<Entity> contacts = service.RetrieveMultiple(query).Entities;
            if(contacts.Count > 0)
            {
                foreach (var item in contacts)
                {
                    OptionSetValue contactMethod = item.GetAttributeValue<OptionSetValue>("preferredcontactmethodcode");
                    string email = item.GetAttributeValue<string>("emailaddress1");
                    
                    // Email must be filled, Preffered contact method must be Email = 2
                    if (contactMethod != null && contactMethod.Value == 2 && !String.IsNullOrWhiteSpace(email))
                    {
                        // Return that contact reference
                        ContactRef.Set(context,
                            new EntityReference()
                            {
                                Id = item.Id,
                                LogicalName = item.LogicalName
                            });
                        return; // and exit
                    }
                }
            }
        }
    }
}
