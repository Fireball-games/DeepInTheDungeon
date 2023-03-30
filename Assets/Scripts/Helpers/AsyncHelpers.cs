using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Helpers
{
    public static class AsyncHelpers
    {
        public static async Task WaitForEndOfFrameAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            // Start a coroutine to signal completion of the task
            IEnumerator Coroutine()
            {
                yield return new WaitForEndOfFrame();
                tcs.SetResult(true);
            }
            
            CoroutineRunner.Run(Coroutine());

            // Wait for the task to complete
            await tcs.Task;
        }
    }
}