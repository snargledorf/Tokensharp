using System.Runtime.InteropServices;

namespace Tokensharp;

public static class CollectionExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        where TKey : notnull
    {
        if (dictionary is null)
            throw new ArgumentNullException(nameof(dictionary));

        if (valueFactory is null)
            throw new ArgumentNullException(nameof(valueFactory));

        ref TValue? value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out bool exists);
        if (!exists)
            value = valueFactory(key);

        return value!;
    }
}