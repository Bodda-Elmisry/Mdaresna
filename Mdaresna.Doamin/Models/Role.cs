﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models
{
    public class Role
    {

        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public bool SchoolRole { get; set; }
        public bool AdminRole { get; set; }
    }
}