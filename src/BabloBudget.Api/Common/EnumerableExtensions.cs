namespace BabloBudget.Api.Common;

public static class EnumerableExtensions
{
    public static (IEnumerable<TFirst>, IEnumerable<TSecond>) UnZip<TFirst, TSecond>(
        this IEnumerable<(TFirst First, TSecond Second)> source)
    {
        var buffer = new List<(TFirst, TSecond)>();
        
        var first = GetFirstSequence(source, buffer);
        var second = GetSecondSequence(buffer);

        return (first, second);
    }

    private static IEnumerable<TFirst> GetFirstSequence<TFirst, TSecond>(
        IEnumerable<(TFirst First, TSecond Second)> source,
        List<(TFirst, TSecond)> buffer)
    {
        foreach (var item in source)
        {
            buffer.Add(item);
            yield return item.First;
        }
    }

    private static IEnumerable<TSecond> GetSecondSequence<TFirst, TSecond>(
        List<(TFirst, TSecond)> buffer) =>
        buffer.Select(item => item.Item2);
}