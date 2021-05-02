using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace FullscreenLock
{
    class Checker
    {
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();

        // Import a bunch of win32 API calls.
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hwnd, out RECT rc);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ClipCursor(ref RECT rcClip);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        private static string[] whitelist = new[] { "chrome", "RaceControl" };

        Label l; // One day I'll figure out how to set the label without sending a pointer into the constructor.
        public Checker(Label ll)
        {

            l = ll;
            t.Tick += new EventHandler(CheckForFullscreenApps);
            t.Interval = 100;
            t.Start();
        }

        public void toggle(Button b, Label l)
        {
            if(t.Enabled)
            {
                t.Stop();
                l.Text = "Paused";
            }
            else
            {
                t.Start();
                l.Text = "Waiting for focus";
            }
        }
        
        private void CheckForFullscreenApps(object sender, System.EventArgs e)
        {
            string sProcName = "";
            if (IsForegroundFullScreenAndNotWhitelisted(out sProcName))
            {
                
                l.Text = sProcName + " in focus";
            }
            else
            {
                l.Text = "Waiting for focus";
                
            }
        }

        public static bool IsForegroundFullScreenAndNotWhitelisted(out string sProcName)
        {
            try
            {
                //Get the handles for the desktop and shell now.
                IntPtr desktopHandle;
                IntPtr shellHandle;
                desktopHandle = GetDesktopWindow();
                shellHandle = GetShellWindow();
                RECT appBounds;
                Rectangle screenBounds;
                IntPtr hWnd;
                sProcName = "";
                hWnd = GetForegroundWindow();
                if (hWnd != null && !hWnd.Equals(IntPtr.Zero))
                {
                    //Check we haven't picked up the desktop or the shell
                    if (!(hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle)))
                    {
                        GetWindowRect(hWnd, out appBounds);
                        //determine if window is fullscreen
                        screenBounds = Screen.FromHandle(hWnd).Bounds;
                        uint procid = 0;
                        GetWindowThreadProcessId(hWnd, out procid);
                        var proc = Process.GetProcessById((int)procid);
                        sProcName = proc.ProcessName;
                        if ((appBounds.Bottom - appBounds.Top) == screenBounds.Height && (appBounds.Right - appBounds.Left) == screenBounds.Width && !whitelist.Contains(proc.ProcessName))
                        {
                            Console.WriteLine(proc.ProcessName);
                            Cursor.Clip = screenBounds;
                            return true;
                        }
                        else
                        {
                            Cursor.Clip = Rectangle.Empty;
                            return false;
                        }
                    }
                }
            } catch (Exception) { }
            sProcName = "";
             return false;
         }
    }
}

