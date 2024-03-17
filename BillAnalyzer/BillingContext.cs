using BillAnalyzer.Models;
using Microsoft.EntityFrameworkCore;

namespace BillAnalyzer;

public class BillingContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<PayCheck> PayChecks { get; set; }
    public DbSet<BillItem> Bills { get; set; }

    public BillingContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        _dbPath = Path.Join(path, "billing.db");
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
}