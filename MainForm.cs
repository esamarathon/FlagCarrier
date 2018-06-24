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

using PcscSdk;
using PcscSdk.Common;

namespace FlagCarrierWin
{
	public partial class MainForm : Form
	{
		private static readonly string DISPLAY_NAME = "display_name";
		private static readonly string COUNTRY_CODE = "country_code";
		private static readonly string SRCOM_NAME = "speedruncom_name";
		private static readonly string TWITCH_NAME = "twitch_name";
		private static readonly string TWITTER_HANDLE = "twitter_handle";

		private NfcHandler nfcHandler;

		public MainForm()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				if(nfcHandler != null)
					nfcHandler.Dispose();
			}
			base.Dispose(disposing);
		}

		private void writeButton_Click(object sender, EventArgs args)
		{
			Dictionary<string, string> vals = new Dictionary<string, string>();

			if (displayNameBox.Text.Trim() == "" || countryCodeBox.Text.Trim() == "")
			{
				outTextBox.Text = "Display Name and Country Code are required!";
				return;
			}

			vals.Add(DISPLAY_NAME, displayNameBox.Text.Trim());
			vals.Add(COUNTRY_CODE, countryCodeBox.Text.Trim());
			vals.Add(SRCOM_NAME, srcomNameBox.Text.Trim());
			vals.Add(TWITCH_NAME, twitchNameBox.Text.Trim());
			vals.Add(TWITTER_HANDLE, twitterHandleBox.Text.Trim());

			foreach (String line in extraDataBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] kv = line.Split(new[] { '=' }, 2, StringSplitOptions.None);

				if (kv.Length != 2)
				{
					outTextBox.Text = "Invalid extra data!";
					return;
				}

				vals.Add(kv[0], kv[1]);
			}

			var msg = NdefHandler.GenerateNdefMessage(vals);
			nfcHandler.WriteNdefMessage(msg);

			outTextBox.Text = "Scan tag now!";
		}

		private void MainForm_Load(object sender, EventArgs args)
		{
			try
			{
				nfcHandler = new NfcHandler();
				nfcHandler.StatusMessage += AppendOutput;
				nfcHandler.ErrorMessage += AppendOutput;
				nfcHandler.CardAdded += CardAdded;
				nfcHandler.CardRemoved += CardRemoved;
				nfcHandler.StartMonitoring();
				readerNameLabel.Text = "Monitoring all readers";
			}
			catch(NfcHandlerException e)
			{
				readerNameLabel.Text = "Error: " + e.Message;
			}

			ResetSettings();
		}

		private void CardRemoved()
		{
			readerStatusLabel.Text = "Tag removed";
		}

		private void CardAdded()
		{
			ClearOutput();
			readerStatusLabel.Text = "Tag added";
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

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void applySettingsButton_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.deviceID = deviceIdBox.Text;
			Properties.Settings.Default.groupID = groupIdBox.Text;
			Properties.Settings.Default.positionsAvail = positionsBox.Text;
			Properties.Settings.Default.targetUrl = targetUrlBox.Text;
			Properties.Settings.Default.Save();
			applySettingsButton.Enabled = false;
		}

		private void resetSettingsButton_Click(object sender, EventArgs e)
		{
			ResetSettings();
		}

		private void ResetSettings()
		{
			deviceIdBox.Text = Properties.Settings.Default.deviceID;
			groupIdBox.Text = Properties.Settings.Default.groupID;
			positionsBox.Text = Properties.Settings.Default.positionsAvail;
			targetUrlBox.Text = Properties.Settings.Default.targetUrl;
			applySettingsButton.Enabled = false;
		}

		private void SettingTextChanged(object sender, EventArgs e)
		{
			applySettingsButton.Enabled = true;
		}
	}
}
