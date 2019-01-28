using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Linq;

using FlagCarrierBase;
using NdefLibrary.Ndef;
using Newtonsoft.Json.Linq;

namespace FlagCarrierWin
{
    /// <summary>
    /// Interaction logic for WriteControl.xaml
    /// </summary>
    public partial class WriteControl : UserControl
    {
		private readonly HttpClient httpClient = new HttpClient();
		private TextBox lastChangedTextBox = null;

		public event Action<Dictionary<string, string>> ManualLoginRequest;
		public event Action<NdefMessage> WriteMessageRequest;
		public event Action<string> ErrorMessage;

        public WriteControl()
        {
            InitializeComponent();

			lastChangedTextBox = displayNameBox;

		}

		public void PrefillWithSettings(Dictionary<string, string> settings)
		{
			displayNameBox.Text = "set";
			countryCodeBox.Code = "DE";
			srcomNameBox.Text = "";
			twitchNameBox.Text = "";
			twitterHandleBox.Text = "";

			extraDataBox.Clear();
			extraDataBox.AppendText("set=" + String.Join(",", settings.Keys) + Environment.NewLine);
			foreach(var entry in settings)
				extraDataBox.AppendText(entry.Key + "=" + entry.Value + Environment.NewLine);
		}

		private void WriteButton_Click(object sender, RoutedEventArgs e)
		{
			var data = GetWriteData();
			if (data == null)
				return;

			var msg = NdefHandler.GenerateNdefMessage(data);

			WriteMessageRequest?.Invoke(msg);
		}

		private void SendToLoginButton_Click(object sender, RoutedEventArgs e)
		{
			var data = GetWriteData();
			if (data == null)
				return;

			ManualLoginRequest?.Invoke(data);
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			displayNameBox.Text = "";
			countryCodeBox.Code = "DE";
			srcomNameBox.Text = "";
			twitchNameBox.Text = "";
			twitterHandleBox.Text = "";
			extraDataBox.Clear();
		}

		private async void FromSrComButton_Click(object sender, RoutedEventArgs e)
		{
			string lookup_name = lastChangedTextBox.Text.Trim();

			if (lookup_name == "")
				return;

			HttpResponseMessage response = await httpClient.GetAsync("https://www.speedrun.com/api/v1/users?lookup=" + HttpUtility.UrlEncode(lookup_name));

			if (response.StatusCode != HttpStatusCode.OK)
			{
				ErrorMessage?.Invoke("sr.com request failed: " + response.StatusCode.ToString());
				return;
			}

			string data = await response.Content.ReadAsStringAsync();

			try
			{
				var srdata = JObject.Parse(data);
				var userdata = srdata["data"];

				if (userdata.Count() <= 0)
				{
					ErrorMessage?.Invoke("Not found on speedrun.com: " + lookup_name);
					return;
				}

				if (userdata.Count() != 1)
				{
					ErrorMessage?.Invoke("Multiple results for \"" + lookup_name + "\", using first one.");
				}

				userdata = userdata[0];

				try
				{
					displayNameBox.Text = (string)userdata["names"]["international"];
				}
				catch (Exception)
				{
					displayNameBox.Text = "";
				}

				try
				{
					string country = (string)userdata["location"]["country"]["code"];
					countryCodeBox.Code = country.Substring(0, 2);
				}
				catch (Exception)
				{
					countryCodeBox.Code = "de";
				}

				try
				{
					string srname = (string)userdata["weblink"];
					srcomNameBox.Text = srname.Split('/').Last();
				}
				catch (Exception)
				{
					srcomNameBox.Text = "";
				}

				try
				{
					string twitch = (string)userdata["twitch"]["uri"];
					twitchNameBox.Text = twitch.Split('/').Last();
				}
				catch (Exception)
				{
					twitchNameBox.Text = "";
				}

				try
				{
					string twitter = (string)userdata["twitter"]["uri"];
					twitterHandleBox.Text = twitter.Split('/').Last();
				}
				catch (Exception)
				{
					twitterHandleBox.Text = "";
				}
			}
			catch(Exception ex)
			{
				ErrorMessage?.Invoke("Failed parsing sr.com data:\n" + ex.Message);
			}
		}

		private Dictionary<string, string> GetWriteData()
		{
			Dictionary<string, string> vals = new Dictionary<string, string>();

			var dspName = displayNameBox.Text.Trim();
			var ctrCode = countryCodeBox.Code;
			if (dspName == "" || ctrCode == "")
			{
				ErrorMessage?.Invoke("Display Name and Country Code are required!");
				return null;
			}

			vals.Add(Definitions.DISPLAY_NAME, dspName);
			vals.Add(Definitions.COUNTRY_CODE, ctrCode);

			var txt = srcomNameBox.Text.Trim();
			if (txt != "")
				vals.Add(Definitions.SRCOM_NAME, txt);

			txt = twitchNameBox.Text.Trim();
			if (txt != "")
				vals.Add(Definitions.TWITCH_NAME, txt);

			txt = twitterHandleBox.Text.Trim();
			if (txt != "")
				vals.Add(Definitions.TWITTER_HANDLE, txt);

			foreach (String line in extraDataBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] kv = line.Split(new[] { '=' }, 2, StringSplitOptions.None);

				if (kv.Length != 2)
				{
					ErrorMessage?.Invoke("Invalid extra data!");
					return null;
				}

				vals.Add(kv[0], kv[1]);
			}

			return vals;
		}

		private void AnyBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox box = sender as TextBox;
			if (box == null)
				return;
			lastChangedTextBox = box;
		}
	}
}
