using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QuestorSettings
{
    internal static class QuestorSettings
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new QuestorSettingsUI());
        }
    }
}