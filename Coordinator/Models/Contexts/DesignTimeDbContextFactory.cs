using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Coordinator.Models.Contexts
{
    public class DesignTimeDbContextFactory: IDesignTimeDbContextFactory<TwoPhaseCommitContext>
    {
        public TwoPhaseCommitContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TwoPhaseCommitContext>();
            var connectionString = "Server =.\\SQL22; Database = TwoPhaseCommitDB; User ID = sa; Password = master.12; TrustServerCertificate = True;";
            builder.UseSqlServer(connectionString);
            return new TwoPhaseCommitContext(builder.Options);
        }
    }
}
