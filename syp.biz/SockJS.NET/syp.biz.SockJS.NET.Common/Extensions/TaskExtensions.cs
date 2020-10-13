using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace syp.biz.SockJS.NET.Common.Extensions
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class TaskExtensions
    {
        public static void IgnoreAwait(this Task task) => task.ConfigureAwait(false);
    }
}
