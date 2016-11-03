// /*******************************************************
//  * Copyright (C) 2016 Suheyl Z
//  * 
//  * This file is part of DataManager.
//  * 
//  * It can not be copied and/or distributed without the express
//  * permission.
//  *******************************************************/

using System;
using System.Threading;

namespace GenericDataManager.Common
{
    public class ThreadRest:IDisposable
    {
        readonly TimeSpan _waitDuration;
        readonly ManualResetEventSlim _signal;

        public ThreadRest(TimeSpan duration)
        {
            _waitDuration = duration;
            _signal = new ManualResetEventSlim(false);
        }

        public void Dispose() => _signal.Dispose();
        public bool Snooze()=> _signal.Wait(_waitDuration);
        public void Wakeup() => _signal.Set();
        public void Reset() => _signal.Reset();

        public bool ShouldContinue => !_signal.IsSet;
    }
}
