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
			Properties.Settings.Default.Save();
			applyButton.IsEnabled = false;
			UpdatedSettings?.Invoke();
		}

		private void WriteToTagButton_Click(object sender, RoutedEventArgs args)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();

			data.Add("pos_avail", positionsBox.Text);
			data.Add("group_id", groupIdBox.Text);
			data.Add("target_url", targetUrlBox.Text);

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
			applyButton.IsEnabled = false;
		}
	}
}
