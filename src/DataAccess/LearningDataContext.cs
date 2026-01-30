using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.DataAccess
{
    [ExcludeFromCodeCoverage]
    public class LearningDataContext(DbContextOptions<LearningDataContext> options) : DbContext(options)
    {
        public IQueryable<Entities.Learning.ApprenticeshipLearning> Apprenticeships => ApprenticeshipsDbSet;
        public virtual DbSet<Entities.Learning.ApprenticeshipLearning> ApprenticeshipsDbSet { get; set; }
        public virtual DbSet<ApprenticeshipEpisode> Episodes { get; set; }
        public virtual DbSet<EpisodePrice> EpisodePrices { get; set; }
        public virtual DbSet<FreezeRequest> FreezeRequests { get; set; }
        public virtual DbSet<MathsAndEnglish> MathsAndEnglish { get; set; }
        public virtual DbSet<LearningSupport> LearningSupport { get; set; }
        public virtual DbSet<EpisodeBreakInLearning> EpisodeBreakInLearnings { get; set; }
        public virtual DbSet<LearningHistory> LearningHistories { get; set; }
        public virtual DbSet<Entities.Learning.ShortCourseLearning> ShortCourseLearnings { get; set; }
        public virtual DbSet<Entities.Learning.ShortCourseEpisode> ShortCourseEpisodes { get; set; }
        public virtual DbSet<Entities.Learning.ShortCourseMilestone> ShortCourseMilestones { get; set; }

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

            // Learning
            modelBuilder.Entity<Entities.Learning.ApprenticeshipLearning>()
                .HasMany(x => x.Episodes)
                .WithOne()
                .HasForeignKey(fk => fk.LearningKey);
            modelBuilder.Entity<Entities.Learning.ApprenticeshipLearning>()
                .HasKey(a => new { a.Key });
            ;
            modelBuilder.Entity<Entities.Learning.ShortCourseLearning>()
                .HasKey(a => new { a.Key });

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

            // EpisodePrice
            modelBuilder.Entity<EpisodePrice>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<EpisodePrice>()
                .HasOne<ApprenticeshipEpisode>()
                .WithMany(e => e.Prices)
                .HasForeignKey(e => e.EpisodeKey)
                .HasPrincipalKey(ae => ae.Key);

            // FreezeRequest
            modelBuilder.Entity<FreezeRequest>()
                .HasKey(x => x.Key);
            modelBuilder.Entity<FreezeRequest>()
                .HasOne<ApprenticeshipLearning>() 
                .WithMany(al => al.FreezeRequests)
                .HasForeignKey(e => e.LearningKey)
                .HasPrincipalKey(al => al.Key);

            // MathsAndEnglish
            modelBuilder.Entity<MathsAndEnglish>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<MathsAndEnglish>()
                .HasOne<ApprenticeshipLearning>()
                .WithMany(al => al.MathsAndEnglishCourses)
                .HasForeignKey(e => e.LearningKey)
                .HasPrincipalKey(al => al.Key);

            // LearningSupport
            modelBuilder.Entity<LearningSupport>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<LearningSupport>()
                .HasOne<ApprenticeshipEpisode>()
                .WithMany(e => e.LearningSupport)
                .HasForeignKey(e => e.EpisodeKey)
                .HasPrincipalKey(ae => ae.Key);

            // EpisodeBreakInLearning
            modelBuilder.Entity<EpisodeBreakInLearning>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<EpisodeBreakInLearning>()
                .HasOne<ApprenticeshipEpisode>()
                .WithMany(e => e.BreaksInLearning)
                .HasForeignKey(e => e.EpisodeKey)
                .HasPrincipalKey(ae => ae.Key);

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
                .WithMany()
                .HasForeignKey(e => e.EpisodeKey)
                .HasPrincipalKey(e => e.Key);

            base.OnModelCreating(modelBuilder);
        }
    }
}