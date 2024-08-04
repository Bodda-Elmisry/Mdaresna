using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Helpers
{
    public static class SettingsHelper
    {
        private static string AppURL;

        public static void SetAppUrl(string url)
        {
            AppURL = url;
        }

        public static string GetAppUrl()
        {
            return AppURL;
        }
    }
}
