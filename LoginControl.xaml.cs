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
        }

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{

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
