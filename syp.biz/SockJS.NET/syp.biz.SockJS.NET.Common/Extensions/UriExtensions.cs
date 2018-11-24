using System;

namespace syp.biz.SockJS.NET.Common.Extensions
{
    public static class UriExtensions
    {
        public static Uri AddPath(this Uri uri, string path)
        {
            var builder = new UriBuilder(uri);
            builder.Path += path.StartsWith("/") ? path : $"/{path}";
            return builder.Uri;
        }
    }
}
