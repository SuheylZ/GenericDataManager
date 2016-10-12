using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDataManager.Common
{
    public class SandBox
    {
        public static void TRY(Action action, Action<Exception> handler)
        {
            try
            {
                action();
            }
            catch(Exception ex)
            {
                handler(ex);
            }
        }
    }
}
