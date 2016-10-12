using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenericDataManager.Interfaces;
using GenericDataManager.Strategies;

namespace GenericDataManager.Common
{
    internal class Cleaner<TStrategy>: 
        ICleaner 
        where TStrategy : CleaningStrategy, IDisposable
    {
        readonly Thread _cleaner;
        readonly Tuple<IContextMap, ExecutionPolicy, ManualResetEvent, ManualResetEvent>  _threadArgs;
        readonly ExecutionPolicy KPolicy;

        ManualResetEvent _pauseEvent, _stopEvent; 

        internal Cleaner(IContextMap map, ExecutionPolicy policy)
        {
            KPolicy = policy;
            _pauseEvent = new ManualResetEvent(false);
            _stopEvent = new ManualResetEvent(false);

            _threadArgs = new Tuple<IContextMap, ExecutionPolicy, ManualResetEvent, ManualResetEvent>(map, policy, _pauseEvent, _stopEvent);

            _cleaner = new Thread(new ParameterizedThreadStart(CleanerLifePath));
            _cleaner.Name = "GenericDM Demon Cleaner";
            _cleaner.Priority = ThreadPriority.BelowNormal;
            _cleaner.IsBackground = true;
        }


        internal void CleanerLifePath(object threadArgs)
        {
            TimeSpan waitDuration = TimeSpan.FromMilliseconds(100);
            var args = threadArgs as Tuple<IContextMap, ExecutionPolicy, ManualResetEvent, ManualResetEvent>;
            ManualResetEvent pause = args.Item3, dispose = args.Item4;

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            using (var cleaner = Activator.CreateInstance(typeof(TStrategy), flags, null, new Object[] { args.Item1, args.Item2 }, null) as TStrategy)
            {
                while (true)
                {
                    Thread.Sleep(KPolicy.HeartBeat);
                    cleaner.Clean();
                    while (pause.WaitOne(TimeSpan.Zero))
                    {
                        Thread.Sleep(waitDuration);
                    }

                    if (dispose.WaitOne(TimeSpan.Zero))
                        break;
                }
            }
        }

        public void Start()
        {
           _cleaner.Start(_threadArgs);
        }

        public void Pause()
        {
            _pauseEvent.Set();
        }
        public void Resume()
        {
            _pauseEvent.Reset();
        }

        public void Dispose()
        {
            _stopEvent.Set();
            _cleaner.Join();
        }
    }
}

