using Microsoft.EntityFrameworkCore;
using RoomReservationSystem.Models;

namespace RoomReservationSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for your entities
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }

        // Configure entity relationships and constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define the schema if necessary, e.g., "ADMIN"
            string schema = "ADMIN"; // Replace with your actual schema name

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS", schema); // Ensure table name and schema are correct

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.FirstName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.LastName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                      .IsRequired()
                      .HasMaxLength(64); // SHA256 hash length

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("User");

                entity.Property(e => e.IsActive)
                      .HasColumnName("IS_ACTIVE")
                      .HasMaxLength(1)
                      .IsRequired();
            });

            // Configure Room entity
            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("ROOMS", schema); // Ensure table name and schema are correct

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Capacity)
                      .IsRequired();

                // Add other property configurations as needed
            });
        }
    }
}
