using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WorldsBestBars.Process.RssXmlGen
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool bAutostart = false;
            if (args.Length > 0) 
                if(args[0].ToLower() == "-autostart")
                    bAutostart = true;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain(bAutostart));
        }
    }
}
