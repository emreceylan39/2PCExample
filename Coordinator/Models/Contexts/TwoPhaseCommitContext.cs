using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace Coordinator.Models.Contexts
{
    public class TwoPhaseCommitContext : DbContext
    {
        public TwoPhaseCommitContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeState> NodeStates { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server =.\\SQL22; Database = TwoPhaseCommitDB; User ID = sa; Password = master.12; TrustServerCertificate = True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Node>().HasData(
                new Node("Order.API") { Id = Guid.NewGuid() },
                 new Node("Payment.API") { Id = Guid.NewGuid() },
                 new Node("Stock.API") { Id = Guid.NewGuid() }

                 );
        }
    }
}
