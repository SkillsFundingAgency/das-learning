namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class TableRowExtensions
{
    internal static List<string> GetIndexedListValues(this TableRow row, string columnName)
    {
        var values = new List<string>();
        var noMoreIndexedValues = false;
        var index = 0;

        while (!noMoreIndexedValues)
        {
            var indexedColumnName = $"{columnName}[{index}]";

            if (row.Keys.Contains(indexedColumnName))
            {
                var cellValue = row[indexedColumnName];
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