using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using RaceDay.Api.Data;
using System;


namespace RaceDay.Api.Data;

public class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<EnrolmentStatus> EnrolmentStatuses => Set<EnrolmentStatus>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e => {
            e.ToTable("Users"); e.HasKey(x => x.UserId);
            e.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            e.Property(x => x.ProfilePictureUrl).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            e.ToTable(t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Organiser','Participant')"));
        });
        modelBuilder.Entity<Event>(e => {
            e.ToTable("Events"); e.HasKey(x => x.EventId);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Location).HasMaxLength(200).IsRequired();
            e.Property(x => x.Distance).HasPrecision(6, 2);
            e.Property(x => x.BannerImageUrl).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            e.ToTable(t => t.HasCheckConstraint("CK_Events_Distance", "[Distance] > 0"));
            e.HasOne(x => x.Organiser).WithMany(x => x.OrganisedEvents).HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Category>(e => {
            e.ToTable("Category"); e.HasKey(x => x.CategoryId);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(255);
            e.HasIndex(x => new { x.EventId, x.Name }).IsUnique();
            e.HasOne(x => x.Event).WithMany(x => x.Categories).HasForeignKey(x => x.EventId);
        });
        modelBuilder.Entity<EnrolmentStatus>(e => {
            e.ToTable("EnrolmentStatus"); e.HasKey(x => x.StatusId);
            e.Property(x => x.StatusName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(200);
            e.HasIndex(x => x.StatusName).IsUnique();
        });
        modelBuilder.Entity<Enrolment>(e => {
            e.ToTable("Enrolment"); e.HasKey(x => x.EnrolmentId);
            e.Property(x => x.EnrolmentDate).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => new { x.ParticipantId, x.EventId }).IsUnique();
            e.HasOne(x => x.Participant).WithMany().HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Event).WithMany(x => x.Enrolments).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(x => x.Enrolments).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Status).WithMany(x => x.Enrolments).HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Result>(e => {
            e.ToTable("Results"); e.HasKey(x => x.ResultId);
            e.HasIndex(x => x.EnrolmentId).IsUnique();
            e.Property(x => x.RecordedAt).HasDefaultValueSql("SYSDATETIME()");
            e.ToTable(t => {
                t.HasCheckConstraint("CK_Results_FinishingPosition", "[FinishingPosition] > 0");
                t.HasCheckConstraint("CK_Results_TotalFinishers", "[TotalFinishers] > 0");
                t.HasCheckConstraint("CK_Results_PositionWithinTotal", "[FinishingPosition] <= [TotalFinishers]");
            });
            e.HasOne(x => x.Enrolment).WithOne(x => x.Result).HasForeignKey<Result>(x => x.EnrolmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RecordedBy).WithMany(x => x.RecordedResults).HasForeignKey(x => x.RecordedById).OnDelete(DeleteBehavior.Restrict);
        });
    }
}

