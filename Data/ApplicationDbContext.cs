// Data/ApplicationDbContext.cs
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
        public DbSet<Booking> Bookings { get; set; }

        // New DbSets
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Address> Addresses { get; set; }

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
                entity.ToTable("MISTNOSTI", schema);

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Capacity)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.Property(e => e.BuildingId)
                      .IsRequired();

                // Configure relationships
                entity.HasOne(e => e.Building)
                      .WithMany(b => b.Rooms)
                      .HasForeignKey(e => e.BuildingId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Building entity
            modelBuilder.Entity<Building>(entity =>
            {
                entity.ToTable("BUDOVY", schema);

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Acronym)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.NumberOfFloors)
                      .IsRequired()
                      .HasMaxLength(50);

                // Configure relationships
                entity.HasMany(e => e.Rooms)
                      .WithOne(r => r.Building)
                      .HasForeignKey(r => r.BuildingId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Address entity
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("ADRESY", schema);

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Street)
                      .HasMaxLength(255);

                entity.Property(e => e.City)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.HouseNumber)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.OrientationNumber)
                      .HasMaxLength(255);

                entity.Property(e => e.ApartmentNumber)
                      .HasMaxLength(255);

                entity.Property(e => e.PostalCode)
                      .IsRequired()
                      .HasMaxLength(10);

                entity.Property(e => e.Country)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.BuildingId)
                      .IsRequired();

                // Configure relationships
                entity.HasOne(e => e.Building)
                      .WithMany()
                      .HasForeignKey(e => e.BuildingId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("REZERVACE", schema);

                entity.HasKey(e => e.Id);

                entity.Property(e => e.StartDate)
                      .IsRequired();

                entity.Property(e => e.EndDate)
                      .IsRequired();

                entity.Property(e => e.RoomId)
                      .IsRequired();

                entity.Property(e => e.UserId)
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

            // Add additional configurations for new entities if necessary
        }
    }
}
