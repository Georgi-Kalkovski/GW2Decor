using System;
using System.Collections.Generic;

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
            while (true)
            {
                var batch = GetBatch(enumerator, size);
                if (batch is not null)
                    yield return batch;
                else
                    yield break;
            }
        }
    }

    private static IEnumerable<T>? GetBatch<T>(IEnumerator<T> enumerator, int size)
    {
        if (!enumerator.MoveNext())
            return null;

        var batch = new List<T> { enumerator.Current };

        for (int i = 0; i < size && enumerator.MoveNext(); i++)
        {
            batch.Add(enumerator.Current);
        }

        return batch;
    }
}
