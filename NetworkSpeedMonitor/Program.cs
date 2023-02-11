using System;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Net.NetworkInformation;
using System.Configuration;
using System.Resources;
using System.Drawing;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NetworkSpeedMonitor
{

    class Program
    {

        static NotifyIcon trayIcon;
        static ContextMenuStrip trayMenu;       
        static ResourceManager rm = new ResourceManager("NetworkSpeedMonitor.Properties.Resources", typeof(Program).Assembly);
        static string prevBallonTip = "";

        private static string NetworkName;
        private const string NetworkName_KeyName = @"HKEY_CURRENT_USER\Software\NetworkSpeedMonitor";
        private const string NetworkName_ValueName = "NetworkName";
        private const string NetworkName_DefaultValue = null;

        private static void SaveConfig()
        {
            Registry.SetValue(NetworkName_KeyName, NetworkName_ValueName, NetworkName);
        }

        private static void LoadConfig()
        {
            NetworkName = (string)Registry.GetValue(NetworkName_KeyName, NetworkName_ValueName, NetworkName_DefaultValue);
        }

        [STAThread]        
        static void Main()
        {                       

            trayIcon = new NotifyIcon();
            trayIcon.Icon = (Icon)rm.GetObject("NetworkSpeedMonitor");
            trayIcon.Visible = true;
            trayIcon.Text = "";

            LoadConfig();
            ContextMenu();

            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

            UpdateNetworkSpeed();            
            Application.Run();
        }

        private static void ContextMenu()
        {
            trayMenu = new ContextMenuStrip();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                trayMenu.Items.Add(ni.Description, null, OnNetwork);

            trayMenu.Items.Add("-");

            if (NetworkName != null)
                trayMenu.Items.Add("Reset Network Device", (Image)Properties.Resources.ResourceManager.GetObject("ResetNetwork"), OnResetNetwork);

            trayMenu.Items.Add("Exit", (Image)Properties.Resources.ResourceManager.GetObject("Exit"), OnExit);

            trayIcon.ContextMenuStrip = trayMenu;
        }

        private static void OnNetwork(object sender, EventArgs e)
        {
            NetworkName = sender.ToString();
            SaveConfig();
            UpdateNetworkSpeed();
            ContextMenu();
        }

        private static void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        private static void OnResetNetwork(object sender, EventArgs e)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.Description == NetworkName)
                {
                    Process.Start(Path.Combine(Application.StartupPath, "ResetNetworkDevice.exe"));
                }
            }
        }

        private static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            UpdateNetworkSpeed();
        }

        private static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            UpdateNetworkSpeed();
        }

        private static void UpdateMenuItems()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (NetworkName is null)
                {
                    return;
                }
                else
                {
                    int idx = 0;
                    foreach (var menuitem in trayMenu.Items)
                    {
                        if (menuitem.ToString() == "Exit" || menuitem.ToString() == "Reset Network Device")
                            continue;

                        if (menuitem.ToString() == NetworkName)
                            trayMenu.Items[idx].Image = (Image)Properties.Resources.ResourceManager.GetObject("Tick");
                        else
                            trayMenu.Items[idx].Image = null;
                        
                        idx++;

                    }
                }

            }
        }

        private static void UpdateNetworkSpeed()
        {
            UpdateMenuItems();

            var maxSpeed = 0L;
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.Description != NetworkName)
                    continue;

                var speed = ni.Speed;
                if (speed > maxSpeed)
                {
                    maxSpeed = speed;
                }
            }

            if (NetworkName is null)
            {
                trayIcon.Icon = (Icon)rm.GetObject("NetworkSpeedMonitor");
                trayIcon.Text = NetworkName + "Select network device";
            }

            else
            {

                if (maxSpeed == 0)
                {
                    trayIcon.Icon = (Icon)rm.GetObject("iconDisconnected");
                    trayIcon.Text = NetworkName + "\n\nDisconnected";
                }
                else if (maxSpeed < 100000000)
                {
                    trayIcon.Icon = (Icon)rm.GetObject("icon10Mbps");
                    trayIcon.Text = NetworkName + "\n\nMax speed: 10 Mbps";
                }
                else if (maxSpeed < 1000000000)
                {
                    trayIcon.Icon = (Icon)rm.GetObject("icon100Mbps");
                    trayIcon.Text = NetworkName + "\n\nMax speed: 100 Mbps";
                }
                else
                {
                    trayIcon.Icon = (Icon)rm.GetObject("icon1Gbps");
                    trayIcon.Text = NetworkName + "\n\nMax speed: 1 Gbps";
                }
            }

            if (prevBallonTip != trayIcon.Text)
            {
                trayIcon.ShowBalloonTip(5, "", trayIcon.Text, ToolTipIcon.Info);
                prevBallonTip = trayIcon.Text;
            }
        }
    }
}


