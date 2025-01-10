using Microsoft.EntityFrameworkCore;
using Library.Models;

namespace Library.LibraryData
{
    // Hanterar anslutning till databasen genom Entity Framework 
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Lending> Lendings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Lending>()
                .HasKey(l => l.LendingId); //Primärnyckel

            modelBuilder.Entity<Lending>() 
                .HasOne(l => l.Book)
                .WithMany() //En bok kan vara utlånad flera gånger
                .HasForeignKey(l => l.BookId);

            modelBuilder.Entity<Lending>()
                .HasOne(l => l.User)
                .WithMany() //En användare kan låna flera böcker
                .HasForeignKey(l => l.UserId); 
        }
    }
}
