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

namespace FlagCarrierWin
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
		public event Action<Dictionary<string, string>> WriteToTagRequest;
		public event Action UpdatedSettings;

        public SettingsControl()
        {
            InitializeComponent();

			ResetSettings();
			ApplyKeyPair();
		}

		private void TextChanged(object sender, TextChangedEventArgs e)
		{
			applyButton.IsEnabled = true;
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs args)
		{
			Properties.Settings.Default.deviceID = deviceIdBox.Text;
			Properties.Settings.Default.groupID = groupIdBox.Text;
			Properties.Settings.Default.positionsAvail = positionsBox.Text;
			Properties.Settings.Default.targetUrl = targetUrlBox.Text;
			Properties.Settings.Default.publicKey = pubKeyBox.Text;
			Properties.Settings.Default.privateKey = privKeyBox.Text;
			Properties.Settings.Default.Save();
			applyButton.IsEnabled = false;
			ApplyKeyPair();
			UpdatedSettings?.Invoke();
		}

		private void WriteToTagButton_Click(object sender, RoutedEventArgs args)
		{
			Dictionary<string, string> data = new Dictionary<string, string>
			{
				{ "pos_avail", positionsBox.Text },
				{ "group_id", groupIdBox.Text },
				{ "target_url", targetUrlBox.Text }
			};

			if (pubKeyBox.Text.Trim().Length != 0)
				data.Add("pub_key", pubKeyBox.Text);

			WriteToTagRequest?.Invoke(data);
		}

		private void ResetButton_Click(object sender, RoutedEventArgs args)
		{
			ResetSettings();
		}

		private void ResetSettings()
		{
			deviceIdBox.Text = Properties.Settings.Default.deviceID;
			groupIdBox.Text = Properties.Settings.Default.groupID;
			positionsBox.Text = Properties.Settings.Default.positionsAvail;
			targetUrlBox.Text = Properties.Settings.Default.targetUrl;
			pubKeyBox.Text = Properties.Settings.Default.publicKey;
			privKeyBox.Text = Properties.Settings.Default.privateKey;
			applyButton.IsEnabled = false;
		}

		private void GeneratePair_Click(object sender, RoutedEventArgs e)
		{
			var pair = FlagCarrierBase.CryptoHandler.GenKeyPair();
			pubKeyBox.Text = Convert.ToBase64String(pair.PublicKey);
			privKeyBox.Text = Convert.ToBase64String(pair.PrivateKey);
			applyButton.IsEnabled = true;
		}

		private void ApplyKeyPair()
		{
			try
			{
				byte[] pubKey = Convert.FromBase64String(Properties.Settings.Default.publicKey);
				byte[] privKey = Convert.FromBase64String(Properties.Settings.Default.privateKey);

				if (Properties.Settings.Default.publicKey.Trim().Length == 0)
					pubKey = null;
				if (Properties.Settings.Default.privateKey.Trim().Length == 0)
					privKey = null;

				FlagCarrierBase.NdefHandler.SetKeys(pubKey, privKey);
			} catch(FormatException)
			{
				MessageBox.Show("Invalid base64 in keypair!", "Error");
				return;
			}
		}
	}
}
