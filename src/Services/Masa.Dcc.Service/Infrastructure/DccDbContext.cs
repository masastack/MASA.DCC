using System.Reflection;

namespace Masa.Dcc.Service.Infrastructure
{
    public class DccDbContext : MasaDbContext
    {
        public DccDbContext(MasaDbContextOptions<DccDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreatingExecuting(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreatingExecuting(builder);
        }
    }
}