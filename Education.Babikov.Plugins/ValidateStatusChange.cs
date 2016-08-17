﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;


namespace Education.Babikov.Plugins
{
    public class ValidateStatusChange : IPlugin
    {
        
        

        public void Execute(IServiceProvider serviceProvider)
        {
            // Connection
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            // Debug.Log
            ITracingService trace = serviceProvider.GetService(typeof(ITracingService)) as ITracingService;

            // Check whether we have image
            if (String.Equals("Update", context.MessageName, StringComparison.OrdinalIgnoreCase) &&
                context.Stage == 40 &&
                context.PrimaryEntityName == "new_order_babikov" &&
                context.PreEntityImages.ContainsKey("Image"))
            {
                // Get new entity values
                Entity target = context.InputParameters["Target"] as Entity;
                // Get old entity values
                Entity preImage = context.PreEntityImages["Image"];
                // Get option new and old values
                int newOptionValue = target.GetAttributeValue<OptionSetValue>("new_statuscode").Value;                
                OptionSetValue oldOption = preImage.GetAttributeValue<OptionSetValue>("new_statuscode");
                int oldOptionValue = 0;
                // If option value unassigned set value to first
                if (oldOption == null)
                    oldOptionValue = 100000000;
                else
                    oldOptionValue = oldOption.Value;

                // New - 100 000 000
                // Confirmed - 100 000 001 etc
                // User can only change option to next or previous.
                if (newOptionValue > oldOptionValue + 1 || newOptionValue < oldOptionValue - 1)
                {
                    throw new InvalidPluginExecutionException("Invalid status change!");
                }
            }
        }
    }
}