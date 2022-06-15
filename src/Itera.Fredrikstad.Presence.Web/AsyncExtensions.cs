namespace Itera.Fredrikstad.Presence.Web;

public static class Extensions
{
    public static async IAsyncEnumerable<TOut> SelectAsync<TIn, TOut>(
        this IEnumerable<TIn> enumerable,
        Func<TIn, Task<TOut>> func)
    {
        foreach (var item in enumerable)
        {
            yield return await func(item);
        }
    }

    public static async Task<List<TIn>> ToListAsync<TIn>(this IAsyncEnumerable<TIn> enumerable)
    {
        var list = new List<TIn>();
        await foreach (var item in enumerable)
        {
            list.Add(item);
        }

        return list;
    }
}