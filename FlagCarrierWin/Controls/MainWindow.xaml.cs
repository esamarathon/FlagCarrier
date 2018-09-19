using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;

using FlagCarrierBase;
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

			settingsControl.WriteToTagRequest += SettingsControl_WriteToTagRequest;
			settingsControl.UpdatedSettings += loginControl.SettingsChanged;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			nfcHandler.StartMonitoring();
			statusTextBlock.Text = "Monitoring all readers";

			LoadCert();
		}

		private void LoadCert()
		{
			string appLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;
			string appPath = Path.GetDirectoryName(appLoc);
			string appName = Path.GetFileNameWithoutExtension(appLoc);
			string certPath = Path.Combine(appPath, appName + ".pfx");

			if (!File.Exists(certPath))
				return;

			try
			{
				NdefHandler.LoadCert(certPath);
			}
			catch (CryptographicException)
			{
				try
				{
					Controls.PasswordDialog pwDialog = new Controls.PasswordDialog();
					pwDialog.Owner = this;
					pwDialog.ShowDialog();

					if (pwDialog.Password == null)
						return;

					NdefHandler.LoadCert(certPath, pwDialog.Password);
				}
				catch (CryptographicException e)
				{
					MessageBox.Show(this, "Certificate could not be loaded:\n" + e.Message, "Error");
				}
			}
		}

		private void SettingsControl_WriteToTagRequest(Dictionary<string, string> settings)
		{
			writeControl.PrefillWithSettings(settings);
			writeTab.IsSelected = true;
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

		private void NfcHandler_CardAdded(string reader)
		{
			Dispatcher.Invoke(() => ClearOutput());
		}

		private void StatusMessage(string msg)
		{
			Dispatcher.Invoke(() => AppendOutput(msg));
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
	}
}
