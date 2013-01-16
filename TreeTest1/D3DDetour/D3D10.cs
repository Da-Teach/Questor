using System;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.Windows;
using WhiteMagic.Internals;
using Device = SlimDX.Direct3D10.Device;

namespace D3DDetour
{
    public class D3D10 : D3DHook
    {
        private Direct3D10Present _presentDelegate;
        private Detour _presentHook;

        const int VMT_PRESENT = 8;
        const int VMT_RESIZETARGET = 14;

        public IntPtr PresentPointer = IntPtr.Zero;
        public IntPtr ResetTargetPointer = IntPtr.Zero;

        public override void Initialize()
        {
            using (var fac = new Factory())
            {
                using (var tmpDevice = new Device(fac.GetAdapter(0), DriverType.Hardware, DeviceCreationFlags.None))
                {
                    using (var rf = new RenderForm())
                    {
                        var desc = new SwapChainDescription
                        {
                            BufferCount = 1,
                            Flags = SwapChainFlags.None,
                            IsWindowed = true,
                            ModeDescription = new ModeDescription(100, 100, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                            OutputHandle = rf.Handle,
                            SampleDescription = new SampleDescription(1, 0),
                            SwapEffect = SwapEffect.Discard,
                            Usage = Usage.RenderTargetOutput
                        };
                        using (var sc = new SwapChain(fac, tmpDevice, desc))
                        {
                            PresentPointer = Pulse.Magic.GetObjectVtableFunction(sc.ComPointer, VMT_PRESENT);
                            ResetTargetPointer = Pulse.Magic.GetObjectVtableFunction(sc.ComPointer, VMT_RESIZETARGET);
                        }
                    }
                }
            }

            _presentDelegate = Pulse.Magic.RegisterDelegate<Direct3D10Present>(PresentPointer);
            _presentHook = Pulse.Magic.Detours.CreateAndApply(_presentDelegate, new Direct3D10Present(Callback), "D10Present");
        }

        private int Callback(IntPtr swapChainPtr, int syncInterval, PresentFlags flags)
        {
            RaiseEvent();
            return (int)_presentHook.CallOriginal(swapChainPtr, syncInterval, flags);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int Direct3D10Present(IntPtr swapChainPtr, int syncInterval, PresentFlags flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int Direct3D10ResizeTarget(IntPtr swapChainPtr, ref DXGI_MODE_DESC newTargetParameters);
    }
}
