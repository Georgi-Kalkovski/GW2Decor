using System.Collections.Generic;
using System;

public static class LinqExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than 0.");

        using (var enumerator = source.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                yield return GetBatch(enumerator, size - 1);
            }
        }
    }

    private static IEnumerable<T> GetBatch<T>(IEnumerator<T> enumerator, int size)
    {
        yield return enumerator.Current;

        for (int i = 0; i < size && enumerator.MoveNext(); i++)
        {
            yield return enumerator.Current;
        }
    }
}
