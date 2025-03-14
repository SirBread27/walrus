using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WalrusMessenger
{
    public class AppContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<PrivateChat> PrivateChats { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        
        public AppContext(DbContextOptions<AppContext> options) : base(options) 
        {
            Database.EnsureCreated();
            ChangeTracker.AutoDetectChangesEnabled = true;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Ignore<Message>();
            #region Users
            builder.Entity<User>()
                    .HasKey(p => p.Id);
            builder.Entity<User>()
                        .Property(p => p.Id)
                        .HasColumnType("bigint")
                        .UseIdentityColumn()
                        .ValueGeneratedOnAdd()
                        .IsRequired()
                        .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
            builder.Entity<User>()
                        .Property(p => p.Name)
                        .HasColumnType("nvarchar")
                        .HasMaxLength(25)
                        .IsRequired();
            builder.Entity<User>()
                        .Property(p => p.Login)
                        .HasColumnType("nvarchar")
                        .HasMaxLength(25)
                        .IsRequired();
            builder.Entity<User>()
                        .Property(p => p.Email)
                        .HasColumnType("nvarchar")
                        .HasMaxLength(40)
                        .IsRequired();
            builder.Entity<User>()
                        .Property(p => p.Chats)
                        .HasConversion(
                            e => string.Join(';', e), 
                            e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList())
                        .HasColumnType("nvarchar")
                        .HasMaxLength(1000);
            builder.Entity<User>()
                        .Property(p => p.BanList)
                        .HasConversion(
                            e => string.Join(';', e), 
                            e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList())
                        .HasColumnType("nvarchar")
                        .HasMaxLength(1000);
            builder.Entity<User>()
                        .Property(p => p.ProfilePicture)
                        .HasColumnType("binary");
            builder.Entity<User>()
                        .Property(p => p.Description)
                        .HasColumnType("nvarchar")
                        .HasMaxLength(400)
                        .HasDefaultValue("");
            builder.Entity<User>()
                        .Property(p => p.Password)
                        .HasColumnType("nvarchar")
                        .HasMaxLength(25)
                        .IsRequired();
            builder.Entity<User>()
                        .Property(p => p.IsOnline)
                        .HasColumnType("bit")
                        .IsRequired();
            builder.Entity<User>()
                        .Property(p => p.LastLogin)
                        .HasColumnType("datetime2")
                        .IsRequired();
            #endregion Users

            #region Chats
            builder.Entity<Chat>()
                    .HasKey(p => p.Id);
            builder.Entity<Chat>()
                        .Property(p => p.Id)
                        .HasColumnType("bigint")
                        .UseIdentityColumn()
                        .ValueGeneratedOnAdd()
                        .IsRequired()
                        .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
            builder.Entity<Chat>()
                        .Property(p => p.Name)
                        .HasColumnType("nvarchar")
                        .HasMaxLength(25)
                        .IsRequired();
            builder.Entity<Chat>()
                        .Property(p => p.Description)
                        .HasColumnType("nvarchar")
                        .HasMaxLength(400);
            builder.Entity<Chat>()
                        .Property(p => p.ProfilePicture)
                        .HasColumnType("binary");
            builder.Entity<Chat>()
                        .Property(p => p.LastMessage)
                        .HasColumnType("nvarchar")
                        .HasMaxLength(400);
            builder.Entity<Chat>()
                        .Property(p => p.Users)
                        .HasConversion(
                             e => string.Join(';', e), 
                             e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList())
                        .HasColumnType("nvarchar")
                        .HasMaxLength(1000);
            builder.Entity<Chat>()
                        .Property(p => p.Admins)
                        .HasConversion(
                             e => string.Join(';', e),
                             e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList())
                        .HasColumnType("nvarchar")
                        .HasMaxLength(1000);
            builder.Entity<Chat>()
                        .Property(p => p.Files)
                        .HasConversion(
                             e => string.Join(';', e.Select(q => q.Path)),
                             e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(IncludedFile.FromPath).ToList())
                        .HasColumnType("nvarchar")
                        .HasMaxLength(1000);
            builder.Entity<Chat>()
                   .HasMany(p => p.Messages)
                   .WithOne(p => p.Chat)
                   .HasForeignKey(p => p.ChatId);
            #endregion Chats
            
            #region PrivateChats
            builder.Entity<PrivateChat>()
                        .Property(p => p.LastMessage)
                        .HasColumnType("nvarchar")
                        .HasMaxLength(400);
            builder.Entity<PrivateChat>()
                        .Property(p => p.Users)
                        .HasConversion(
                             e => string.Join(';', e),
                             e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList())
                        .HasColumnType("nvarchar")
                        .HasMaxLength(1000);
            builder.Entity<PrivateChat>()
                        .Property(p => p.Files)
                        .HasConversion(
                             e => string.Join(';', e.Select(q => q.Path)),
                             e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(IncludedFile.FromPath).ToList())
                        .HasColumnType("nvarchar")
                        .HasMaxLength(1000);
            builder.Entity<PrivateChat>()
                   .HasMany(p => p.Messages)
                   .WithOne(p => (PrivateChat)p.Chat)
                   .HasForeignKey(p => p.ChatId); 
            #endregion PrivateChats
            #region Messages
              builder.Entity<Message>()
                     .HasKey(p => p.Id);
              builder.Entity<Message>()
                     .Property(p => p.Id)
                     .HasColumnType("bigint")
                     .UseIdentityColumn()
                     .ValueGeneratedOnAdd()
                     .IsRequired()
                     .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
              builder.Entity<Message>()
                     .Property(c => c.Text)
                     .HasColumnType("nvarchar")
                     .HasMaxLength(500)
                     .IsRequired();
              builder.Entity<Message>()
                     .Property(c => c.DateTime)
                     .HasColumnType("datetime2")
                     .IsRequired();
              builder.Entity<Message>()
                     .Property(c => c.SenderId)
                     .HasColumnType("bigint")
                     .IsRequired();
              builder.Entity<Message>()
                     .Property(c => c.ResentFrom)
                     .HasColumnType("bigint");
              builder.Entity<Message>()
                     .Property(c => c.AnswerTo)
                     .HasColumnType("bigint"); 
            builder.Entity<Message>()
                   .Property(p => p.DeletedFor)
                   .HasConversion(
                       e => string.Join(';', e),
                       e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList()); ;
            builder.Entity<Message>()
                   .Property(p => p.ReadBy)
                   .HasConversion(
                       e => string.Join(';', e),
                       e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList()); 
            builder.Entity<Message>()
                   .Property(p => p.IncludedFiles)
                   .HasConversion(
                             e => string.Join(';', e.Select(q => q.Path)),
                             e => e.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(IncludedFile.FromPath).ToArray())
                        .HasColumnType("nvarchar")
                        .HasMaxLength(1000);
            #endregion Messages
        }
    }
}
