using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DDetour
{
    public class Example
    {
        const D3DVersion dxver = D3DVersion.Direct3D9;
        static Example _ex;

        public static int MyMethod(String pwzArgument)
        {
            System.Diagnostics.Debugger.Log(0, null, "MyMethod" + Environment.NewLine);
            _ex = new Example();
            return 0;
        }

        public Example()
        {
            // Initialize the hook
            Pulse.Initialize(dxver);

            // Set something that will only run 1 time in an OnFrame
            D3DHook.OnFrameOnce += delegate
            {
                System.Diagnostics.Debugger.Log(0, null, "OnFrameOnce hook." + Environment.NewLine);
            };

            // Make the hook call our method each frame
            D3DHook.OnFrame += new EventHandler(D3DHook_OnFrame);
        }

        void D3DHook_OnFrame(object sender, EventArgs e)
        {
            // Pulse your components that needs to be run in
            // EndScene or Present here
            // e.g. ObjectManager or LUA stuff
        }
    }
}
