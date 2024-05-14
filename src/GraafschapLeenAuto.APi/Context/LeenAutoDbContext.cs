using GraafschapLeenAuto.Api.Entities;
using GraafschapLeenAuto.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GraafschapLeenAuto.Api.Context
{
    public class LeenAutoDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public LeenAutoDbContext(DbContextOptions<LeenAutoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 2,
                    Name = "User",
                    Email = "user@example.com",
                    Password = "UserPassword"
                });

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Name = "Admin"
                },
                new Role
                {
                    Id = 2,
                    Name = "User"
                });
        }
    }
}

