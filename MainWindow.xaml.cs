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

using NdefLibrary.Ndef;

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
			nfcHandler.StatusMessage += StatusMessage;
			nfcHandler.ErrorMessage += StatusMessage;
			nfcHandler.ReceiveNdefMessage += NfcHandler_ReceiveNdefMessage;

			writeControl.ManualLoginRequest += WriteControl_ManualLoginRequest;
			writeControl.WriteMessageRequest += WriteControl_WriteMessageRequest;
			writeControl.ErrorMessage += StatusMessage;
		}

		private void WriteControl_WriteMessageRequest(NdefMessage msg)
		{
			nfcHandler.WriteNdefMessage(msg);
			ClearOutput("Scan tag to write.");
		}

		private void WriteControl_ManualLoginRequest(Dictionary<string, string> data)
		{
			loginControl.NewData(data);
			loginTab.IsSelected = true;
		}

		private void NfcHandler_ReceiveNdefMessage(NdefMessage msg)
		{
			var data = NdefHandler.ParseNdefMessage(msg);
			Dispatcher.Invoke(() =>
			{
				loginControl.NewData(data);
				loginTab.IsSelected = true;
			});
		}

		private void StatusMessage(string msg)
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
			ClearOutput();
			nfcHandler.WriteNdefMessage(null);
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
