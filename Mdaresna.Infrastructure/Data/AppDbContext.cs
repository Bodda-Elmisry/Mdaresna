using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;

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
            
            modelBuilder.Entity<SchoolUser>().HasKey(s => new { s.UserId, s.SchoolId });

            #endregion


            #region Column Types

            modelBuilder
                .Entity<CoinType>()
                .Property(p=> p.Value)
                .HasColumnType("decimal(18,2)");

            modelBuilder
                .Entity<PaymentTransaction>()
                .Property(p=> p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder
                .Entity<SchoolPaymentRequest>()
                .Property(p=> p.TransfareAmount)
                .HasColumnType("decimal(18,2)");

            #endregion
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserPermission> userPermissions { get; set; }
        public DbSet<UserPermissionSchoolClassRoom> userPermissionSchoolClassRooms { get; set; }
        public DbSet<RelationType> relationTypes { get; set; }
        public DbSet<SchoolUser> SchoolUsers { get; set; }
        public DbSet<CoinType> CoinsTypes { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<SchoolPaymentRequest> SchoolPaymentRequests { get; set; }
        public DbSet<SchoolPost> SchoolPosts { get; set; }
        public DbSet<SchoolPostImage> SchoolPostImages { get; set; }


        
    }
}
