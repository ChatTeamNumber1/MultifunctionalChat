using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MultifunctionalChat.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomMember> RoomMembers { get; set; }
        public DbSet<RoomUser> RoomUsers { get; set; }
        public DbSet<User> Users { get; set; }

        public ApplicationContext()
        {
        }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            string connectionString = config.GetConnectionString("RemoteConnection");
            optionsBuilder.UseNpgsql(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasMany(c => c.Rooms)
                .WithMany(s => s.Users)
                .UsingEntity<RoomUser>(
                   j => j
                    .HasOne(pt => pt.Room)
                    .WithMany(t => t.RoomUsers)
                    .HasForeignKey(pt => pt.RoomsId),
                j => j
                    .HasOne(pt => pt.User)
                    .WithMany(p => p.RoomUsers)
                    .HasForeignKey(pt => pt.UsersId),
                j =>
                {
                    j.HasKey(t => new { t.UsersId, t.RoomsId });
                    j.ToTable("RoomUsers");
                    j.HasKey(t => new { t.Id });
                    j.ToTable("RoomUsers");
                }
            );
        }
    }
}