using System;
using System.Collections.Generic;
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

using NdefLibrary.Ndef;

namespace FlagCarrierWin
{
    /// <summary>
    /// Interaction logic for WriteControl.xaml
    /// </summary>
    public partial class WriteControl : UserControl
    {
		public event Action<Dictionary<string, string>> ManualLoginRequest;
		public event Action<NdefMessage> WriteMessageRequest;
		public event Action<string> ErrorMessage;

        public WriteControl()
        {
            InitializeComponent();
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

		private Dictionary<string, string> GetWriteData()
		{
			Dictionary<string, string> vals = new Dictionary<string, string>();

			var dspName = displayNameBox.Text.Trim();
			var ctrCode = countryCodeBox.Text.Trim();
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
	}
}
