namespace DirectEve
{
    using System;
    using D3DDetour;
    using System.Diagnostics;

    public class StandaloneFramework : IFramework
    {
        private EventHandler<EventArgs> _frameHook = null;

        public void RegisterFrameHook(EventHandler<EventArgs> frameHook)
        {
            Pulse.Initialize(D3DVersion.Direct3D9);
            _frameHook = frameHook;
            D3DHook.OnFrame += _frameHook;
        }

        public void RegisterLogger(EventHandler<EventArgs> logger)
        {
        }

        public void Log(string msg)
        {
            Debugger.Log(0, "", msg);
        }

        #region IDisposable Members
        public void Dispose()
        {
            D3DHook.OnFrame -= _frameHook;
            Pulse.Shutdown();
        }
        #endregion
    }
}