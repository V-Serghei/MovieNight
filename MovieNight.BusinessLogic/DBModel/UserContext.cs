using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;
using MovieNight.Domain.Entities.Friends;
using MovieNight.Domain.Entities.MailE;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;


namespace MovieNight.BusinessLogic.DBModel
{
    public sealed class UserContext : DbContext
    {
        public UserContext() : base("name=MovieNight")
        {

        }

        public DbSet<UserDbTable> UsersT { get; set; }
        public DbSet<PEdBdTable> PEdBdTables { get; set; }
        
        public DbSet<ViewListDbTable> ViewList { get; set; }
        
        public DbSet<BookmarkDbTable> Bookmark { get; set; }

        public DbSet<FriendsDbTable> Friends { get; set; }
        
        public DbSet<MailDbTable> MailE { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDbTable>()
                .HasMany(u => u.Bookmark)
                .WithRequired(b => b.User)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<MovieDbTable>()
                .HasMany(m => m.BookmarkDbTables)
                .WithRequired(b => b.Movie)
                .HasForeignKey(b => b.MovieId);

            modelBuilder.Entity<ViewListDbTable>()
                .HasKey(w => w.Id);
        
            modelBuilder.Entity<ViewListDbTable>()
                .HasRequired(w => w.User)
                .WithMany(u => u.ViewListEntries)
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<ViewListDbTable>()
                .HasRequired(w => w.Movie)
                .WithMany(m => m.ViewListEntries)
                .HasForeignKey(w => w.MovieId);
            
            modelBuilder.Entity<FriendsDbTable>()
                .HasRequired(f => f.User)
                .WithMany(u => u.FriendsDbTables)
                .HasForeignKey(f => f.IdUser)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<FriendsDbTable>()
                .HasRequired(f => f.Friend)
                .WithMany() // Нет навигационного свойства, так как Friend используется только в контексте Friendship
                .HasForeignKey(f => f.IdFriend)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<MailDbTable>()
                .HasRequired(f => f.Sender)
                .WithMany()
                .HasForeignKey(f => f.SenderId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<MailDbTable>()
                .HasRequired(f => f.Recipient)
                .WithMany()
                .HasForeignKey(f => f.RecipientId)
                .WillCascadeOnDelete(false);

            // modelBuilder.Entity<ViewListDbTable>()
            //     .HasOptional(v => v.User)
            //     .WithMany(u => u.ViewListEntries)
            //     .HasForeignKey(v => v.UserId);
            //
            // modelBuilder.Entity<ViewListDbTable>()
            //     .HasOptional(v => v.Movie)
            //     .WithMany(m => m.ViewListEntries)
            //     .HasForeignKey(v => v.MovieId);


            // modelBuilder.Entity<MovieDbTable>()
            //     .HasMany(m => m.CastMembers)
            //     .WithMany(c => c.Movies)
            //     .Map(mc =>
            //     {
            //         mc.ToTable("MovieCastMembers");
            //         mc.MapLeftKey("MovieId");
            //         mc.MapRightKey("CastMemberId");
            //     });
        }
    }
}
