using FirebaseAdmin.Messaging;
using Mdaresna.Doamin.Enums;
using Mdaresna.Infrastructure.BServices.Common;
using Mdaresna.Repository.IBServices.Common;
using Mdaresna.Repository.IFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Factories
{
    public class NotificationFactory : INotificationFactory
    {
        private readonly IServiceProvider serviceProvider;

        public NotificationFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public INotificationService GetProvider(NotificationProvidersEnum provider)
        {
            try
            {
                var result = provider switch
                {
                    NotificationProvidersEnum.Mobile => (INotificationService)serviceProvider.GetService(typeof(FcmService)),
                    NotificationProvidersEnum.Hub => (INotificationService)serviceProvider.GetService(typeof(NotificationHubService)),
                    _ => throw new ArgumentException("Invalid notification type")
                };

                return result;
            }
            catch(Exception ex)
            {
                // Log the exception or handle it as needed
                throw new InvalidOperationException("Error retrieving notification service provider", ex);
            }
            
        }
    }
}
