using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Hilpers
{
    public class DataGenerationHilper
    {
        public static Guid GenerateRowId()
        {
            return Guid.NewGuid();
        }
    }
}
