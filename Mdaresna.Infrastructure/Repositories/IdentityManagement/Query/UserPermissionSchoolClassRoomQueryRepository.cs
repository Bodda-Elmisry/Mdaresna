using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.IdentityManagement.Query
{
    public class UserPermissionSchoolClassRoomQueryRepository : BaseQueryRepository<UserPermissionSchoolClassRoom>, IUserPermissionSchoolClassRoomQueryRepository
    {
       public UserPermissionSchoolClassRoomQueryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
