using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Windows.Networking.Proximity;
using Windows.Storage.Streams;

namespace FlagCarrierWin
{
    public partial class MainForm : Form
    {
        private ProximityDevice device;

        public MainForm()
        {
            InitializeComponent();

            device = ProximityDevice.GetDefault();

            if(device == null)
            {
                textLabel.Text = "Failed to get proximity device!";
            } else {
                device.DeviceArrived += DeviceArrived;
                device.DeviceDeparted += DeviceDeparted;
                textLabel.Text = "Ready";
            }
        }

        private void DeviceDeparted(ProximityDevice sender)
        {
            Console.WriteLine("Device Departed");
            Invoke(new Action(
            () => {
                textLabel.Text = "Device Departed";
            }));
        }

        private void DeviceArrived(ProximityDevice sender)
        {
            Console.WriteLine("Device Arrived");
            Invoke(new Action(
            () => {
                textLabel.Text = "Device Arrived";
            }));
        }
    }
}
