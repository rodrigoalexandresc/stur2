using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace STUR_mvc.Models {
    public class STURDBContext : DbContext {

        public STURDBContext(DbContextOptions<STURDBContext> options) : base(options)
        {
            
        }

        public DbSet<Imposto> Impostos { get; set; }
        public DbSet<Lote> Lotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new ImpostoMap(modelBuilder.Entity<Imposto>());
            new LoteMap(modelBuilder.Entity<Lote>());
        }
    }

}