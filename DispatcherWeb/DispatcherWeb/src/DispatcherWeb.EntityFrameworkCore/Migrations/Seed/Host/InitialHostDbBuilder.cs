using DispatcherWeb.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed.Host
{
    public class InitialHostDbBuilder
    {
        private readonly DispatcherWebDbContext _context;

        public InitialHostDbBuilder(DispatcherWebDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            new DefaultEditionCreator(_context).Create();
            new DefaultLanguagesCreator(_context).Create(null);
            new HostRoleAndUserCreator(_context).Create();
            new DefaultSettingsCreator(_context).Create(null);

            _context.SaveChanges();
        }
    }
}
