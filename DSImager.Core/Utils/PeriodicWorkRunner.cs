using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSImager.Core.Utils
{
    public static class PeriodicWorkRunner
    {
        public static async Task DoWorkAsync(Func<Task> workToPerform,
            TimeSpan dueTime, TimeSpan interval, bool runImmediately, CancellationToken token)
        {
            if (runImmediately)
                await workToPerform();

            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                await workToPerform();

                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
        }

        public static async Task DoWorkAsync(Action workToPerform,
            TimeSpan dueTime, TimeSpan interval, bool runImmediately, CancellationToken token)
        {
            if (runImmediately)
                workToPerform();

            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                workToPerform();

                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
        }
    }
}
