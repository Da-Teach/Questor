using System;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using WhiteMagic.Internals;
using Device = SlimDX.Direct3D11.Device;

namespace D3DDetour
{
    public class D3D11 : D3DHook
    {
        private Direct3D11Present _presentDelegate;
        private Detour _presentHook;

        const int VMT_PRESENT = 8;
        const int VMT_RESIZETARGET = 14;

        public IntPtr PresentPointer = IntPtr.Zero;
        public IntPtr ResetTargetPointer = IntPtr.Zero;

        public override void Initialize()
        {
            Device tmpDevice;
            SwapChain sc;
            using (var rf = new RenderForm())
            {
                var desc = new SwapChainDescription
                {
                    BufferCount = 1,
                    Flags = SwapChainFlags.None,
                    IsWindowed = true,
                    ModeDescription = new ModeDescription(100, 100, new Rational(60, 1), SlimDX.DXGI.Format.R8G8B8A8_UNorm),
                    OutputHandle = rf.Handle,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,
                    Usage = Usage.RenderTargetOutput
                };

                var res = Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out tmpDevice, out sc);
                if (res.IsSuccess)
                {
                    using (tmpDevice)
                    {
                        using (sc)
                        {
                            PresentPointer = Pulse.Magic.GetObjectVtableFunction(sc.ComPointer, VMT_PRESENT);
                            ResetTargetPointer = Pulse.Magic.GetObjectVtableFunction(sc.ComPointer, VMT_RESIZETARGET);
                        }
                    }
                }
            }

            _presentDelegate = Pulse.Magic.RegisterDelegate<Direct3D11Present>(PresentPointer);
            _presentHook = Pulse.Magic.Detours.CreateAndApply(_presentDelegate, new Direct3D11Present(Callback), "D11Present");
        }

        private int Callback(IntPtr swapChainPtr, int syncInterval, PresentFlags flags)
        {
            RaiseEvent();
            return (int)_presentHook.CallOriginal(swapChainPtr, syncInterval, flags);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int Direct3D11Present(IntPtr swapChainPtr, int syncInterval, PresentFlags flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int Direct3D11ResizeTarget(IntPtr swapChainPtr, ref DXGI_MODE_DESC newTargetParameters);        
    }
}
