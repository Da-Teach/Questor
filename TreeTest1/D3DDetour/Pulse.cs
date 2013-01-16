using System;
using System.Windows.Forms;
using WhiteMagic;
using WhiteMagic.Internals;

namespace D3DDetour
{
    public static class Pulse
    {
        public static readonly Magic Magic = new Magic();
        private static D3DHook Hook;

        public static void Initialize(D3DVersion ver)
        {
            switch (ver)
            {
                case D3DVersion.Direct3D9:
                    Hook = new D3D9();
                    break;
#if FALSE
                case D3DVersion.Direct3D10:
                case D3DVersion.Direct3D10_1:
                    Hook = new D3D10();
                    break;
                case D3DVersion.Direct3D11:
                    Hook = new D3D11();
                    break;
#endif
            }

            if (Hook == null)
                throw new Exception("Hook = null!");

            Hook.Initialize();
        }
    }
}
