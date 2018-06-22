namespace FlagCarrierWin
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.writeButton = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.readerNameLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.spacerLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.readerStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.outTextBox = new System.Windows.Forms.RichTextBox();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// writeButton
			// 
			this.writeButton.Location = new System.Drawing.Point(19, 13);
			this.writeButton.Name = "writeButton";
			this.writeButton.Size = new System.Drawing.Size(75, 23);
			this.writeButton.TabIndex = 1;
			this.writeButton.Text = "Write";
			this.writeButton.UseVisualStyleBackColor = true;
			this.writeButton.Click += new System.EventHandler(this.writeButton_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.readerNameLabel,
			this.spacerLabel,
			this.readerStatusLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 428);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(800, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip";
			// 
			// readerNameLabel
			// 
			this.readerNameLabel.Name = "readerNameLabel";
			this.readerNameLabel.Size = new System.Drawing.Size(61, 17);
			this.readerNameLabel.Text = "Initializing";
			// 
			// spacerLabel
			// 
			this.spacerLabel.Name = "spacerLabel";
			this.spacerLabel.Size = new System.Drawing.Size(666, 17);
			this.spacerLabel.Spring = true;
			// 
			// readerStatusLabel
			// 
			this.readerStatusLabel.Name = "readerStatusLabel";
			this.readerStatusLabel.Size = new System.Drawing.Size(58, 17);
			this.readerStatusLabel.Text = "No Status";
			// 
			// outTextBox
			// 
			this.outTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.outTextBox.Location = new System.Drawing.Point(19, 43);
			this.outTextBox.Name = "outTextBox";
			this.outTextBox.ReadOnly = true;
			this.outTextBox.Size = new System.Drawing.Size(769, 382);
			this.outTextBox.TabIndex = 3;
			this.outTextBox.Text = "";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.outTextBox);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.writeButton);
			this.Name = "MainForm";
			this.Text = "FlagCarrier for Windows";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button writeButton;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel readerNameLabel;
		private System.Windows.Forms.ToolStripStatusLabel readerStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel spacerLabel;
		private System.Windows.Forms.RichTextBox outTextBox;
	}
}

