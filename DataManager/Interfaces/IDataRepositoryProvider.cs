using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDataManager.Interfaces
{
    public interface IDataRepositoryProvider:IDisposable
    {
        IDataRepository Repository { get; }
    }
}
