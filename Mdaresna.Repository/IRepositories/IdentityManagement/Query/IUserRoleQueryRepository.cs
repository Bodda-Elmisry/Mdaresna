﻿using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.IdentityManagement.Query
{
    public interface IUserRoleQueryRepository : IBaseQueryRepository<UserRole>
    {
        Task<bool> CheckUserRole(UserRole userRole);
    }
}
