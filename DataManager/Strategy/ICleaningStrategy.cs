using System;

namespace GenericDataManager.Interfaces
{
    public interface ICleaningStrategy:IDisposable
    {
        void Start();
        void Stop();
    }
}