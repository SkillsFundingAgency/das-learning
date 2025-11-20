using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Apprenticeship
{
    public class EpisodePriceDomainModel
    {
        private readonly EpisodePrice _entity;
        public Guid Key => _entity.Key;
        public DateTime StartDate => _entity.StartDate; 
        public DateTime EndDate => _entity.EndDate; 
        public decimal TotalPrice => _entity.TotalPrice;
        public decimal? EndPointAssessmentPrice => _entity.EndPointAssessmentPrice;
        public decimal? TrainingPrice => _entity.TrainingPrice;

        internal static EpisodePriceDomainModel New(
            DateTime startDate,
            DateTime endDate,
            decimal totalPrice,
            decimal? trainingPrice,
            decimal? endpointAssessmentPrice)
        {
            return new EpisodePriceDomainModel(new EpisodePrice
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalPrice = totalPrice,
                TrainingPrice = trainingPrice,
                EndPointAssessmentPrice = endpointAssessmentPrice,
            });
        }

        public EpisodePrice GetEntity()
        {
            return _entity;
        }

        public static EpisodePriceDomainModel Get(EpisodePrice entity)
        {
            return new EpisodePriceDomainModel(entity);
        }

        public void UpdatePrice(decimal trainingPrice, decimal assessmentPrice, decimal totalPrice)
        {
            _entity.TotalPrice = totalPrice;
            _entity.TrainingPrice = trainingPrice;
            _entity.EndPointAssessmentPrice = assessmentPrice;
        }

        private EpisodePriceDomainModel(EpisodePrice entity)
        {
            _entity = entity;
        }
    }
}
