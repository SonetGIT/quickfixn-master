using Microsoft.EntityFrameworkCore;
using SimpleAcceptor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleAcceptor
{
    class EFDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("User ID=postgres;Password=postgres;Server=158.181.144.2;Port=63278;Database=kse;Integrated Security=true;Pooling=true;");

        public DbSet<financeInstrument> financeInstruments { get; set; }
        public DbSet<account> accounts { get; set; }
        public DbSet<accountType> accountTypes { get; set; }
    }
}
