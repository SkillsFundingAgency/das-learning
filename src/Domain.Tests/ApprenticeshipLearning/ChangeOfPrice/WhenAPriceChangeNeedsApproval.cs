using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfPrice;

[TestFixture]
public class WhenAPriceChangeNeedsApproval
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;

    private IEnumerable<LearningUpdateChanges> ChangesNeedingApproval =>
        _learning.ChangeDetails.Where(x => x.RequiresApproval).Select(x => x.ChangeType);

    [SetUp]
    public void SetUp()
    {
        var existingCosts = new List<Cost>
        {
            new()
            {
                FromDate = new DateTime(2024, 08, 01),
                TrainingPrice = 10000,
                EpaoPrice = 1000
            }
        };

        (_learning, _learner) = new LearningDomainModelBuilder()
            .WithCosts(existingCosts)
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .Build();
    }

    [Test]
    public void AndTrainingPriceChangesOnApprovedEpisodeThenPricesNeedApproval()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().TrainingPrice += 1000;

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.Prices]);
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeTrue();
    }

    [TestCase(1200)]
    [TestCase(null)]
    public void AndEpaoPriceChangesOnApprovedEpisodeThenPricesNeedApproval(int? newEpaoPrice)
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().EpaoPrice = newEpaoPrice;

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.Prices]);
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeTrue();
    }

    [Test]
    public void AndEpaoPriceChangesFromUnknownToZeroOnApprovedEpisodeThenPricesNeedApproval()
    {
        //Arrange
        (_learning, _learner) = new LearningDomainModelBuilder()
            .WithCosts([new Cost { FromDate = new DateTime(2024, 08, 01), TrainingPrice = 10000, EpaoPrice = null }])
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .Build();

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().EpaoPrice = 0;

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.Prices]);
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeTrue();
    }

    [Test]
    public void AndALaterPriceIsAddedOnApprovedEpisodeThenPricesNeedApproval()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Add(new Cost { FromDate = new DateTime(2025, 01, 01), TrainingPrice = 12000, EpaoPrice = 1000 });

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.Prices]);
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeTrue();
    }

    [Test]
    public void AndALaterPriceIsRemovedOnApprovedEpisodeThenPricesNeedApproval()
    {
        //Arrange
        WithTwoPrices();
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.RemoveAll(x => x.FromDate == new DateTime(2025, 01, 01));

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.Prices]);
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeTrue();
    }

    [Test]
    public void AndALaterPriceDateMovesOnApprovedEpisodeThenPricesNeedApproval()
    {
        //Arrange
        WithTwoPrices();
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single(x => x.FromDate == new DateTime(2025, 01, 01)).FromDate = new DateTime(2025, 02, 01);

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.Prices]);
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeTrue();
    }

    [Test]
    public void AndOnlyTheFirstPriceDateMovesOnApprovedEpisodeWithTwoPricesThenOnlyTheStartDateNeedsApproval()
    {
        //Arrange
        WithTwoPrices();
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single(x => x.FromDate == new DateTime(2024, 08, 01)).FromDate = new DateTime(2024, 08, 15);

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.StartDate]);
    }

    [Test]
    public void AndAnEarlierPriceIsAddedOnApprovedEpisodeThenTheStartDateAndPricesNeedApproval()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Add(new Cost { FromDate = new DateTime(2024, 07, 01), TrainingPrice = 9000, EpaoPrice = 1000 });

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEquivalentTo([LearningUpdateChanges.StartDate, LearningUpdateChanges.Prices]);
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeTrue();
    }

    [Test]
    public void AndPricesChangeOnUnapprovedEpisodeThenNothingNeedsApproval()
    {
        //Arrange
        (_learning, _learner) = new LearningDomainModelBuilder()
            .WithCosts([new Cost { FromDate = new DateTime(2024, 08, 01), TrainingPrice = 10000, EpaoPrice = 1000 }])
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .WithIsApproved(false)
            .Build();

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().TrainingPrice += 1000;

        //Act
        var result = _learning.Update(updateModel);

        //Assert
        result.Should().Contain(LearningUpdateChanges.Prices);
        ChangesNeedingApproval.Should().BeEmpty();
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeFalse();
    }

    [Test]
    public void AndPricesAreUnchangedThenNothingNeedsApproval()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());

        //Act
        _learning.Update(updateModel);

        //Assert
        ChangesNeedingApproval.Should().BeEmpty();
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeFalse();
    }

    private void WithTwoPrices()
    {
        (_learning, _learner) = new LearningDomainModelBuilder()
            .WithCosts(
            [
                new Cost { FromDate = new DateTime(2024, 08, 01), TrainingPrice = 10000, EpaoPrice = 1000 },
                new Cost { FromDate = new DateTime(2025, 01, 01), TrainingPrice = 12000, EpaoPrice = 1000 }
            ])
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .Build();
    }
}
