﻿using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class LoginResultDTO
    {
        public User LogedinUser { get; set; }
        public IEnumerable<string> Permissions { get; set; }
    }
}