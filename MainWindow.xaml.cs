using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fearware
{
    public partial class FormStart : Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        private Point newPos;
        private Point oldPos;
        public static bool isActive=true;
        public static DateTime lastMove;
        public FormStart()
        {
            Thread RevealMove= new Thread(() =>
            {

                DetectUserMovement();

            });
            RevealMove.SetApartmentState(ApartmentState.STA);
            RevealMove.Start();

            Core mwCore = new Core();

            this.Opacity = 0;
            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Hidden;
            InitializeComponent();
        }


        private bool DetectUserMovement()
        {
            while (true)
            {
                Thread.Sleep(400);
                GetCursorPos(out newPos);
                if (!(newPos.Equals(oldPos)))
                {
                    lastMove = DateTime.Now;
                    oldPos = newPos;
                    isActive = true;
                }
                if (DateTime.Now.Subtract(lastMove).TotalSeconds > 1200)
                {
                    isActive = false;
                }
            }
        }

       
    }
}
