using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.DataAccess;

#pragma warning disable CS8618 // Non-nullable field is uninitialized
[ExcludeFromCodeCoverage]
public class LearningDataContext(DbContextOptions<LearningDataContext> options) : DbContext(options)
{
    public IQueryable<ApprenticeshipLearning> Apprenticeships => ApprenticeshipLearningDbSet;
    public virtual DbSet<Learner> LearnersDbSet { get; set; }
    public virtual DbSet<Entities.Learning.ApprenticeshipLearning> ApprenticeshipLearningDbSet { get; set; }
    public virtual DbSet<ApprenticeshipEpisode> Episodes { get; set; }
    public virtual DbSet<EpisodePrice> EpisodePrices { get; set; }
    public virtual DbSet<EnglishAndMaths> EnglishAndMaths { get; set; }
    public virtual DbSet<ApprenticeshipLearningSupport> ApprenticeshipLearningSupport { get; set; }
    public virtual DbSet<ShortCourseLearningSupport> ShortCourseLearningSupport { get; set; }
    public virtual DbSet<EpisodeBreakInLearning> EpisodeBreakInLearnings { get; set; }
    public virtual DbSet<EnglishAndMathsBreakInLearning> EnglishAndMathsBreakInLearnings { get; set; }

    public virtual DbSet<LearningHistory> LearningHistories { get; set; }
    public virtual DbSet<ShortCourseLearning> ShortCourseLearnings { get; set; }
    public virtual DbSet<ShortCourseEpisode> ShortCourseEpisodes { get; set; }
    public virtual DbSet<ShortCourseMilestone> ShortCourseMilestones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(Console.WriteLine)
            .EnableSensitiveDataLogging();

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Entities.Learning.Learning>();
        modelBuilder.Ignore<LearningSupport>();

        // Learner
        modelBuilder.Entity<Learner>()
            .HasKey(x => x.Key);

        // ApprenticeshipLearning
        modelBuilder.Entity<Entities.Learning.ApprenticeshipLearning>()
            .HasKey(a => a.Key);

        modelBuilder.Entity<Entities.Learning.ApprenticeshipLearning>()
            .Property(a => a.LearnerKey)
            .IsRequired();

        modelBuilder.Entity<Entities.Learning.ApprenticeshipLearning>()
            .HasMany(x => x.Episodes)
            .WithOne()
            .HasForeignKey(fk => fk.LearningKey);
        modelBuilder.Entity<Entities.Learning.ApprenticeshipLearning>()
             .HasKey(a => new { a.Key });
        modelBuilder.Entity<Entities.Learning.ApprenticeshipLearning>()
            .HasMany(x => x.MathsAndEnglishCourses)
            .WithOne()
            .HasForeignKey(fk => fk.LearningKey);

        // ShortCourseLearning
        modelBuilder.Entity<Entities.Learning.ShortCourseLearning>()
            .HasMany(x => x.Episodes)
            .WithOne()
            .HasForeignKey(fk => fk.LearningKey);
        modelBuilder.Entity<Entities.Learning.ShortCourseLearning>()
            .HasKey(a => new { a.Key });

        modelBuilder.Entity<Entities.Learning.ShortCourseLearning>()
            .Property(a => a.LearnerKey)
            .IsRequired();

        // Episode
        modelBuilder.Entity<ApprenticeshipEpisode>()
            .HasKey(a => new { a.Key });
        modelBuilder.Entity<ApprenticeshipEpisode>()
            .Property(p => p.FundingType)
            .HasConversion(
                v => v.ToString(),
                v => (FundingType)Enum.Parse(typeof(FundingType), v));
        modelBuilder.Entity<ApprenticeshipEpisode>()
            .Property(p => p.FundingPlatform)
            .HasConversion(
                v => (int?)v,
                v => (FundingPlatform?)v);

        modelBuilder.Entity<ApprenticeshipEpisode>()
            .HasOne<ApprenticeshipLearning>()
            .WithMany(a => a.Episodes)
            .HasForeignKey(e => e.LearningKey)
            .HasPrincipalKey(a => a.Key);

        modelBuilder.Entity<ShortCourseEpisode>()
            .HasKey(a => new { a.Key });

        modelBuilder.Entity<ShortCourseEpisode>()
            .HasOne<ShortCourseLearning>()
            .WithMany(l => l.Episodes)
            .HasForeignKey(e => e.LearningKey);

        // EpisodePrice
        modelBuilder.Entity<EpisodePrice>()
            .HasKey(x => x.Key);

            modelBuilder.Entity<EpisodePrice>()
                .HasOne<ApprenticeshipEpisode>()
                .WithMany(e => e.Prices)
                .HasForeignKey(e => e.EpisodeKey)
                .HasPrincipalKey(ae => ae.Key);

        // EnglishAndMaths
        modelBuilder.Entity<EnglishAndMaths>()
            .HasKey(x => x.Key);

        modelBuilder.Entity<EnglishAndMaths>()
            .HasOne<ApprenticeshipLearning>()
            .WithMany(al => al.MathsAndEnglishCourses)
            .HasForeignKey(e => e.LearningKey)
            .HasPrincipalKey(al => al.Key);

        // ApprenticeshipLearningSupport
        modelBuilder.Entity<ApprenticeshipLearningSupport>()
            .HasKey(x => x.Key);

        modelBuilder.Entity<ApprenticeshipLearningSupport>()
            .HasOne<ApprenticeshipEpisode>()
            .WithMany(e => e.LearningSupport)
            .HasForeignKey(e => e.EpisodeKey)
            .HasPrincipalKey(ae => ae.Key);

        // ShortCourseLearningSupport
        modelBuilder.Entity<ShortCourseLearningSupport>()
            .HasKey(x => x.Key);

        modelBuilder.Entity<ShortCourseLearningSupport>()
            .HasOne<ShortCourseEpisode>()
            .WithMany(e => e.LearningSupport)
            .HasForeignKey(e => e.EpisodeKey)
            .HasPrincipalKey(se => se.Key);

        // EpisodeBreakInLearning
        modelBuilder.Entity<EpisodeBreakInLearning>()
            .HasKey(x => x.Key);

        modelBuilder.Entity<EpisodeBreakInLearning>()
            .HasOne<ApprenticeshipEpisode>()
            .WithMany(e => e.BreaksInLearning)
            .HasForeignKey(e => e.EpisodeKey)
            .HasPrincipalKey(ae => ae.Key)
            .IsRequired();

        // EnglishAndMathsBreakInLearning
        modelBuilder.Entity<EnglishAndMathsBreakInLearning>()
            .HasKey(x => x.Key);

        modelBuilder.Entity<EnglishAndMathsBreakInLearning>()
            .Property(x => x.EnglishAndMathsKey)
            .IsRequired();

        // LearningHistory
        modelBuilder.Entity<LearningHistory>()
            .ToTable("LearningHistory", "History")
            .HasKey(x => x.Key);

        // ShortCourseMilestone
        modelBuilder.Entity<ShortCourseMilestone>()
            .HasKey(e => e.Key);

        modelBuilder.Entity<ShortCourseMilestone>()
            .Property(e => e.Milestone)
            .HasConversion(
                v => v.ToString(),
                v => (Milestone)Enum.Parse(typeof(Milestone), v));

        modelBuilder.Entity<ShortCourseMilestone>()
            .HasOne<ShortCourseEpisode>()
            .WithMany(e => e.Milestones)
            .HasForeignKey(m => m.EpisodeKey);

        base.OnModelCreating(modelBuilder);
    }
}

#pragma warning restore CS8618 // Non-nullable field is uninitialized