using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Consumers
{
    internal class SqlDirect : ContextConsumerBase, ISqlDirect
    {
        Stack<IDataReader> _stk;
        internal SqlDirect(IContextProvider arg) : base(arg)
        {
            _stk = new Stack<IDataReader>();
        }

        T ISqlDirect.ExecuteScalar<T>(string name, Dictionary<string, object> args)
        {
            T ret = default(T);

            using (var cmd = _provider.DataContext.Database.Connection.CreateCommand())
            {
                cmd.CommandText = name;
                cmd.CommandType = System.Data.CommandType.Text;
                if (args != null)
                    AttachParameters(cmd, args);

                EnsureConnectionOpen();
                var obj = cmd.ExecuteScalar();
                ret = (T) Convert.ChangeType(obj, typeof(T));
            }
            return ret;
        }
        void ISqlDirect.ExecuteNonQuery(string name, Dictionary<string, object> args)
        {
            using (var cmd = _provider.DataContext.Database.Connection.CreateCommand())
            {
                cmd.CommandText = name;
                cmd.CommandType = System.Data.CommandType.Text;
                
                if (args != null)
                    AttachParameters(cmd, args);

                EnsureConnectionOpen();
                cmd.ExecuteNonQuery();
            }
        }
        IDataReader ISqlDirect.ExecuteReader(string sql, Dictionary<string, object> args)
        {
            using (var cmd = _provider.DataContext.Database.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = System.Data.CommandType.Text;

                if (args != null)
                    AttachParameters(cmd, args);
                var rd = cmd.ExecuteReader(CommandBehavior.Default);
                _stk.Push(rd);
                return rd;
            }
        }
        long ISqlDirect.GetNumber(string name)
        {
            EnsureConnectionOpen();
           return (this as ISqlDirect).ExecuteScalar<long>($"SELECT NEXT VALUE FOR [{name}]");
        }



        private void AttachParameters(DbCommand cmd, Dictionary<string, object> args)
        {
            foreach (var key in args.Keys)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = key.StartsWith("@")? key: "@"+key;
                p.Value = args[key];
                cmd.Parameters.Add(p);
            }
        }

        protected override void Dispose(bool disposing)
        {
            var builder = new AggregateExceptionBuilder("Some DataReader objects might be open");
            
            while (_stk.Count > 0)
            {
                var rd = _stk.Pop();
                if (!rd.IsClosed)
                    SandBox.TRY(() => rd.Dispose(), (ex) => builder.Add(ex));
            }

            base.Dispose(disposing);

            if (builder.HasErrors)
                throw builder.ToAggregateException();
        }
    }
}
