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
    public class HandleOrderStatusChange : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Connection
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            // Debug.Log
            ITracingService trace = serviceProvider.GetService(typeof(ITracingService)) as ITracingService;

            trace.Trace("Action Plugin initialized!");

            if (String.Equals("new_Action_OrderStatusChange_babikov", context.MessageName, StringComparison.OrdinalIgnoreCase) &&
                context.Stage == 40)
            {
                // Here we can make changes
                // For example

                // Access input parameters
                string subject = context.InputParameters["Subject"] as string;
                //int? body = (int?)context.InputParameters["Body"];
                subject = " (ActionPlugin)";
                //body += 33;

                // Update input parameters
                context.InputParameters["Subject"] = subject;
                //context.InputParameters["Body"] = body;

                // Update Output parameters
                //context.OutputParameters["Contact"] = new EntityReference("contact", Guid.NewGuid());
            }
        }
    }
}
