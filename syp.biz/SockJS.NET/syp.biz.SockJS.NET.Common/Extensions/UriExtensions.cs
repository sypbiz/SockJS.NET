using System;

namespace syp.biz.SockJS.NET.Common.Extensions
{
    public static class UriExtensions
    {
        public static Uri AddPath(this Uri uri, string path)
        {
            var builder = new UriBuilder(uri);
            return builder.AddPath(path).Uri;
        }

        public static UriBuilder AddPath(this UriBuilder builder, string path)
        {
            builder.Path += path.StartsWith("/") ? path : $"/{path}";
            return builder;
        }
    }
}
