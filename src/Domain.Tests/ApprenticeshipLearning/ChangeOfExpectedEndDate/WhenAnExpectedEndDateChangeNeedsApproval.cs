using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfExpectedEndDate;

[TestFixture]
public class WhenAnExpectedEndDateChangeNeedsApproval
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;

    private IEnumerable<LearningUpdateChanges> ChangesNeedingApproval =>
        _learning.ChangeDetails.Where(x => x.RequiresApproval).Select(x => x.ChangeType);

    [SetUp]
    public void SetUp()
    {
        (_learning, _learner) = BuildLearning();
    }

    [TestCase("2025-05-31", TestName = "AndTheExpectedEndDateMovesToAnEarlierMonthOnApprovedEpisodeThenItNeedsApproval")]
    [TestCase("2025-09-30", TestName = "AndTheExpectedEndDateMovesToALaterMonthOnApprovedEpisodeThenItNeedsApproval")]
    [TestCase("2025-07-15", TestName = "AndTheExpectedEndDateMovesWithinTheSameMonthOnApprovedEpisodeThenItNeedsApproval")]
    public void AndTheExpectedEndDateChangesOnApprovedEpisodeThenItNeedsApproval(string newExpectedEndDate)
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.ExpectedEndDate = DateTime.Parse(newExpectedEndDate);

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.ExpectedEndDate]);
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeTrue();
    }

    [Test]
    public void AndTheExpectedEndDateChangesOnApprovedEpisodeThenTheChangeDetailCarriesOldNewAndRequiresApproval()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.ExpectedEndDate = new DateTime(2025, 09, 30);

        //Act
        _learning.Update(updateModel);

        //Assert
        var detail = _learning.ChangeDetails.Single(x => x.ChangeType == LearningUpdateChanges.ExpectedEndDate);
        detail.Old!.GetValue<string>().Should().Be("2025-07-31");
        detail.New!.GetValue<string>().Should().Be("2025-09-30");
        detail.RequiresApproval.Should().BeTrue();
    }

    [Test]
    public void AndTheExpectedEndDateAndPriceChangeTogetherOnApprovedEpisodeThenBothNeedApprovalAndTheOldEndDateIsTheOneBeforeTheUpdate()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().TrainingPrice += 1000;
        updateModel.OnProgrammeDetails.ExpectedEndDate = new DateTime(2025, 09, 30);

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.Prices, LearningUpdateChanges.ExpectedEndDate]);

        var detail = _learning.ChangeDetails.Single(x => x.ChangeType == LearningUpdateChanges.ExpectedEndDate);
        detail.Old!.GetValue<string>().Should().Be("2025-07-31");
        detail.New!.GetValue<string>().Should().Be("2025-09-30");
    }

    [Test]
    public void AndTheExpectedEndDateChangesOnUnapprovedEpisodeThenTheChangeIsReportedButDoesNotNeedApproval()
    {
        //Arrange
        (_learning, _learner) = BuildLearning(builder => builder.WithIsApproved(false));
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.ExpectedEndDate = new DateTime(2025, 09, 30);

        //Act
        var result = _learning.Update(updateModel);

        //Assert
        result.Should().Contain(LearningUpdateChanges.ExpectedEndDate);
        ChangesNeedingApproval.Should().BeEmpty();
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeFalse();
    }

    [Test]
    public void AndTheExpectedEndDateChangesOnApprovedEpisodeNotPaidByDasThenTheChangeIsReportedButDoesNotNeedApproval()
    {
        //Arrange
        (_learning, _learner) = BuildLearning(builder => builder.WithFundingPlatform(FundingPlatform.SLD));
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.ExpectedEndDate = new DateTime(2025, 09, 30);

        //Act
        var result = _learning.Update(updateModel);

        //Assert
        result.Should().Contain(LearningUpdateChanges.ExpectedEndDate);
        ChangesNeedingApproval.Should().BeEmpty();
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeFalse();
    }

    [Test]
    public void AndTheExpectedEndDateIsUnchangedThenNothingNeedsApproval()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEmpty();
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeFalse();
    }

    private static (ApprenticeshipLearningDomainModel, LearnerDomainModel) BuildLearning(
        Func<LearningDomainModelBuilder, LearningDomainModelBuilder>? customise = null)
    {
        var builder = new LearningDomainModelBuilder()
            .WithCosts([new Cost { FromDate = new DateTime(2024, 08, 01), TrainingPrice = 10000, EpaoPrice = 1000 }])
            .WithPlannedEndDate(new DateTime(2025, 07, 31));

        return (customise?.Invoke(builder) ?? builder).Build();
    }
}
