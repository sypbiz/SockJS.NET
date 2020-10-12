using System.Collections.Generic;

namespace syp.biz.SockJS.NET.Common.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value associated with the specified key, or <c>default</c> of <typeparamref name="TValue"/> if not present in the <paramref name="dictionary"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> to search.</param>
        /// <param name="key">The key to search for.</param>
        /// <returns>
        /// The value of <paramref name="key"/> in the <paramref name="dictionary"/>, or <c>default</c> of <typeparamref name="TValue"/> if not found or if <paramref name="dictionary"/> is <c>null</c>.
        /// </returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) => dictionary != null && dictionary.TryGetValue(key, out var value) ? value : default;
    }
}
