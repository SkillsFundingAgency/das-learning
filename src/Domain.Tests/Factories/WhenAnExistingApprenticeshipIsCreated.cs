using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Factories;

namespace SFA.DAS.Learning.Domain.UnitTests.Factories
{
    [TestFixture]
    public class WhenAnExistingApprenticeshipIsCreated
    {
        private LearningFactory _learningFactory;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _learningFactory = new LearningFactory();
            _fixture = new Fixture();
        }

        [Test]
        public void ThenAnApprenticeshipCreatedEventIsAdded()
        {
            var expectedModel = _fixture.Create<DataAccess.Entities.Learning.Learning>();
            var apprenticeship = _learningFactory.GetExisting(expectedModel);
            apprenticeship.GetEntity().Should().BeSameAs(expectedModel);
        }
    }
}
