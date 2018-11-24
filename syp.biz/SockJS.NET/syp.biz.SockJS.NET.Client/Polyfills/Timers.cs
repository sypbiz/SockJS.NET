using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace syp.biz.SockJS.NET.Client.Polyfills
{
    internal static class Timers
    {
        private static readonly ConcurrentDictionary<int, Action> Timeouts = new ConcurrentDictionary<int, Action>();
        private static readonly ConcurrentDictionary<int, Action> Intervals = new ConcurrentDictionary<int, Action>();

        public static int SetTimeout(Action action, int timeoutMilliseconds)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (timeoutMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));

            var id = action.GetHashCode();
            if (!Timeouts.TryAdd(id, action)) throw new Exception("Action with same hash code already exists");
            Task.Delay(timeoutMilliseconds).ContinueWith(_ =>
            {
                if (!Timeouts.TryRemove(id, out var handler)) return;
                try
                {
                    handler.Invoke();
                }
                catch
                {
                    // ignore
                }
            });
            return id;
        }

        public static void ClearTimeout(int? id)
        {
            if (id is null) return;
            Timeouts.TryRemove(id.Value, out _);
        }

        public static int SetInterval(Action action, int delayMilliseconds)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (delayMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(delayMilliseconds));

            var id = action.GetHashCode();
            if (!Timeouts.TryAdd(id, action)) throw new Exception("Action with same hash code already exists");

            void executor()
            {
                if (!Timeouts.TryRemove(id, out var handler)) return;
                try
                {
                    handler.Invoke();
                }
                catch
                {
                    // ignore
                }
                finally
                {
                    Task.Delay(delayMilliseconds).ContinueWith(_ => executor());
                }
            }

            Task.Delay(delayMilliseconds).ContinueWith(_ => executor());
            return id;
        }

        public static void ClearInterval(int? id)
        {
            if (id is null) return;
            Intervals.TryRemove(id.Value, out _);
        }
    }
}
