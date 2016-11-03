using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenericDataManager.Common
{
    /// <summary>
    /// Provides quick locking using Interlocked.CompareExchange with a specified delay
    /// </summary>
    public class QuickLock
    {
        const int K_MAX_TRIES = 10;

        volatile int _value;

        readonly int KDelay;
        readonly TimeSpan KMaxWaitingTime;
        readonly string KMessage;


        /// <summary>
        /// Instantiates a QuickLock with a specified duration to use between successive lock checking
        /// </summary>
        /// <param name="maxWait">The duration between two successive lock checking</param>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public QuickLock(TimeSpan maxWait)
        {
            Interlocked.Exchange(ref _value, 0);

            KMaxWaitingTime = maxWait;
            KDelay = Convert.ToInt32(maxWait.TotalMilliseconds / K_MAX_TRIES);
            KMessage = $"Cannot continue further. Lock is longer than {TimeSpan.FromMilliseconds(KDelay * K_MAX_TRIES).TotalSeconds} second(s)";

        }

        /// <summary>
        /// Tries 10 times to gain the lock. If it fails after 10 tries, throws TimeoutExpiration exception  
        /// </summary>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Lock()
        {
            var tries = K_MAX_TRIES;

            while (Interlocked.CompareExchange(ref _value, 1, 0)!=0)
            {
                if(tries--<1)
                    throw new TimeoutException(KMessage);
                Thread.Sleep(KDelay);
            }
       }

        public bool IsLocked => Interlocked.CompareExchange(ref _value, -1, -1) == 1;
      


        /// <summary>
        /// Unlocked a previous lock 
        /// </summary>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Unlock() {
           Interlocked.Exchange(ref _value, 0);
        }

#if DEBUG
        public override string ToString()
        {
            Interlocked.MemoryBarrier();
            var ret= $"{_value}";
            return ret;
        }
#endif

    }
}
