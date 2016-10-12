using System;
using System.Linq;

namespace GenericDataManager.Common
{
    internal static class Konstants
    {
        internal const string MultipleActiveRecordSets = "MultipleActiveResultSets=True";
        internal const string EntityFramework = "EntityFramework";

        internal static readonly TimeSpan DefaultHeartbeat = TimeSpan.FromSeconds(3);
        internal static readonly TimeSpan DefaultMinimumAge = TimeSpan.FromSeconds(3);
        internal const int EstimatedThreads=10;

        public static readonly TimeSpan DefaultDisposalWait = TimeSpan.FromSeconds(10);
        public const int DefaultRetries=3;

        internal static Tuple<int,int, int, int> GetEntityFrameworkVersion()
        {
            var version = "";
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName).ToList();
            foreach(var asm in assemblies)
            {
                var tmp = asm.Split(new char[] { ',', '{', '}' }, StringSplitOptions.RemoveEmptyEntries).Select(x=> x.Trim()).ToList();
                if(string.Compare(tmp[0], EntityFramework, true)==0)
                {
                    version = tmp[1].Split(new char[] {'='}, StringSplitOptions.RemoveEmptyEntries)[1];
                    break;
                }
            }

            var fragments = version.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(x=> Int32.Parse(x));
            return Tuple.Create<int, int, int, int>(fragments[0], fragments[1], fragments[2], fragments[3]);
        }
    }
}
