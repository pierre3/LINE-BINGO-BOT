using Microsoft.EntityFrameworkCore;

namespace BingoWebApi.Models
{
    public class BingoApiContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<DrawSource> DrawSource { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardCell> CardCells { get; set; }

        public BingoApiContext(DbContextOptions<BingoApiContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<DrawSource>()
                .HasKey(data => new { data.GameId, data.Index });
            modelBuilder.Entity<CardCell>()
                .HasKey(data => new { data.CardId, data.Index });

            base.OnModelCreating(modelBuilder);
        }

    }
}
