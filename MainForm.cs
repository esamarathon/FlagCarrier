using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Forms;

using NdefLibrary.Ndef;

namespace FlagCarrierWin
{
	public partial class MainForm : Form
	{
		private static readonly string DISPLAY_NAME = "display_name";
		private static readonly string COUNTRY_CODE = "country_code";
		private static readonly string SRCOM_NAME = "speedruncom_name";
		private static readonly string TWITCH_NAME = "twitch_name";
		private static readonly string TWITTER_HANDLE = "twitter_handle";
		private static readonly Dictionary<string, string> KV_DISPLAY_VALUES = new Dictionary<string, string>
		{
			{ DISPLAY_NAME, "Display Name" },
			{ COUNTRY_CODE, "Country Code" },
			{ SRCOM_NAME, "Speedrun.com Name" },
			{ TWITCH_NAME, "Twitch Name" },
			{ TWITTER_HANDLE, "Twitter Handle" }
		};

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

		private Dictionary<string, string> GetWriteData()
		{
			Dictionary<string, string> vals = new Dictionary<string, string>();

			if (displayNameBox.Text.Trim() == "" || countryCodeBox.Text.Trim() == "")
			{
				outTextBox.Text = "Display Name and Country Code are required!";
				return null;
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
					return null;
				}

				vals.Add(kv[0], kv[1]);
			}

			return vals;
		}

		private void WriteButton_Click(object sender, EventArgs args)
		{
			var vals = GetWriteData();
			if (vals == null)
				return;

			var msg = NdefHandler.GenerateNdefMessage(vals);
			nfcHandler.WriteNdefMessage(msg);

			outTextBox.Text = "Scan tag now!";
		}

		private void SendToLoginButton_Click(object sender, EventArgs e)
		{
			var vals = GetWriteData();
			if (vals == null)
				return;

			GotLoginData(vals);
			tabControl.SelectedTab = loginTagPage;
			outTextBox.Clear();
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
				nfcHandler.ReceiveNdefMessage += NfcHandler_ReceiveNdefMessage;
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

		private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void ApplySettingsButton_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.deviceID = deviceIdBox.Text;
			Properties.Settings.Default.groupID = groupIdBox.Text;
			Properties.Settings.Default.positionsAvail = positionsBox.Text;
			Properties.Settings.Default.targetUrl = targetUrlBox.Text;
			Properties.Settings.Default.Save();
			applySettingsButton.Enabled = false;
			SetupPositions();
		}

		private void ResetSettingsButton_Click(object sender, EventArgs e)
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
			SetupPositions();
		}

		private void WriteSettingsTagButton_Click(object sender, EventArgs e)
		{
			displayNameBox.Text = "set";
			countryCodeBox.Text = "XX";
			srcomNameBox.Text = "";
			twitchNameBox.Text = "";
			twitterHandleBox.Text = "";

			extraDataBox.Clear();
			extraDataBox.AppendText("set=pos_avail,group_id,target_url" + Environment.NewLine);
			extraDataBox.AppendText("pos_avail=" + Properties.Settings.Default.positionsAvail + Environment.NewLine);
			extraDataBox.AppendText("group_id=" + Properties.Settings.Default.groupID + Environment.NewLine);
			extraDataBox.AppendText("target_url=" + Properties.Settings.Default.targetUrl + Environment.NewLine);

			tabControl.SelectedTab = writeTabPage;
		}

		private void SettingTextChanged(object sender, EventArgs e)
		{
			applySettingsButton.Enabled = true;
		}

		private void SetupPositions()
		{
			positionSelectBox.Items.Clear();
			foreach(string position in Properties.Settings.Default.positionsAvail.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				positionSelectBox.Items.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(position));
			}

			positionSelectBox.SelectedIndex = 0;
		}

		private void NfcHandler_ReceiveNdefMessage(NdefMessage msg)
		{
			try
			{
				var res = NdefHandler.ParseNdefMessage(msg);
				Invoke(new Action(() =>
				{
					GotLoginData(res);
				}));
			}
			catch(NdefHandlerException e)
			{
				AppendOutput("Failure parsing message: " + e.Message);
			}
		}

		private Dictionary<string, string> loginData;

		private void GotLoginData(Dictionary<string, string> data)
		{
			loginData = data;
			SetupLoginTab();
		}

		private void SetupLoginTab()
		{
			loginTextBox.Text = "";
			foreach (var knownKey in KV_DISPLAY_VALUES)
			{
				if (!loginData.ContainsKey(knownKey.Key))
					continue;
				loginTextBox.AppendText(knownKey.Value + ": " + loginData[knownKey.Key] + Environment.NewLine);
			}

			bool writtenHeader = false;
			foreach (var element in loginData)
			{
				if (KV_DISPLAY_VALUES.ContainsKey(element.Key))
					continue;
				if (!writtenHeader)
				{
					loginTextBox.AppendText(Environment.NewLine + "Extra data:" + Environment.NewLine);
					writtenHeader = true;
				}
				loginTextBox.AppendText("  " + element.Key + "=" + element.Value + Environment.NewLine);
			}

			tabControl.SelectedTab = loginTagPage;
		}

		private HttpHandler httpHandler = new HttpHandler();

		private async void LoginButton_Click(object sender, EventArgs args)
		{
			if (loginData == null)
			{
				loginTextBox.Text = "No data to login!";
				return;
			}

			loginTextBox.AppendText(Environment.NewLine + Environment.NewLine);

			Dictionary<string, string> extraData = new Dictionary<string, string>();
			extraData.Add("position", positionSelectBox.SelectedText.ToLower());

			try
			{
				string response = await httpHandler.DoRequestAsync("login", loginData, extraData);
				loginTextBox.AppendText("Login successful:" + Environment.NewLine);
				loginTextBox.AppendText(response);
				loginData = null;
			}
			catch(Exception e)
			{
				loginTextBox.AppendText("Request failed:" + Environment.NewLine);
				loginTextBox.AppendText(e.Message);
			}
		}

		private async void ClearButton_Click(object sender, EventArgs args)
		{
			var confirmResult = MessageBox.Show("This will clear the currently logged in users. Are you sure?", "Clear?", MessageBoxButtons.YesNo);
			if (confirmResult != DialogResult.Yes)
				return;

			loginData = null;
			loginTextBox.Text = "";

			try
			{
				string response = await httpHandler.DoRequestAsync("clear");
				loginTextBox.AppendText("Clear successful:" + Environment.NewLine);
				loginTextBox.AppendText(response);
			}
			catch (Exception e)
			{
				loginTextBox.AppendText("Request failed:" + Environment.NewLine);
				loginTextBox.AppendText(e.Message);
			}
		}
	}
}
