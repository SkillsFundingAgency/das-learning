namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class TableExtensions
{
    internal static bool GetBoolean(this Table table, string columnName, int rowIndex = 0)
    {
        var cellValue = table.Rows[rowIndex][columnName];
        return bool.Parse(cellValue);
    }
}
