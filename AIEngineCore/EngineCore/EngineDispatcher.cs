namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using System;

    public class EngineDispatcher<T>
    {
        private IEngineQueue<T> _EngineQueue;

        public EngineDispatcher(IEngineQueue<T> engineQueue)
        {
            _EngineQueue = engineQueue;
        }

        public ValueTask dispatchNotification()
        {
            //Diversion takes place from here if it is email notification pushes to email notification.
            throw new NotImplementedException();
        }
    }
}