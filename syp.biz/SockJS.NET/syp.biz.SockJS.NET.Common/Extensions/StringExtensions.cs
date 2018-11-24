namespace syp.biz.SockJS.NET.Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is <c>null</c> or <see cref="string.Empty"/>,
        /// or if <paramref name="value"/> consists exclusively of white-space characters.
        /// </returns>
        /// <seealso cref="string.IsNullOrWhiteSpace"/>
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Indicates whether the specified string is null or an <see cref="F:System.String.Empty"></see> string.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="value">value</paramref> parameter is null or an empty string (""); otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="string.IsNullOrEmpty"/>
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
    }
}
