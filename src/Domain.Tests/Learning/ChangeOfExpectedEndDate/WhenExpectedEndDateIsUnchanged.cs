using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Linq;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfExpectedEndDate
{
    [TestFixture]
    public class WhenExpectedEndDateIsUnchanged : ChangeOfExpectedEndDateTestBase
    {
        private LearningDomainModel _learning;
        private LearningUpdateChanges[] _result;

        [SetUp]
        public void SetUp()
        {
            _learning = CreateLearner(1, new DateTime(2025, 07, 31), 15000);

            //Act
            var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());
            updateModel.OnProgrammeDetails.ExpectedEndDate = new DateTime(2025, 07, 31);

            _result = _learning.UpdateLearnerDetails(updateModel);
        }

        [Test]
        public void ThenExpectedEndDateIsNotMarkedAsUpdated()
        {
            _result.Should().NotContain(LearningUpdateChanges.ExpectedEndDate);

            var prices = _learning.LatestEpisode.EpisodePrices
                .OrderBy(x => x.StartDate)
                .ToList();

            prices.Count().Should().Be(1);
            prices.Last().EndDate.Should().Be(new DateTime(2025, 07, 31));
        }

        [Test]
        public void ThenAnEndDateChangedEventIsNotEmitted()
        {
            var events = _learning.FlushEvents();
            events.Should().NotContain(x => x.GetType() == typeof(EndDateChangedEvent));
        }
    }
}
