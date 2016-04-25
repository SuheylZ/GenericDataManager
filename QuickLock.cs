using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataManager.Common
{
    public class QuickLock
    {
        volatile int _value = 0;
        readonly long _maxWait;

        public QuickLock():this(TimeSpan.FromSeconds(1)){
        }
        public QuickLock(TimeSpan maxWait) {
            _maxWait = Convert.ToInt64(maxWait.TotalSeconds*1000);
        }

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

        public bool IsLocked {
            get
            {
                return _value==1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock() {
           Interlocked.Exchange(ref _value, 0);
        }
    }
}
