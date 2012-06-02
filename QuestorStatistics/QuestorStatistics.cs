using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QuestorStatistics
{
    internal static class QuestorStatistics
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new QuestorStatisticsUI());
        }
    }
}