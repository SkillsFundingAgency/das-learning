using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.AcceptanceTests.Bindings;

[Binding]
public static class DatabasePerTestRunHook
{
    [BeforeTestRun(Order = 1)]
    public static void RefreshDatabaseModel()
    {
        SqlDatabaseModel.Update("SFA.DAS.Learning.Database");
    }
}
