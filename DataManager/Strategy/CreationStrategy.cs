using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;
using GenericDataManager.Providers;

namespace GenericDataManager.Strategies
{
    public class CreationStrategy
    {
        readonly ConnectionParameters KConnection;
        readonly ExecutionPolicy KPolicy;


        public CreationStrategy(ConnectionParameters connection, ExecutionPolicy policy)
        {
            KConnection = connection;
            KPolicy = policy;
        }

        public IContextProvider Create(Thread th)
        {
            ContextProviderBase provider = null;

            switch (KPolicy.PeriodicDisposalStrategy)
            {
                case Strategy.DisposeLeastRecentlyUsed:
                    provider = new SimpleContextProvider(KConnection); 
                    break;
                default:
                    provider = new ContextProviderWithAge(KConnection);
                    break;
            }

            return provider;
        }
    }
}
