using System;
using System.Collections.Generic;
using System.Data;

namespace GenericDataManager.Interfaces
{
    public interface ISqlDirect: IDisposable
    {
       T ExecuteScalar<T>(string sql, Dictionary<string, object> args = null);
       void ExecuteNonQuery(string sql, Dictionary<string, object> args = null);
       IDataReader ExecuteReader(string sql, Dictionary<string, object> args = null);
       long GetNumber(string sequenceName);
    }
}