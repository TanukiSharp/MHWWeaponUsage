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
        public static async void Forget(this Task task, Action<Exception> onError = null)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            try
            {
                if (ExecutionContext.IsFlowSuppressed() == false)
                    ExecutionContext.SuppressFlow();

                await task;
            }
            catch (Exception ex)
            {
                if (onError != null)
                    onError(ex);
                else
                    throw;
            }
        }
    }
}
