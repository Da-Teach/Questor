using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WhiteMagic.Internals;

namespace D3DDetour
{
    public class D3D9 : D3DHook
    {
        private Direct3D9EndScene _endSceneDelegate;
        private Detour _endSceneHook;

        public IntPtr EndScenePointer = IntPtr.Zero;
        public IntPtr ResetPointer = IntPtr.Zero;
        public IntPtr ResetExPointer = IntPtr.Zero;

        const int VMT_ENDSCENE = 42;
        const int VMT_RESET = 16;

        public override void Initialize()
        {
            // Create a new 'window'
            var window = new Form();

            // Grabbed from d3d9.h
            const uint D3D_SDK_VERSION = 32;

            // Create the IDirect3D* object.
            IntPtr direct3D = Direct3DCreate9(D3D_SDK_VERSION);

            // Make sure it's valid. (Should always be valid....)
            if (direct3D == IntPtr.Zero)
                throw new Exception("Failed to create D3D.");

            // Setup some present params...
            var d3dpp = new D3DPRESENT_PARAMETERS { Windowed = true, SwapEffect = 1, BackBufferFormat = 0 };

            IntPtr device;

            // CreateDevice is a vfunc of IDirect3D. Hence; why this entire thing only works in process! (Unless you
            // know of some way to hook funcs from out of process...)
            // It's the 16th vfunc btw.
            // Check d3d9.h
            var createDevice = Pulse.Magic.RegisterDelegate<IDirect3D9_CreateDevice>(Pulse.Magic.GetObjectVtableFunction(direct3D, 16));

            // Pass it some vals. You can check d3d9.h for what these actually are....
            if (createDevice(direct3D, 0, 1, window.Handle, 0x20, ref d3dpp, out device) < 0)
                throw new Exception("Failed to create device.");

            EndScenePointer = Pulse.Magic.GetObjectVtableFunction(device, VMT_ENDSCENE);
            ResetPointer = Pulse.Magic.GetObjectVtableFunction(device, VMT_RESET);

            // We now have a valid pointer to the device. We can release the shit we don't need now. :)
            // Again, the Release() funcs are virtual. Part of the IUnknown interface for COM.
            // They're the 3rd vfunc. (2nd index)
            var deviceRelease = Pulse.Magic.RegisterDelegate<D3DVirtVoid>(Pulse.Magic.GetObjectVtableFunction(device, 2));
            var d3dRelease = Pulse.Magic.RegisterDelegate<D3DVirtVoid>(Pulse.Magic.GetObjectVtableFunction(direct3D, 2));

            // And finally, release the device and d3d object.
            deviceRelease(device);
            d3dRelease(direct3D);

            // Destroy the window...
            window.Dispose();

            // Hook endscene
            _endSceneDelegate = Pulse.Magic.RegisterDelegate<Direct3D9EndScene>(EndScenePointer);
            _endSceneHook = Pulse.Magic.Detours.CreateAndApply(_endSceneDelegate, new Direct3D9EndScene(Callback), "D9EndScene");
        }

        private int Callback(IntPtr device)
        {
            RaiseEvent();
            return (int)_endSceneHook.CallOriginal(device);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int Direct3D9Reset(IntPtr device, IntPtr presentationParameters);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int Direct3D9ResetEx(IntPtr presentationParameters, IntPtr displayModeEx);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int Direct3D9EndScene(IntPtr device);

        #region DLL Imports

        [DllImport("d3d9.dll")]
        private static extern IntPtr Direct3DCreate9(uint sdkVersion);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void D3DVirtVoid(IntPtr instance);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int IDirect3D9_CreateDevice(IntPtr instance, uint adapter, uint deviceType,
                                                     IntPtr focusWindow,
                                                     uint behaviorFlags,
                                                     [In] ref D3DPRESENT_PARAMETERS presentationParameters,
                                                     [Out] out IntPtr returnedDeviceInterface);

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        private struct D3DPRESENT_PARAMETERS
        {
            public readonly uint BackBufferWidth;
            public readonly uint BackBufferHeight;
            public uint BackBufferFormat;
            public readonly uint BackBufferCount;
            public readonly uint MultiSampleType;
            public readonly uint MultiSampleQuality;
            public uint SwapEffect;
            public readonly IntPtr hDeviceWindow;
            [MarshalAs(UnmanagedType.Bool)]
            public bool Windowed;
            [MarshalAs(UnmanagedType.Bool)]
            public readonly bool EnableAutoDepthStencil;
            public readonly uint AutoDepthStencilFormat;
            public readonly uint Flags;
            public readonly uint FullScreen_RefreshRateInHz;
            public readonly uint PresentationInterval;
        }

        #endregion
    }
}
