using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenericDataManager.Common
{
    public class ThreadRest:IDisposable
    {
        readonly TimeSpan _waitDuration;
        readonly ManualResetEventSlim _signal;

        public ThreadRest(TimeSpan duration)
        {
            _waitDuration = duration;
            _signal = new ManualResetEventSlim(false);
        }
        public void Dispose()
        {
            _signal.Dispose();
        }

        public bool Snooze()=> _signal.Wait(_waitDuration);
        public void Wakeup() => _signal.Set();
        public void Reset() => _signal.Reset();

        public bool ShouldContinue => !_signal.IsSet;
    }
}
