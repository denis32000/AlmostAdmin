using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        //public DbSet<QuestionTag> QuestionTags { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // использование Fluent API
            //modelBuilder.Entity<Question>()
            //    .HasOne(q => q.Answer)
            //    .WithMany(a => a.Questions)
            //    .IsRequired(false)
            //    .OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<Answer>()
            //    .HasMany(a => a.Questions)
            //    .WithOne(q => q.Answer)
            //    .OnDelete(DeleteBehavior.SetNull);

            // Many Users to Many Projects
            modelBuilder.Entity<UserProject>()
                .HasKey(t => new { t.UserId, t.ProjectId });

            modelBuilder.Entity<UserProject>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProjects)
                .HasForeignKey(up => up.UserId)
                .HasPrincipalKey(u => u.Id);

            modelBuilder.Entity<UserProject>()
                .HasOne(up => up.Project)
                .WithMany(c => c.UserProjects)
                .HasForeignKey(up => up.ProjectId);

            // Many Questions to Many Tags
            modelBuilder.Entity<QuestionTag>()
                .HasKey(t => new { t.QuestionId, t.TagId });

            modelBuilder.Entity<QuestionTag>()
                .HasOne(qt => qt.Question)
                .WithMany(q => q.QuestionTags)
                .HasForeignKey(sc => sc.QuestionId);

            modelBuilder.Entity<QuestionTag>()
                .HasOne(qt => qt.Tag)
                .WithMany(c => c.QuestionTags)
                .HasForeignKey(sc => sc.TagId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
