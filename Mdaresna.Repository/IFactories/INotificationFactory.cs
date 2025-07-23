using Mdaresna.Doamin.Enums;
using Mdaresna.Repository.IBServices.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IFactories
{
    public interface INotificationFactory
    {
        INotificationService GetProvider(NotificationProvidersEnum provider);
    }
}
