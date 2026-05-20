using AutoParkManager.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Remoting.Contexts;

namespace AutoParkManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<Abonement> Abonements { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ParkingSession> ParkingSessions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext() : base("Server=localhost;Database=AutoParkDB;Trusted_Connection=True;")
        {
        }
    }
}