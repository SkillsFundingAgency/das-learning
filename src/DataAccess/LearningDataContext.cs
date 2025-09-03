using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.DataAccess
{
    [ExcludeFromCodeCoverage]
    public class LearningDataContext(DbContextOptions<LearningDataContext> options) : DbContext(options)
    {
        public IQueryable<Entities.Learning.Learning> Apprenticeships => ApprenticeshipsDbSet;
        public virtual DbSet<Entities.Learning.Learning> ApprenticeshipsDbSet { get; set; }
        public virtual DbSet<Episode> Episodes { get; set; }
        public virtual DbSet<EpisodePrice> EpisodePrices { get; set; }
        public virtual DbSet<PriceHistory> PriceHistories { get; set; }
        public virtual DbSet<StartDateChange> StartDateChanges { get; set; }
        public virtual DbSet<FreezeRequest> FreezeRequests { get; set; }
        public virtual DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
        public virtual DbSet<MathsAndEnglish> MathsAndEnglish { get; set; }
        public virtual DbSet<LearningSupport> LearningSupport { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Learning
            modelBuilder.Entity<Entities.Learning.Learning>()
                .HasMany(x => x.Episodes)
                .WithOne()
                .HasForeignKey(fk => fk.LearningKey);
            modelBuilder.Entity<Entities.Learning.Learning>()
                .HasKey(a => new { a.Key });

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

            // PriceHistory
            modelBuilder.Entity<PriceHistory>()
                .HasKey(x => x.Key);
            modelBuilder.Entity<PriceHistory>()
                .Property(x => x.PriceChangeRequestStatus)
                .HasConversion(
                    v => v.ToString(),
                    v => (ChangeRequestStatus)Enum.Parse(typeof(ChangeRequestStatus), v));
            modelBuilder.Entity<PriceHistory>()
                .Property(x => x.Initiator)
                .HasConversion(
                    v => v.ToString(),
                    v => (ChangeInitiator)Enum.Parse(typeof(ChangeInitiator), v));
            
            // StartDateChange
            modelBuilder.Entity<StartDateChange>()
                .HasKey(x => x.Key);
            modelBuilder.Entity<StartDateChange>()
                .Property(x => x.RequestStatus)
                .HasConversion(
                    v => v.ToString(),
                    v => (ChangeRequestStatus)Enum.Parse(typeof(ChangeRequestStatus), v));
            modelBuilder.Entity<StartDateChange>()
                .Property(x => x.Initiator)
                .HasConversion(
                    v => v.ToString(),
                    v => (ChangeInitiator)Enum.Parse(typeof(ChangeInitiator), v));

            // FreezeRequest
            modelBuilder.Entity<FreezeRequest>()
                .HasKey(x => x.Key);

            // WithdrawalRequest
            modelBuilder.Entity<WithdrawalRequest>()
                .HasKey(x => x.Key);

            // MathsAndEnglish
            modelBuilder.Entity<MathsAndEnglish>()
                .HasKey(x => x.Key);

            // LearningSupport
            modelBuilder.Entity<LearningSupport>()
                .HasKey(x => x.Key);

            base.OnModelCreating(modelBuilder);
        }
    }
}