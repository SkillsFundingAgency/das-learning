using AutoFixture;
using SFA.DAS.Learning.Domain.Factories;

namespace SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations
{
    public class ApprenticeshipCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new ApprenticeshipLearningFactory().CreateNew(
                fixture.Create<long>(),
                fixture.Create<string>(),
                fixture.Create<DateTime>(),
                fixture.Create<string>(),
                fixture.Create<string>(),
                fixture.Create<string>()));
        }
    }
}
