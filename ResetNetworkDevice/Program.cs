using System;
using System.Management;

namespace ResetNetworkDevice
{
    class Program
    {
        private const int SW_HIDE = 0;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            args = new string[1];
            args[0] = "Realtek PCIe GbE Family Controller";
            
            ShowWindow(GetConsoleWindow(), SW_HIDE);

            if (args.Length == 1)
            {
                DisableNetworkAdapter(args[0]);
                System.Threading.Thread.Sleep(2000);
                EnableNetworkAdapter(args[0]);
            }
        }

        private static void DisableNetworkAdapter(string adapterName)
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapter");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if (mo["Name"].ToString().Equals(adapterName))
                {
                    mo.InvokeMethod("Disable", null);
                    break;
                }
            }
        }

        private static void EnableNetworkAdapter(string adapterName)
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapter");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if (mo["Name"].ToString().Equals(adapterName))
                {
                    mo.InvokeMethod("Enable", null);
                    break;
                }
            }
        }
    }
}
