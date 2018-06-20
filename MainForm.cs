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

            if (device == null)
            {
                textLabel.Text = "Failed to get proximity device!";
            } else {
                device.DeviceArrived += DeviceArrived;
                device.DeviceDeparted += DeviceDeparted;
                device.SubscribeForMessage("NDEF", MessageReceived);
                textLabel.Text = "Ready";
            }
        }

        private void DeviceDeparted(ProximityDevice sender)
        {
            Console.WriteLine("Device Departed");
        }

        private void DeviceArrived(ProximityDevice sender)
        {
            Console.WriteLine("Device Arrived");
        }

		private static byte[] GetArrayFromMessage(ProximityMessage message)
        {
            using (var reader = DataReader.FromBuffer(message.Data))
            {
                byte[] data = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(data);
                return data;
            }
        }

        private void MessageReceived(ProximityDevice sender, ProximityMessage message)
		{
            Console.WriteLine("Got Message");

            byte[] data = GetArrayFromMessage(message);
            var res = NdefHandler.ParseNdefMessage(data);

            Invoke(new Action(
            () => {
                textLabel.Text = "";
                foreach(var pair in res)
                {
                    textLabel.Text += pair.Key + "=" + pair.Value;
                    textLabel.Text += Environment.NewLine;
                }
            }));
		}
    }
}
