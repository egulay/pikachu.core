using Helpdesk.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pikachu.Core.Repository.Sql;

namespace Helpdesk.Data
{
    public partial class HelpDeskContext : DbContextBase
    {
        public virtual DbSet<Request> Request { get; set; }
        public virtual DbSet<RequestType> RequestType { get; set; }
        public virtual DbSet<Response> Response { get; set; }
        public virtual DbSet<User> User { get; set; }

        public HelpDeskContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Request>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Body).IsRequired();

                entity.Property(e => e.Header)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.RequestType)
                    .WithMany(p => p.Request)
                    .HasForeignKey(d => d.RequestTypeId)
                    .HasConstraintName("FK_Request_RequestType");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Request)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Request_User");
            });

            modelBuilder.Entity<RequestType>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Response>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Body).IsRequired();

                entity.Property(e => e.Header)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.Response)
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Response_Request");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Response)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Response_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(3000);
            });
        }


    }
}