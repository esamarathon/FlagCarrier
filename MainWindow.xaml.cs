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
using System.Windows.Shapes;

namespace FlagCarrierWin
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		NfcHandler nfcHandler;

		public MainWindow()
		{
			InitializeComponent();

			nfcHandler = new NfcHandler();
			nfcHandler.CardAdded += NfcHandler_CardAdded;
			nfcHandler.StatusMessage += NfcHandler_StatusMessage;
			nfcHandler.ErrorMessage += NfcHandler_ErrorMessage;
			nfcHandler.ReceiveNdefMessage += NfcHandler_ReceiveNdefMessage;
		}

		private void NfcHandler_ReceiveNdefMessage(NdefLibrary.Ndef.NdefMessage msg)
		{
			var data = NdefHandler.ParseNdefMessage(msg);
			Dispatcher.Invoke(() =>
			{
				loginControl.NewData(data);
				loginTab.IsSelected = true;
			});
		}

		private void NfcHandler_ErrorMessage(string msg)
		{
			Dispatcher.Invoke(() => AppendOutput(msg));
		}

		private void NfcHandler_StatusMessage(string msg)
		{
			Dispatcher.Invoke(() => AppendOutput(msg));
		}

		private void NfcHandler_CardAdded(string reader)
		{
			Dispatcher.Invoke(() => ClearOutput());
		}

		private void ClearOutput(string msg = null)
		{
			outputTextBox.Clear();
			if (msg != null)
				outputTextBox.Text = msg + Environment.NewLine;
		}

		private void AppendOutput(string msg)
		{
			outputTextBox.AppendText(msg + Environment.NewLine);
			outputTextBox.ScrollToEnd();
		}

		private void ExitItem_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void WriteTab_Unselected(object sender, RoutedEventArgs e)
		{
			writeControl.CancelWriting();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (nfcHandler != null)
			{
				nfcHandler.Dispose();
				nfcHandler = null;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			nfcHandler.StartMonitoring();
			statusTextBlock.Text = "Monitoring all readers";
		}
	}
}
