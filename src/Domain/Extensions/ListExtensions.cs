namespace SFA.DAS.Learning.Domain.Extensions;

public static class ListExtensions
{
    public static List<TResult> SelectOrEmptyList<TSource, TResult>(
        this IEnumerable<TSource>? source,
        Func<TSource, TResult> selector)
    {
        return (source ?? Enumerable.Empty<TSource>()).Select(selector).ToList();
    }

    public static void RemoveWhere<TSource>(
        this List<TSource>? source,
        Func<TSource, bool> comparison)
    {
        source.RemoveWhere(comparison, out _);
    }

    public static void RemoveWhere<TSource>(
        this List<TSource>? source,
        Func<TSource, bool> comparison,
        out List<TSource> removedItems)
    {
        removedItems = new List<TSource>();

        if (source == null) return;

        var toRemove = source
               .Where(x => comparison(x))
               .ToList();

        foreach (var item in toRemove)
        {
            source.Remove(item);
            removedItems.Add(item);
        }
    }
}
