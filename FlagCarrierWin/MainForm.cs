using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Windows.Devices.SmartCards;

namespace FlagCarrierWin
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
		}

        private void writeButton_Click(object sender, EventArgs e)
        {
			
		}

		private async void MainForm_Load(object sender, EventArgs e)
		{
			SmartCardReader reader = await SmartCardUtils.FindNfcReaderAsync();
			if (reader == null)
			{
				readerNameLabel.Text = "No NFC reader found!";
				return;
			}

			readerNameLabel.Text = "Using " + reader.Name;

			reader.CardAdded += Reader_CardAdded;
			reader.CardRemoved += Reader_CardRemoved;
		}

		private void Reader_CardRemoved(SmartCardReader sender, CardRemovedEventArgs args)
		{
			Invoke(new Action(() =>
			{
				readerStatusLabel.Text = "Card removed";
			}));
		}

		private async void Reader_CardAdded(SmartCardReader sender, CardAddedEventArgs args)
		{
			Invoke(new Action(() =>
			{
				readerStatusLabel.Text = "Card found";
			}));

			await HandleCard(args.SmartCard);
		}

		private async Task HandleCard(SmartCard card)
		{
			ClearOutput();

			using (SmartCardConnection con = await card.ConnectAsync())
			{

			}
		}

		private void ClearOutput()
		{
			Invoke(new Action(() =>
			{
				outTextBox.Clear();
			}));
		}

		private void AppendOutput(string text)
		{
			Invoke(new Action(() =>
			{
				outTextBox.AppendText(text);
				outTextBox.SelectionStart = outTextBox.Text.Length;
				outTextBox.ScrollToCaret();
			}));
		}
	}
}
