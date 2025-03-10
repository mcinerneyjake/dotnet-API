using Microsoft.EntityFrameworkCore;

namespace TheEmployeeAPI;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }
  public DbSet<Employee> Employees { get; set; }
}