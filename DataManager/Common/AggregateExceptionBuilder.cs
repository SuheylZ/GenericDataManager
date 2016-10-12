using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDataManager.Common
{
    public class AggregateExceptionBuilder
    {
        readonly List<Exception> _errors;
        readonly string _message;

        public AggregateExceptionBuilder(string message = "")
        {
            _message = message;
            _errors = new List<Exception>();
        }

        public bool HasErrors => _errors.Count > 0;
        public void Add(Exception ex) => _errors.Add(ex);
        public AggregateException ToAggregateException() => string.IsNullOrEmpty(_message) ? new AggregateException(_errors.ToArray()) : new AggregateException(_message, _errors.ToArray());
    }
}
