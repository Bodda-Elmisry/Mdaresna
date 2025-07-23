using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IBServices.Common
{
    public interface INotificationService
    {
        Task SendAsync(string token, string title, string body);
        Task SendToMultiUsersAsync(List<string> tokens, string title, string body);
    }
}
