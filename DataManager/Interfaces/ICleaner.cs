using System;

namespace GenericDataManager.Interfaces
{
    public interface ICleaner:IDisposable
    {
        void Start();
        void Pause();
        void Resume();
    }
}