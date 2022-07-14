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
using System.Collections.Specialized;

namespace FlagCarrierWin
{
    /// <summary>
    /// Interaction logic for WriteControl.xaml
    /// </summary>
    public partial class WriteControl : UserControl
    {
        public event Action<Dictionary<string, string>> ManualLoginRequest;
        public event Action<Dictionary<string, string>> WriteDataRequest;
        public event Action<string> ErrorMessage;

        public WriteControl()
        {
            InitializeComponent();
        }

        public void SetWriteQuery(NameValueCollection dict)
        {
            clearButton.Visibility = Visibility.Hidden;
            sendToLoginButton.Visibility = Visibility.Hidden;

            displayNameBox.IsEnabled = false;
            userIdBox.IsEnabled = false;
            extraDataBox.IsEnabled = false;

            ClearButton_Click(null, null);

            foreach (string key in dict.Keys)
            {
                if (key == Definitions.DISPLAY_NAME)
                {
                    displayNameBox.Text = dict.Get(key);
                }
                else if (key == Definitions.USER_ID)
                {
                    userIdBox.Text = dict.Get(key);
                }
                else
                {
                    extraDataBox.Text += $"{key}={dict.Get(key)}\n";
                }
            }

            Application.Current.Dispatcher.Invoke(() => WriteButton_Click(null, null));
        }

        public void PrefillWithSettings(Dictionary<string, string> settings)
        {
            displayNameBox.Text = "set";
            userIdBox.Text = "";

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
            userIdBox.Text = "";
            extraDataBox.Clear();
        }

        private Dictionary<string, string> GetWriteData()
        {
            Dictionary<string, string> vals = new Dictionary<string, string>();

            var dspName = displayNameBox.Text.Trim();
            var userId = userIdBox.Text.Trim();
            if (dspName == "" || userId == "")
            {
                ErrorMessage?.Invoke("Display Name and User ID are required!");
                return null;
            }

            vals.Add(Definitions.DISPLAY_NAME, dspName);
            vals.Add(Definitions.USER_ID, userId);

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
    }
}
