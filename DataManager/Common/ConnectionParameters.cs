using System;

namespace GenericDataManager.Common
{



    public struct ConnectionParameters
    {
        public string connection;
        public string modelResource;
        public string provider;
        public TimeSpan delay;






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
