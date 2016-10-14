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
        where TStrategy : CleaningStrategyBase, IDisposable
    {
        readonly ExecutionPolicy KPolicy; 
        readonly Tuple<IContextMap, ExecutionPolicy>  _threadArgs;

        readonly ThreadRest _rest;
        readonly Thread _cleaner;


        internal Cleaner(IContextMap map, ExecutionPolicy policy)
        {
            KPolicy = policy;
            _rest = new ThreadRest(KPolicy.HeartBeat);

            _threadArgs = new Tuple<IContextMap, ExecutionPolicy>(map, policy);

            _cleaner = new Thread(new ParameterizedThreadStart(CleanerLifePath));
            _cleaner.Name = Konstants.CleanerName;
            _cleaner.Priority = ThreadPriority.BelowNormal;
            _cleaner.IsBackground = true;
        }


        internal void CleanerLifePath(object threadArgs)
        {
            var args = threadArgs as Tuple<IContextMap, ExecutionPolicy>;

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            using (var cleaner = Activator.CreateInstance(typeof(TStrategy), flags, null, new Object[] { args.Item1, args.Item2 }, null) as TStrategy)
            {
                while (_rest.ShouldContinue)
                {
                    cleaner.Clean();
                    _rest.Snooze();
                }
            }
        }

        void ICleaner.Start()
        {
            _cleaner.Start(_threadArgs);
        }

        
        void IDisposable.Dispose()
        {
            _rest.Wakeup();
            _cleaner.Join();

            _rest.Dispose();
        }
    }
}

