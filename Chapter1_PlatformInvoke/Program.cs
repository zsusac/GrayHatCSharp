using System;
using System.Runtime.InteropServices;

namespace Chapter1_PlatformInvoke
{
    class Program
    {
        [DllImport("user32", CharSet=CharSet.Auto)]
        private static extern int MessageBox(IntPtr hwnd, String text, String caption, int options); 

        [DllImport("libc")]
        private static extern void printf(string message);

        static void Main(string[] args)
        {
            OperatingSystem operatingSystem = Environment.OSVersion;

            if(operatingSystem.Platform == PlatformID.Unix)
            {
                printf("Hello World!");               
            } 
            else
            {
                MessageBox(IntPtr.Zero, "Hello World!", "Hello World!", 0);
            }
        }
    }
}
