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

        // Existing DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }

        // Add the Bookings DbSet
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            string schema = "ADMIN"; // Replace with your actual schema name

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS", schema);

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
                      .HasMaxLength(64);

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
                entity.ToTable("ROOMS", schema);

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Capacity)
                      .IsRequired();
            });

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("BOOKINGS", schema);

                entity.HasKey(e => e.Id);

                entity.Property(e => e.StartDate)
                      .IsRequired();

                entity.Property(e => e.EndDate)
                      .IsRequired();

                // Configure relationships
                entity.HasOne(e => e.Room)
                      .WithMany()
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}