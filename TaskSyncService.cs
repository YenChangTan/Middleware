using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware
{
    public class TaskSyncService
    {
        public TaskCompletionSource<bool> ProceedTaskSource { get; private set; } = new TaskCompletionSource<bool>();

        public void Reset()
        {
            ProceedTaskSource = new TaskCompletionSource<bool>();
        }
    }
}
