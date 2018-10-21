using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHWWeaponUsage
{
    public static class TaskExtensions
    {
        public static void ForgetRethrowOnError(this Task task)
        {
            task.Forget(ex => throw new Exception("rethrow", ex));
        }

        // Based on Ben Adams suggestion
        public static void Forget(this Task task, Action<Exception> onError)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            if (onError == null)
                throw new ArgumentNullException(nameof(onError));

            // Pass paramters explicitly to async local function or it will allocate to pass them
            async Task ForgetAwaited(Task t, Action<Exception> onErr)
            {
                try
                {
                    // No need to capture AsyncLocals and restore them across the await (extending their lifetime)
                    if (ExecutionContext.IsFlowSuppressed() == false)
                        ExecutionContext.SuppressFlow();

                    // Resume on original SynchronizationContext, it's up to the caller to provide
                    // a task already configured to continue on the captured context or not
                    await task;
                }
                catch (Exception ex)
                {
                    onErr.Invoke(ex);
                }
            }

            // Only care about tasks that may fault or are faulted,
            // so fast-path for SuccessfullyCompleted and Cancelled tasks
            if (task.IsCompleted == false || task.IsFaulted)
                _ = ForgetAwaited(task, onError);
        }
    }
}
