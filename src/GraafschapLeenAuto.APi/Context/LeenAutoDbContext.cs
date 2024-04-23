using GraafschapLeenAuto.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace GraafschapLeenAuto.Api.Context
{
    public class LeenAutoDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public LeenAutoDbContext(DbContextOptions<LeenAutoDbContext> options)
            : base(options)
        {
        }
    }
}

