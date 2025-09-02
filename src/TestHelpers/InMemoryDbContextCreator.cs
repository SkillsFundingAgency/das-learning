using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;

namespace SFA.DAS.Learning.TestHelpers;

public static class InMemoryDbContextCreator
{
    public static LearningDataContext SetUpInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase("ApprenticeshipsDbContext" + Guid.NewGuid()).Options;

        return new LearningDataContext(options);
    }
}