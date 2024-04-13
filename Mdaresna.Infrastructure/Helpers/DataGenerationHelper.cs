using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Helpers
{
    public class DataGenerationHelper
    {
        public static Guid GenerateRowId()
        {
            return Guid.NewGuid();
        }
    }
}
