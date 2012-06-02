using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace UpdateInvTypes
{
    internal static class UpdateInvTypes
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdateInvTypesUI());
        }
    }
}