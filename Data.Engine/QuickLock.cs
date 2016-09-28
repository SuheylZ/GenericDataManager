using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataManager.Common
{
    /// <summary>
    /// Provides quick locking using Interlocked.CompareExchange with a specified delay
    /// </summary>
    public class QuickLock
    {
        volatile int _value = 0;
        readonly long _maxWait;

        /// <summary>
        /// Instantiates a QuickLock with a wait of 2 seconds
        /// </summary>
        public QuickLock():this(TimeSpan.FromSeconds(1)){
        }
        /// <summary>
        /// Instantiates a QuickLock with a specified duration to use between successive lock checking
        /// </summary>
        /// <param name="maxWait">The duration between two successive lock checking</param>
        public QuickLock(TimeSpan maxWait) {
            _maxWait = Convert.ToInt64(maxWait.TotalSeconds*1000);
        }

        
        /// <summary>
        /// Tries 10 times to gain the lock. If it fails after 10 tries, throws TimeoutExpiration exception  
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock() {
            var tries = 10;
            while(Interlocked.CompareExchange(ref _value, 1, 0)!=0) {
                tries -=1;
                if(tries<1)
                    throw new TimeoutException(string.Format("Cannot continue further. Lock is longer than {0} second(s)", _maxWait/1000));
                else
                    Thread.Sleep(Convert.ToInt32(_maxWait/10));
            }
       }

        /// <summary>
        /// Checks if the lock can be acquired
        /// </summary>
        /// <returns>true: Already locked, false: can be locked</returns>
        public bool IsLocked {
            get
            {
                return _value==1;
            }
        }

        /// <summary>
        /// Unlocked a previous lock 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock() {
           Interlocked.Exchange(ref _value, 0);
        }
    }
}
