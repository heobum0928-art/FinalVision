using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Project.BaseLib.Extension
{
  internal struct VoidTypeStruct { }
  public static class TaskExtensions
  {
    public static bool IsTaskActive(this Task task)
    {
      return task != null && (task.Status == TaskStatus.Running || task.Status == TaskStatus.WaitingToRun || task.Status == TaskStatus.WaitingForActivation);
    }

    public static bool IsTaskActive<T>(this TaskCompletionSource<T> tcs)
    {
      return tcs != null && tcs.Task != null && (tcs.Task.Status == TaskStatus.Running || tcs.Task.Status == TaskStatus.WaitingToRun || tcs.Task.Status == TaskStatus.WaitingForActivation);
    }

    public static async Task TimeoutAfter(this Task task, int millisecondsTimeout, string description = null)
    {
      var cancellationToken = new CancellationTokenSource();
      Task delay = Task.Delay(millisecondsTimeout, cancellationToken.Token);

      if (task == await Task.WhenAny(task, delay))
      {
        cancellationToken.Cancel();
        delay.Dispose();







        await task;
      }
      else
        throw new TimeoutException(description);
    }

    public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout, string description = null)
    {
      var cancellationToken = new CancellationTokenSource();
      Task delay = Task.Delay(millisecondsTimeout, cancellationToken.Token);

      if(task == await Task.WhenAny(task, delay))
      {
        cancellationToken.Cancel();
        delay.Dispose();
        return await task;
      }
      else
      {
        throw new TimeoutException(description);
      }
    }
  }
}
