using System;
using System.Data.Entity;
#if EF5
using System.Data.EntityClient;
using System.Data.Objects;
#else
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.EntityClient;
#endif
using GenericDataManager.Common;

namespace GenericDataManager.Providers
{
    internal class ContextProviderWithAge : ContextProviderBase
    {
        readonly DateTime _createdAt;
        DateTime _lastUsed;


        internal ContextProviderWithAge(ConnectionParameters arg):base(arg)
        {
            _createdAt = DateTime.Now;
            _lastUsed = _createdAt;
            ObjectContext.SavingChanges += (sender, args) => _lastUsed = DateTime.Now;
        }

        internal TimeSpan Age => DateTime.Now.Subtract(_createdAt);
        internal TimeSpan LastUsed => DateTime.Now.Subtract(_lastUsed);
        

        public override DbContext DataContext
        {
            get
            {
                _lastUsed = DateTime.Now;
                return base.DataContext;
            }
        }
        public override ObjectContext ObjectContext
        {
            get
            {
                _lastUsed = DateTime.Now;
                return base.ObjectContext;
            }
        }
    }
}
