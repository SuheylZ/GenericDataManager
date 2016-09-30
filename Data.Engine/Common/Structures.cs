using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Common
{
    public struct ConnectionParameters
    {
        public readonly string connection;
        public readonly string modelResource;
        public readonly string provider;
        public readonly TimeSpan delay;

        public ConnectionParameters(string cnnStr, string modelName, int secondsDelay=30, string providerName = "System.Data.SqlClient")
        {
            connection = cnnStr.IndexOf(Konstants.MultipleActiveRecordSets)==-1?
                string.Concat(cnnStr, Konstants.MultipleActiveRecordSets, ";") :
                cnnStr;
            modelResource = string.Format(@"res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", modelName);
            provider = providerName;
            delay = TimeSpan.FromSeconds(secondsDelay);
        }
    }

    

}
