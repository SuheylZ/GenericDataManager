using System;

namespace GenericDataManager.Common
{
    public enum Strategy
    {
        DoNothing,
        DisposeWhenNotInUse,
        DisposeLeastRecentlyUsed,
        DisposeWithThread
    }

    public enum ManagerDisposalStrategy
    {
        Default,
        DisposeButThrowIfInUse,
        DisposeSilentlyEvenIfInUse

        // They have been removed after a comment received: 
        // If it's time to dispose, why waiting and retrying or even not disposing at all?

        //RetryUntilDisposedOrFail,   
        //FailIfNotDisposed,
    }


    public class ExecutionPolicy
    {
        public Strategy PeriodicDisposalStrategy { get; set; } 
        public TimeSpan HeartBeat { get; set; }
        public TimeSpan MinimumAge { get; set; }
        public TimeSpan DisposalWait { get; set; }

        //public int RetryCount { get; set; }

        public ManagerDisposalStrategy FinalDisposalBehaviour { get; set; }


        public ExecutionPolicy()
        {
           PeriodicDisposalStrategy = Strategy.DoNothing;
           HeartBeat = Konstants.DefaultHeartbeat;
           MinimumAge = Konstants.DefaultMinimumAge;

            FinalDisposalBehaviour = ManagerDisposalStrategy.Default;
            DisposalWait = Konstants.DefaultDisposalWait;

            //RetryCount = Konstants.DefaultRetries;
        }
    }
}
