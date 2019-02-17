using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using FlagCarrierBase;

namespace FlagCarrierWin
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
		HttpHandler httpHandler;
		Dictionary<string, string> loginData;

        public LoginControl()
        {
            InitializeComponent();

			httpHandler = new HttpHandler();

			UpdateAvailablePositions();
		}

		public void SettingsChanged()
		{
			UpdateAvailablePositions();
		}

		public void UpdateAvailablePositions()
		{
			string[] positions =
				Properties.Settings.Default.positionsAvail.Split(
					new[] { ',' },
					StringSplitOptions.RemoveEmptyEntries);

			positionComboBox.Items.Clear();

			foreach (string pos in positions)
			{
				var item = new ComboBoxItem
				{
					Content = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pos.Trim())
				};
				positionComboBox.Items.Add(item);
			}

			positionComboBox.SelectedIndex = 0;
		}

		private async void LoginButton_Click(object sender, RoutedEventArgs args)
		{
			if (loginData == null)
			{
				loginDataBox.Text = "No data to login with!";
				return;
			}
			if (positionComboBox.SelectedItem == null)
			{

				loginDataBox.AppendText("No position selected!" + Environment.NewLine);
				return;
			}

			loginDataBox.AppendText(Environment.NewLine + Environment.NewLine);

			Dictionary<string, string> extraData = new Dictionary<string, string>
			{
				{ "position", ((ComboBoxItem)positionComboBox.SelectedItem).Content.ToString().ToLower() }
			};

			try
			{
				string targetUrl = Properties.Settings.Default.targetUrl;
				string deviceId = Properties.Settings.Default.deviceID;
				string groupId = Properties.Settings.Default.groupID;

				string response = await httpHandler.DoRequestAsync(targetUrl, deviceId, groupId, "login", loginData, extraData);
				loginDataBox.AppendText("Login successful:" + Environment.NewLine);
				loginDataBox.AppendText(response);
				loginData = null;
			}
			catch (Exception e)
			{
				loginDataBox.AppendText("Request failed:" + Environment.NewLine);
				loginDataBox.AppendText(e.Message);
			}
		}

		private async void ClearButton_Click(object sender, RoutedEventArgs args)
		{
			var msgBoxRes = MessageBox.Show(
				"This will clear the currently logged in users. Are you sure?",
				"Clear?", MessageBoxButton.YesNo);
			if (msgBoxRes != MessageBoxResult.Yes)
				return;

			loginData = null;
			loginDataBox.Clear();

			try
			{
				string targetUrl = Properties.Settings.Default.targetUrl;
				string deviceId = Properties.Settings.Default.deviceID;
				string groupId = Properties.Settings.Default.groupID;

				string response = await httpHandler.DoRequestAsync(targetUrl, deviceId, groupId, "clear");
				loginDataBox.AppendText("Clear successful:" + Environment.NewLine);
				loginDataBox.AppendText(response);
			}
			catch (Exception e)
			{
				loginDataBox.AppendText("Request failed:" + Environment.NewLine);
				loginDataBox.AppendText(e.Message);
			}
		}

		internal void NewData(Dictionary<string, string> data)
		{
			loginData = data;
			ShowLoginData();

			if (loginData.ContainsKey("sig_valid"))
			{
				bool sigValid = Convert.ToBoolean(loginData["sig_valid"]);

				loginButton.IsEnabled = sigValid;

				if (sigValid)
					loginDataBox.AppendText(Environment.NewLine + "Valid signature!" + Environment.NewLine);
				else
					loginDataBox.AppendText(Environment.NewLine + "INVALID signature!" + Environment.NewLine);
			}
			else
			{
				loginButton.IsEnabled = NdefHandler.HasPrivKey() || !NdefHandler.HasPubKey();
			}
		}

		private void ShowLoginData()
		{
			loginDataBox.Clear();
			foreach (var knownKey in Definitions.KV_DISPLAY_VALUES)
			{
				if (!loginData.ContainsKey(knownKey.Key))
					continue;
				loginDataBox.AppendText(knownKey.Value + ": " + loginData[knownKey.Key] + Environment.NewLine);
			}

			bool writtenHeader = false;
			foreach (var element in loginData)
			{
				if (Definitions.KV_DISPLAY_VALUES.ContainsKey(element.Key))
					continue;
				if (element.Key == "sig" || element.Key == "sig_valid")
					continue;
				if (!writtenHeader)
				{
					loginDataBox.AppendText(Environment.NewLine + "Extra data:" + Environment.NewLine);
					writtenHeader = true;
				}
				loginDataBox.AppendText("  " + element.Key + "=" + element.Value + Environment.NewLine);
			}
		}
	}
}
