using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Networking.Proximity;
using Windows.Storage.Streams;


namespace FlagCarrierWin
{
	class ProximityApiHandler
	{
		private ProximityDevice device;
		private long subId;

		public ProximityApiHandler()
		{

			device = ProximityDevice.GetDefault();

			if (device == null)
			{
				Console.WriteLine("Failed to get proximity device!");
			}
			else
			{
				device.DeviceArrived += DeviceArrived;
				device.DeviceDeparted += DeviceDeparted;
				subId = device.SubscribeForMessage("NDEF", MessageReceived);
				Console.WriteLine("Ready");
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

			foreach (var pair in res)
			{
				Console.WriteLine(pair.Key + "=" + pair.Value);
			}
		}

		private bool publishing = false;

		private void writeButton_Click(object sender, EventArgs e)
		{
			if (publishing)
				return;

			Dictionary<string, string> data = new Dictionary<string, string>
			{
				{ "display_name", "Timo" },
				{ "country_code", "US" },
			};

			byte[] binMsg = NdefHandler.GenerateRawNdefMessage(data);

			DataWriter writer = new DataWriter();
			writer.WriteBytes(binMsg);

			device.StopSubscribingForMessage(subId);
			device.PublishBinaryMessage("NDEF:WriteTag", writer.DetachBuffer(), HandleTransmitted);
			publishing = true;
		}

		private void HandleTransmitted(ProximityDevice sender, long messageId)
		{
			device.StopPublishingMessage(messageId);
			subId = device.SubscribeForMessage("NDEF", MessageReceived);
			publishing = false;

			Console.WriteLine("Submitted Message!");
		}
	}
}
