using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenericDataManager.Common
{
    public class SharedData<T> where T:class
    {
        static volatile T _data;
        ReaderWriterLockSlim _lock;

        public SharedData(T arg)
        {
            Interlocked.Exchange<T>(ref _data, arg);
            _lock = new ReaderWriterLockSlim();
        }

        public T Value
        {
            get
            {
                _lock.EnterReadLock();
                var ret = Interlocked.CompareExchange<T>(ref _data, default(T), default(T));
                _lock.ExitReadLock();

                return ret;
            }
            set
            {
                _lock.EnterWriteLock();
                Interlocked.Exchange<T>(ref _data, value);
                _lock.ExitWriteLock();
            }
        }
    }
}
