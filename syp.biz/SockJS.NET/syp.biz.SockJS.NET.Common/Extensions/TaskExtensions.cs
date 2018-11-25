using System.Threading.Tasks;

namespace syp.biz.SockJS.NET.Common.Extensions
{
    public static class TaskExtensions
    {
        public static void IgnoreAwait(this Task task) => task.ConfigureAwait(false);
    }
}
