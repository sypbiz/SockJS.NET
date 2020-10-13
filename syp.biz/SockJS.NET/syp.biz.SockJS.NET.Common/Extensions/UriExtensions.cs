using System;
using System.Diagnostics.CodeAnalysis;

namespace syp.biz.SockJS.NET.Common.Extensions
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class UriExtensions
    {
        public static Uri AddPath(this Uri uri, string path) => new UriBuilder(uri).AddPath(path).Uri;

        public static UriBuilder AddPath(this UriBuilder builder, string path)
        {
            builder.Path += path.StartsWith("/") ? path : $"/{path}";
            return builder;
        }
    }
}
