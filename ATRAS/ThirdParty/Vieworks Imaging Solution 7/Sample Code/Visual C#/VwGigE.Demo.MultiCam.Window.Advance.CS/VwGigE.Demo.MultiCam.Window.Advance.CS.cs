using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VwGigE.Demo.SingleCam.window.Advance.C
{
    static class VwGigE
    {
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CVwGigE_Demo_SingleCam_Window_Advance_CS());
        }
    }
}