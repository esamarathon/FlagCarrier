using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Windows.Devices.SmartCards;
using Pcsc;
using Pcsc.Common;
using MifareUltralight;
using Windows.Storage.Streams;

namespace FlagCarrierWin
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
		}

        private void writeButton_Click(object sender, EventArgs e)
        {
			
		}

		private async void MainForm_Load(object sender, EventArgs e)
		{
			SmartCardReader reader = await SmartCardUtils.FindNfcReaderAsync();
			if (reader == null)
			{
				readerNameLabel.Text = "No NFC reader found!";
				return;
			}

			readerNameLabel.Text = "Using " + reader.Name;

			reader.CardAdded += Reader_CardAdded;
			reader.CardRemoved += Reader_CardRemoved;
		}

		private void Reader_CardRemoved(SmartCardReader sender, CardRemovedEventArgs args)
		{
			Invoke(new Action(() =>
			{
				readerStatusLabel.Text = "Card removed";
			}));
		}

		private async void Reader_CardAdded(SmartCardReader sender, CardAddedEventArgs args)
		{
			Invoke(new Action(() =>
			{
				readerStatusLabel.Text = "Card found";
			}));

			await HandleCard(args.SmartCard);
		}

		private async Task HandleCard(SmartCard card)
		{
			ClearOutput();

			using (SmartCardConnection con = await card.ConnectAsync())
			{
				IccDetection cardIdent = new IccDetection(card, con);
				await cardIdent.DetectCardTypeAync();

				AppendOutput("Connected to card\r\n\r\n"
					+ "Device class: " + cardIdent.PcscDeviceClass.ToString() + "\r\n"
					+ "Card name: " + cardIdent.PcscCardName.ToString() + "\r\n"
					+ "ATR: " + BitConverter.ToString(cardIdent.Atr) + "\r\n\r\n");

				if (cardIdent.PcscDeviceClass != DeviceClass.StorageClass ||
					(cardIdent.PcscCardName != CardName.MifareUltralight
					&& cardIdent.PcscCardName != CardName.MifareUltralightC
					&& cardIdent.PcscCardName != CardName.MifareUltralightEV1))
				{
					AppendOutput("Card not supported");
					return;
				}

				byte[] cmd = new byte[4];
				cmd[0] = 0x30;
				cmd[1] = 3;
				cmd[2] = 0;
				cmd[3] = 0; // Calc CRC: https://stackoverflow.com/questions/202466/how-to-calculate-crc-b-in-c-sharp
				byte[] res = (await con.TransmitAsync(cmd.AsBuffer())).ToArray();
				AppendOutput("Got Test Resp: " + BitConverter.ToString(res) + "\r\n\r\n");

				return;

				//await HandleMifareUL(new MifareUltralight.AccessHandler(con));
			}
		}

		private async Task HandleMifareUL(MifareUltralight.AccessHandler mifare)
		{
			byte[] uid = await mifare.GetUidAsync();
			AppendOutput("UID: " + BitConverter.ToString(uid) + "\r\n");

			byte[] infoData = await mifare.ReadAsync(0);

			AppendOutput("CC: " + BitConverter.ToString(infoData.Skip(12).ToArray()) + "\r\n\r\n");

			byte identByte = infoData[12];
			byte identVersion = infoData[13];
			int identCapacity = infoData[14] * 8;

			if (identByte != 0xE1 || identVersion < 0x10)
			{
				AppendOutput("Unsupported tag");
				return;
			}

			AppendOutput("NDEF tag with " + identCapacity + " bytes capacity detected.\r\n\r\n");

			byte[] data = await DumpMifare(mifare, identCapacity);

			AppendOutput(BitConverter.ToString(data));
		}

		private async Task<byte[]> DumpMifare(AccessHandler mifare, int capacity)
		{
			byte[] res = new byte[capacity];

			for (byte pos = 4; capacity > 0; pos += 4, capacity -= 16)
			{
				try
				{
					Console.WriteLine("Reading at position " + pos);
					byte[] data = await mifare.ReadAsync(pos);
					if (capacity < 16)
						data = data.Take(capacity).ToArray();

					data.CopyTo(res, (pos - 4) * 4);
				} catch(Exception e)
				{
					Console.WriteLine("Failed reading at pos " + pos);
					pos -= 3;
					capacity += 12;
				}
			}
			
			return res;
		}

		private void ClearOutput()
		{
			Invoke(new Action(() =>
			{
				outTextBox.Clear();
			}));
		}

		private void AppendOutput(string text)
		{
			Invoke(new Action(() =>
			{
				outTextBox.AppendText(text);
				outTextBox.SelectionStart = outTextBox.Text.Length;
				outTextBox.ScrollToCaret();
			}));
		}
	}
}
