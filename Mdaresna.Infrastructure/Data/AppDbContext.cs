using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mdaresna.Doamin.Models.Identity;

namespace Mdaresna.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Keies

            modelBuilder.Entity<RolePermission>().HasKey(r => new { r.RoleId, r.PermissionId });
            modelBuilder.Entity<UserRole>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<UserPermission>().HasKey(r => new { r.UserId, r.PermissionId });
            modelBuilder.Entity<UserPermissionSchoolClassRoom>().HasKey(r => new { r.UserId, r.PermissionId, r.SchoolId, r.ClassRoomId });

            #endregion
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserPermission> userPermissions { get; set; }
        public DbSet<UserPermissionSchoolClassRoom> userPermissionSchoolClassRooms { get; set; }
    }
}
