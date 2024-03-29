using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Configrations
{
    public static class DependencyInjectionConfig
    {
        public static void ConfigerBaseRepos(IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseSharedRepository<>), typeof(BaseSharedRepository<>));
        }
    }
}
