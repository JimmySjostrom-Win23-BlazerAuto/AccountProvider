using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Data.Contexts;

namespace Data.Factories;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
	public DataContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
		optionsBuilder.UseSqlServer("Server=tcp:sqlserver-silicon-js.database.windows.net,1433;Initial Catalog=accounts;Persist Security Info=False;User ID=SqlAdmin;Password=oG%77_sE123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

		return new DataContext(optionsBuilder.Options);
	}
}