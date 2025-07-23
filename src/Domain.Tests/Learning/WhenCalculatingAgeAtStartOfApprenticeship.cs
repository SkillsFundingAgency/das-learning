using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning
{
    [TestFixture]
    public class WhenCalculatingAgeAtStartOfApprenticeship
    {
        private Fixture _fixture;

        public WhenCalculatingAgeAtStartOfApprenticeship()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipCustomization());
        }
        
        [TestCase(11, 20)]
        [TestCase(09, 19)]
        public void ThenItIsCalculatedCorrectlyAccordingToTheDateOfBirth(int month, int expectedAge)
        {
            var dateOfBirth = new DateTime(2000, 10, 16);
            var startDate = new DateTime(2020, month, 01);
            var apprenticeship = CreateApprenticeshipDomainModel(dateOfBirth, startDate);

            apprenticeship.AgeAtStartOfApprenticeship.Should().Be(expectedAge);
        }

        private LearningDomainModel CreateApprenticeshipDomainModel(DateTime dateOfBirth, DateTime startDate)
        {
            var apprenticeship = _fixture.Create<LearningDomainModel>();
            apprenticeship.GetEntity().DateOfBirth = dateOfBirth;
            apprenticeship.AddEpisode(
                _fixture.Create<long>(), 
                _fixture.Create<long>(),
                startDate,
                _fixture.Create<DateTime>(), 
                _fixture.Create<decimal>(), 
                _fixture.Create<decimal?>(), 
                _fixture.Create<decimal?>(), 
                _fixture.Create<FundingType>(), 
                _fixture.Create<FundingPlatform>(), 
                _fixture.Create<int>(), 
                _fixture.Create<long?>(), 
                _fixture.Create<string>(), 
                _fixture.Create<long?>(), 
                _fixture.Create<string>(),
                _fixture.Create<string>());

            return apprenticeship;
        }
    }
}
