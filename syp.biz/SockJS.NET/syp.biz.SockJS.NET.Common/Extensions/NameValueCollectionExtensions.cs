using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace syp.biz.SockJS.NET.Common.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static IEnumerable<(string name, string value)> AsEnumerable(this NameValueCollection collection) => collection
                .Cast<string>()
                .Select(name => (name, collection[name]));
    }
}
