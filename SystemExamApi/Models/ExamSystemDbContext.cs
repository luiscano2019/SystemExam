using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SystemExamApi.Models;

public partial class ExamSystemDbContext : DbContext
{
    public ExamSystemDbContext()
    {
    }

    public ExamSystemDbContext(DbContextOptions<ExamSystemDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamAnswer> ExamAnswers { get; set; }

    public virtual DbSet<ExamAttempt> ExamAttempts { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ExamSystemDB;integrated security=yes");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name, "IX_Categories_Name").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#3b82f6");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Exams_CategoryId");

            entity.HasIndex(e => e.CreatedBy, "IX_Exams_CreatedBy");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PassingScore).HasDefaultValue(60);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Category).WithMany(p => p.Exams).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ExamAnswer>(entity =>
        {
            entity.HasIndex(e => new { e.AttemptId, e.QuestionId }, "IX_ExamAnswers_AttemptId_QuestionId").IsUnique();

            entity.HasIndex(e => e.QuestionId, "IX_ExamAnswers_QuestionId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.SelectedOptions).HasDefaultValue("[]");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Attempt).WithMany(p => p.ExamAnswers).HasForeignKey(d => d.AttemptId);

            entity.HasOne(d => d.Question).WithMany(p => p.ExamAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ExamAttempt>(entity =>
        {
            entity.HasIndex(e => e.ExamId, "IX_ExamAttempts_ExamId");

            entity.HasIndex(e => e.StudentId, "IX_ExamAttempts_StudentId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("in-progress");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamAttempts).HasForeignKey(d => d.ExamId);

            entity.HasOne(d => d.Student).WithMany(p => p.ExamAttempts).HasForeignKey(d => d.StudentId);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasIndex(e => new { e.ExamId, e.OrderNumber }, "IX_Questions_ExamId_OrderNumber").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Points).HasDefaultValue(1);
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasDefaultValue("multiple-choice");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Exam).WithMany(p => p.Questions).HasForeignKey(d => d.ExamId);
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasIndex(e => new { e.QuestionId, e.OrderNumber }, "IX_QuestionOptions_QuestionId_OrderNumber").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions).HasForeignKey(d => d.QuestionId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Dni, "IX_Users_DNI")
                .IsUnique()
                .HasFilter("([DNI] IS NOT NULL)");

            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .HasColumnName("DNI");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.UserRole)
                .HasMaxLength(20)
                .HasDefaultValue(UserRole.Student)
                .HasConversion(
                    v => v.GetDescription(),
                    v => UserRoleExtensions.FromDescription(v));
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
