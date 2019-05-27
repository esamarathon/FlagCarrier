using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Linq;

using FlagCarrierBase;
using FlagCarrierBase.Helpers;
using NdefLibrary.Ndef;
using Newtonsoft.Json.Linq;

namespace FlagCarrierWin
{
    /// <summary>
    /// Interaction logic for WriteControl.xaml
    /// </summary>
    public partial class WriteControl : UserControl
    {
        private TextBox lastChangedTextBox = null;

        public event Action<Dictionary<string, string>> ManualLoginRequest;
        public event Action<Dictionary<string, string>> WriteDataRequest;
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
            extraDataBox.AppendText("set=" + string.Join(",", settings.Keys) + Environment.NewLine);
            foreach(var entry in settings)
                extraDataBox.AppendText(entry.Key + "=" + entry.Value + Environment.NewLine);
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            var data = GetWriteData();
            if (data == null)
                return;

            WriteDataRequest?.Invoke(data);
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

            try
            {
                var srData = await SpeedrunComHelper.GetUserInfo(lookup_name);

                displayNameBox.Text = srData.DisplayName;
                countryCodeBox.Code = srData.CountryCode;
                srcomNameBox.Text = srData.SrComName;
                twitchNameBox.Text = srData.TwitchName;
                twitterHandleBox.Text = srData.TwitterHandle;
            }
            catch(Exception ex)
            {
                ErrorMessage?.Invoke(ex.Message);
                return;
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

            foreach (string line in extraDataBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] kv = line.Split(new[] { '=' }, 2, StringSplitOptions.None);

                if (kv.Length != 2 || kv[0] == "sig_valid")
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
            var box = sender as TextBox;
            if (box == null)
                return;
            lastChangedTextBox = box;
        }
    }
}
