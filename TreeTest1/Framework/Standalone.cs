namespace DirectEve
{
    using System;
    using D3DDetour;
    using System.Diagnostics;

    public class StandaloneFramework : IFramework
    {
        public void RegisterFrameHook(EventHandler<EventArgs> frameHook)
        {
            Pulse.Initialize(D3DVersion.Direct3D9);
            D3DHook.OnFrame += frameHook;
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
        }
        #endregion
    }
}