using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Common
{
    class Countable<T>
    {
            public T Item { get; }
            public int Count { get; private set; }

            public Countable(T arg)
            {
                Item = arg;
                Count = 0;
            }
            public void Increment()
            {
                Count += 1;
            }
            public void Decrement()
            {
                Count = (Count > 0) ? Count - 1 : 0;
            }
#if DEBUG
        public override string ToString()
        {
            return $"({Item}, {Count})";
        }
#endif

    }
    
}
