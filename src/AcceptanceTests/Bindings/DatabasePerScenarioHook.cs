using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.AcceptanceTests.Bindings;

[Binding]
public class DatabasePerScenarioHook
{
    private readonly ScenarioContext _scenarioContext;

    public DatabasePerScenarioHook(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario(Order = 2)]
    public void CreateDatabase(TestContext context)
    {
        context.SqlDatabase = new SqlDatabase();
        _scenarioContext.SetDbConnectionString(context.SqlDatabase.DatabaseInfo.ConnectionString);
    }

    [AfterScenario(Order = 100)]
    public static void TearDownDatabase(TestContext context)
    {
        context.SqlDatabase?.Dispose();
    }
}
