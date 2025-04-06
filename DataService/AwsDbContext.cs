using FormulaOne.ChatService.Models;
using Microsoft.EntityFrameworkCore;

namespace FormulaOne.ChatService.DataService
{
    public class AwsDbContext : DbContext
    {
        public AwsDbContext(DbContextOptions<AwsDbContext> options) : base(options) { }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserConnection> UsersConnection { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserConnection>()
                .HasKey(uc => uc.Id);

        }
    }
}
