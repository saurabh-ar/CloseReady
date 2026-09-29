using CloseReady.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CloseReady.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    public DbSet<Client> Clients { get; set; }
    public DbSet<CollectionCycle> CollectionCycles { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<ClientUploadLink> ClientUploadLinks { get; set; }
    public DbSet<CycleDocument> CycleDocuments { get; set; }
    //add a small seed configuration so our database automatically contains the initial checklist.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<DocumentType>().HasData(
          new DocumentType { Id = 1, Name = "Bank Statements" },
          new DocumentType { Id = 2, Name = "Credit Card Statements" },
          new DocumentType { Id = 3, Name = "Payroll Reports" },
          new DocumentType { Id = 4, Name = "Sales Reports" },
          new DocumentType { Id = 5, Name = "AP Aging Report" },
          new DocumentType { Id = 6, Name = "AR Aging Report" },
          new DocumentType { Id = 7, Name = "Tax Documents" },
          new DocumentType { Id = 8, Name = "Supporting Documents" }
      );
      }
    }
}
