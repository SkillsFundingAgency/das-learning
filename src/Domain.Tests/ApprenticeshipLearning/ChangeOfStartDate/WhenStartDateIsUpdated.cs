using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfStartDate;

[TestFixture]
public class WhenStartDateIsUpdated
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

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
    public void AndEarliestCostFromDateIsChangedThenStartDateChangeIsReported()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().FromDate = new DateTime(2024, 08, 15);

        //Act
        _result = _learning.Update(updateModel);

        //Assert
        _result.Should().Contain(LearningUpdateChanges.StartDate);
        _result.Should().Contain(LearningUpdateChanges.Prices);
    }

    [Test]
    public void AndEarlierCostIsAddedThenStartDateChangeIsReported()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Add(new Cost
        {
            FromDate = new DateTime(2024, 07, 01),
            TrainingPrice = 9000,
            EpaoPrice = 1000
        });

        //Act
        _result = _learning.Update(updateModel);

        //Assert
        _result.Should().Contain(LearningUpdateChanges.StartDate);
    }

    [Test]
    public void AndStartDateChangesOnApprovedEpisodeThenFurtherApprovalIsNeeded()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().FromDate = new DateTime(2024, 08, 15);

        //Act
        _learning.Update(updateModel);

        //Assert
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeTrue();
    }

    [Test]
    public void AndStartDateChangesOnUnapprovedEpisodeThenFurtherApprovalIsNotNeeded()
    {
        //Arrange
        (_learning, _learner) = new LearningDomainModelBuilder()
            .WithCosts([new Cost { FromDate = new DateTime(2024, 08, 01), TrainingPrice = 10000, EpaoPrice = 1000 }])
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .WithIsApproved(false)
            .Build();

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().FromDate = new DateTime(2024, 08, 15);

        //Act
        var result = _learning.Update(updateModel);

        //Assert
        result.Should().Contain(LearningUpdateChanges.StartDate);
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeFalse();
    }

    [Test]
    public void AndStartDateIsNotChangedThenFurtherApprovalIsNotNeeded()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().TrainingPrice += 1000;

        //Act
        _learning.Update(updateModel);

        //Assert
        _learning.GetEntity().Episodes.Single().FurtherApprovalNeeded.Should().BeFalse();
    }

    [Test]
    public void AndCostsAreUnchangedThenStartDateChangeIsNotReported()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());

        //Act
        _result = _learning.Update(updateModel);

        //Assert
        _result.Should().NotContain(LearningUpdateChanges.StartDate);
    }

    [Test]
    public void AndOnlyPriceAmountIsChangedThenStartDateChangeIsNotReported()
    {
        //Arrange
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Single().TrainingPrice += 1000;

        //Act
        _result = _learning.Update(updateModel);

        //Assert
        _result.Should().Contain(LearningUpdateChanges.Prices);
        _result.Should().NotContain(LearningUpdateChanges.StartDate);
    }

    [Test]
    public void AndOnlyALaterCostFromDateIsChangedThenStartDateChangeIsNotReported()
    {
        //Arrange
        var costs = new List<Cost>
        {
            new() { FromDate = new DateTime(2024, 08, 01), TrainingPrice = 10000, EpaoPrice = 1000 },
            new() { FromDate = new DateTime(2025, 01, 01), TrainingPrice = 11000, EpaoPrice = 1000 }
        };

        (_learning, _learner) = new LearningDomainModelBuilder()
            .WithCosts(costs)
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .Build();

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.Costs.Last().FromDate = new DateTime(2025, 02, 01);

        //Act
        _result = _learning.Update(updateModel);

        //Assert
        _result.Should().Contain(LearningUpdateChanges.Prices);
        _result.Should().NotContain(LearningUpdateChanges.StartDate);
    }
}
