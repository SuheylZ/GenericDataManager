using System.Threading;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Interfaces
{
    public interface IContextMap
    {
        ContextProviderThreadPair this[int tid] { get; }

        int[] Keys { get; }
        ContextProviderThreadPair[] Values { get; }

        void Add(Thread th, IContextProvider provider);
        ContextProviderThreadPair[] Clear();
        bool Has(Thread thread);
        IContextProvider Remove(int id);
        int Count { get; }
    }
}