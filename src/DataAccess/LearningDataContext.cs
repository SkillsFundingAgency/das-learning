using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.DataAccess;

#pragma warning disable CS8618 // Non-nullable field is uninitialized
[ExcludeFromCodeCoverage]
public class LearningDataContext(DbContextOptions<LearningDataContext> options) : DbContext(options)
{
    public IQueryable<Entities.Learning.Learning> Apprenticeships => ApprenticeshipsDbSet;
    public virtual DbSet<Entities.Learning.Learning> ApprenticeshipsDbSet { get; set; }
    public virtual DbSet<Episode> Episodes { get; set; }
    public virtual DbSet<EpisodePrice> EpisodePrices { get; set; }
    public virtual DbSet<FreezeRequest> FreezeRequests { get; set; }
    public virtual DbSet<MathsAndEnglish> MathsAndEnglish { get; set; }
    public virtual DbSet<LearningSupport> LearningSupport { get; set; }
    public virtual DbSet<EpisodeBreakInLearning> EpisodeBreakInLearnings { get; set; }
    public virtual DbSet<MathsAndEnglishBreakInLearning> MathsAndEnglishBreakInLearnings { get; set; }


    public virtual DbSet<LearningHistory> LearningHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Learning
        modelBuilder.Entity<Entities.Learning.Learning>()
            .HasMany(x => x.Episodes)
            .WithOne()
            .HasForeignKey(fk => fk.LearningKey);
        modelBuilder.Entity<Entities.Learning.Learning>()
            .HasKey(a => new { a.Key });

        modelBuilder.Entity<Entities.Learning.Learning>()
            .HasMany(x => x.MathsAndEnglishCourses)
            .WithOne()
            .HasForeignKey(fk => fk.LearningKey);


        // Episode
        modelBuilder.Entity<Episode>()
            .HasKey(a => new { a.Key });
        modelBuilder.Entity<Episode>()
            .Property(p => p.FundingType)
            .HasConversion(
                v => v.ToString(),
                v => (FundingType)Enum.Parse(typeof(FundingType), v));
        modelBuilder.Entity<Episode>()
            .Property(p => p.FundingPlatform)
            .HasConversion(
                v => (int?)v,
                v => (FundingPlatform?)v);

        // EpisodePrice
        modelBuilder.Entity<EpisodePrice>()
            .HasKey(x => x.Key);

        // FreezeRequest
        modelBuilder.Entity<FreezeRequest>()
            .HasKey(x => x.Key);

        // MathsAndEnglish
        modelBuilder.Entity<MathsAndEnglish>()
            .HasKey(x => x.Key);

        // LearningSupport
        modelBuilder.Entity<LearningSupport>()
            .HasKey(x => x.Key);

        // EpisodeBreakInLearning
        modelBuilder.Entity<EpisodeBreakInLearning>()
            .HasKey(x => x.Key);

        // MathsAndEnglishBreakInLearning
        modelBuilder.Entity<MathsAndEnglishBreakInLearning>()
            .HasKey(x => x.Key);

        modelBuilder.Entity<MathsAndEnglishBreakInLearning>()
            .Property(x => x.MathsAndEnglishKey)
            .IsRequired();

        // LearningHistory
        modelBuilder.Entity<LearningHistory>()
            .ToTable("LearningHistory", "History")
            .HasKey(x => x.Key);

        base.OnModelCreating(modelBuilder);
    }
}

#pragma warning restore CS8618 // Non-nullable field is uninitialized