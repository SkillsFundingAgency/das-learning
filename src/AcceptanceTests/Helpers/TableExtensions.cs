namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class TableExtensions
{
    internal static bool GetBoolean(this Table table, string columnName, int rowIndex = 0)
    {
        var cellValue = table.Rows[rowIndex][columnName];
        return bool.Parse(cellValue);
    }

    internal static List<string> GetIndexedListValues(this Table table, string columnName, int rowIndex = 0)
    {
        var values = new List<string>();
        var noMoreIndexedValues = false;
        var index = 0;

        while (!noMoreIndexedValues)
        {
            var indexedColumnName = $"{columnName}[{index}]";

            if(table.Header.Contains(indexedColumnName))
            {
                var cellValue = table.Rows[rowIndex][indexedColumnName];
                values.Add(cellValue);
            }
            else
            {
                noMoreIndexedValues = true;
            }

            index++;
        }

        return values;
    }
}
