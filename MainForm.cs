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
using Windows.Storage.Streams;

namespace FlagCarrierWin
{
	public partial class MainForm : Form
	{
		private NfcHandler nfcHandler;

		public MainForm()
		{
			InitializeComponent();
		}

		private void writeButton_Click(object sender, EventArgs args)
		{
			Dictionary<string, string> vals = new Dictionary<string, string>();

			vals.Add("display_name", "Test");
			vals.Add("country_code", "US");

			var msg = NdefHandler.GenerateNdefMessage(vals);
			nfcHandler.WriteNdefMessage(msg);

			outTextBox.Text = "Scan tag now!";
		}

		private async void MainForm_Load(object sender, EventArgs args)
		{
			try
			{
				var readers = await NfcHandler.GetAvailableReaders();
				if(readers.Count < 1)
				{
					readerNameLabel.Text = "No reader found";
					return;
				}

				var reader = readers.First();
				readerNameLabel.Text = reader.Name;

				nfcHandler = await NfcHandler.GetFromDevInfoAsync(reader);
				nfcHandler.StatusMessage += AppendOutput;
				nfcHandler.ErrorMessage += AppendOutput;
				nfcHandler.CardAdded += ClearOutput;
			}
			catch(NfcHandlerException e)
			{
				readerNameLabel.Text = "Error: " + e.Message;
			}
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
				outTextBox.AppendText(text + Environment.NewLine);
				outTextBox.SelectionStart = outTextBox.Text.Length;
				outTextBox.ScrollToCaret();
			}));
		}
	}
}
