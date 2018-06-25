using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
			Properties.Settings.Default.PropertyChanged += SettingsChanged;
		}

		private void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
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
				positionComboBox.Items.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pos.Trim()));

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

			Dictionary<string, string> extraData = new Dictionary<string, string>();
			extraData.Add("position", positionComboBox.SelectedItem.ToString().ToLower());

			try
			{
				string response = await httpHandler.DoRequestAsync("login", loginData, extraData);
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
				string response = await httpHandler.DoRequestAsync("clear");
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
