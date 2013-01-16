using System;
using System.Runtime.InteropServices;
using WhiteMagic;

namespace D3DDetour
{
    public abstract class D3DHook
    {
        protected static readonly object _frameLock = new object();
        public static event EventHandler<EventArgs> OnFrame;

        public delegate void OnFrameDelegate();
        public static event OnFrameDelegate OnFrameOnce;

        public abstract void Initialize();

        protected void RaiseEvent()
        {
            lock (_frameLock)
            {
                if (OnFrame != null)
                    OnFrame(null, new EventArgs());

                if (OnFrameOnce != null)
                {
                    OnFrameOnce();
                    OnFrameOnce = null;
                }
            }   
        }
    }

    public enum D3DVersion
    {
        Direct3D9,
        Direct3D10,
        Direct3D10_1,
        Direct3D11,
    }
#if FALSE
    [StructLayout(LayoutKind.Sequential)]
    public struct DXGI_MODE_DESC
    {
        public int Width;
        public int Height;
        public Rational RefreshRate;
        public Format Format;
        public DisplayModeScanlineOrdering ScanlineOrdering;
        public DisplayModeScaling Scaling;
    }
#endif
}
