using System;
using System.Diagnostics.Contracts;
using System.Linq;

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
            Contract.Requires(cnnStr!=null);

            connection = cnnStr.Contains(Konstants.MultipleActiveRecordSets) ? cnnStr : string.Concat(cnnStr, Konstants.MultipleActiveRecordSets, ";");
            modelResource = string.Format(@"res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", modelName);
            provider = providerName;
            delay = TimeSpan.FromSeconds(secondsDelay);
        }
    }

    

}
