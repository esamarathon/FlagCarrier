using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

using FlagCarrierBase;
using NdefLibrary.Ndef;

namespace FlagCarrierWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, string> dataToWrite;
        NfcHandler nfcHandler;

        NdefHandler NdefHandler { get; set; } = new NdefHandler();

        public MainWindow()
        {
            InitializeComponent();

            RoutedCommand jumpToSettings = new RoutedCommand();
            jumpToSettings.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(jumpToSettings, JumpToSettings));

            loginControl.NdefHandler = NdefHandler;
            settingsControl.NdefHandler = NdefHandler;

            nfcHandler = new NfcHandler();
            nfcHandler.CardAdded += NfcHandler_CardAdded;
            nfcHandler.CardRemoved += NfcHandler_CardRemoved;
            nfcHandler.StatusMessage += StatusMessage;
            nfcHandler.ErrorMessage += StatusMessage;
            nfcHandler.ReceiveNdefMessage += NfcHandler_ReceiveNdefMessage;
            nfcHandler.NewTagUid += NfcHandler_NewTagUid;

            writeControl.ManualLoginRequest += WriteControl_ManualLoginRequest;
            writeControl.WriteDataRequest += WriteControl_WriteDataRequest;
            writeControl.ErrorMessage += StatusMessage;

            settingsControl.WriteToTagRequest += SettingsControl_WriteToTagRequest;
            settingsControl.UpdatedSettings += loginControl.SettingsChanged;
            settingsControl.UpdatedSettings += UpdatedSettings;

            UpdatedSettings();
        }

        private void NfcHandler_CardRemoved(string name)
        {
            NdefHandler.ClearExtraSignData();
        }

        private void NfcHandler_NewTagUid(byte[] uid)
        {
            NdefHandler.SetExtraSignData(uid);

            if (dataToWrite != null) {
                var msg = NdefHandler.GenerateNdefMessage(dataToWrite);
                nfcHandler.WriteNdefMessage(msg);
                dataToWrite = null;
            }
        }

        private void JumpToSettings(object sender, ExecutedRoutedEventArgs e)
        {
            mainTabControl.SelectedItem = settingsTab;
        }

        private void UpdatedSettings()
        {
            settingsTab.Visibility = Properties.Settings.Default.hideSettings
                ? Visibility.Hidden
                : Visibility.Visible;
            writeTab.Visibility = Properties.Settings.Default.hideWrite
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                nfcHandler.StartMonitoring();
                statusTextBlock.Text = "Monitoring all readers";
            }
            catch (Exception ex)
            {
                ClearOutput(ex.Message);
            }
        }

        private void SettingsControl_WriteToTagRequest(Dictionary<string, string> settings)
        {
            writeControl.PrefillWithSettings(settings);
            writeTab.IsSelected = true;
        }

        private void WriteControl_WriteDataRequest(Dictionary<string, string> msg)
        {
            dataToWrite = msg;
            if (msg != null)
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
            NdefHandler.ClearExtraSignData();
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
