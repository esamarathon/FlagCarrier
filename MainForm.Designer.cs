namespace FlagCarrierWin
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.writeButton = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.readerNameLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.spacerLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.readerStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.outTextBox = new System.Windows.Forms.RichTextBox();
			this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.loginTagPage = new System.Windows.Forms.TabPage();
			this.clearButton = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.loginButton = new System.Windows.Forms.Button();
			this.positionSelectBox = new System.Windows.Forms.ComboBox();
			this.loginTextBox = new System.Windows.Forms.RichTextBox();
			this.writeTabPage = new System.Windows.Forms.TabPage();
			this.extraDataBox = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.twitterHandleBox = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.twitchNameBox = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.srcomNameBox = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.countryCodeBox = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.displayNameBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.settingsTabPage = new System.Windows.Forms.TabPage();
			this.resetSettingsButton = new System.Windows.Forms.Button();
			this.groupIdBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.deviceIdBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.positionsBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.targetUrlBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.applySettingsButton = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.sendToLoginButton = new System.Windows.Forms.Button();
			this.statusStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.loginTagPage.SuspendLayout();
			this.writeTabPage.SuspendLayout();
			this.settingsTabPage.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// writeButton
			// 
			this.writeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.writeButton.Location = new System.Drawing.Point(8, 397);
			this.writeButton.Name = "writeButton";
			this.writeButton.Size = new System.Drawing.Size(75, 23);
			this.writeButton.TabIndex = 14;
			this.writeButton.Text = "Write";
			this.writeButton.UseVisualStyleBackColor = true;
			this.writeButton.Click += new System.EventHandler(this.WriteButton_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.readerNameLabel,
            this.spacerLabel,
            this.readerStatusLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 589);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(484, 22);
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
			this.spacerLabel.Size = new System.Drawing.Size(350, 17);
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
			this.outTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outTextBox.Location = new System.Drawing.Point(0, 0);
			this.outTextBox.Name = "outTextBox";
			this.outTextBox.ReadOnly = true;
			this.outTextBox.Size = new System.Drawing.Size(484, 112);
			this.outTextBox.TabIndex = 3;
			this.outTextBox.Text = "";
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = new System.Drawing.Point(0, 24);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.tabControl);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.outTextBox);
			this.mainSplitContainer.Size = new System.Drawing.Size(484, 565);
			this.mainSplitContainer.SplitterDistance = 449;
			this.mainSplitContainer.TabIndex = 4;
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.loginTagPage);
			this.tabControl.Controls.Add(this.writeTabPage);
			this.tabControl.Controls.Add(this.settingsTabPage);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(484, 449);
			this.tabControl.TabIndex = 2;
			// 
			// loginTagPage
			// 
			this.loginTagPage.Controls.Add(this.clearButton);
			this.loginTagPage.Controls.Add(this.label11);
			this.loginTagPage.Controls.Add(this.loginButton);
			this.loginTagPage.Controls.Add(this.positionSelectBox);
			this.loginTagPage.Controls.Add(this.loginTextBox);
			this.loginTagPage.Location = new System.Drawing.Point(4, 22);
			this.loginTagPage.Name = "loginTagPage";
			this.loginTagPage.Padding = new System.Windows.Forms.Padding(3);
			this.loginTagPage.Size = new System.Drawing.Size(476, 423);
			this.loginTagPage.TabIndex = 0;
			this.loginTagPage.Text = "Login";
			this.loginTagPage.UseVisualStyleBackColor = true;
			// 
			// clearButton
			// 
			this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.clearButton.Location = new System.Drawing.Point(393, 394);
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = new System.Drawing.Size(75, 23);
			this.clearButton.TabIndex = 4;
			this.clearButton.Text = "Clear";
			this.clearButton.UseVisualStyleBackColor = true;
			this.clearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// label11
			// 
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(8, 342);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(44, 13);
			this.label11.TabIndex = 1;
			this.label11.Text = "Position";
			// 
			// loginButton
			// 
			this.loginButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.loginButton.Location = new System.Drawing.Point(9, 394);
			this.loginButton.Name = "loginButton";
			this.loginButton.Size = new System.Drawing.Size(378, 23);
			this.loginButton.TabIndex = 3;
			this.loginButton.Text = "Login";
			this.loginButton.UseVisualStyleBackColor = true;
			this.loginButton.Click += new System.EventHandler(this.LoginButton_Click);
			// 
			// positionSelectBox
			// 
			this.positionSelectBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.positionSelectBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.positionSelectBox.FormattingEnabled = true;
			this.positionSelectBox.Location = new System.Drawing.Point(9, 358);
			this.positionSelectBox.Name = "positionSelectBox";
			this.positionSelectBox.Size = new System.Drawing.Size(459, 21);
			this.positionSelectBox.TabIndex = 2;
			// 
			// loginTextBox
			// 
			this.loginTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.loginTextBox.Location = new System.Drawing.Point(9, 7);
			this.loginTextBox.Name = "loginTextBox";
			this.loginTextBox.ReadOnly = true;
			this.loginTextBox.Size = new System.Drawing.Size(459, 320);
			this.loginTextBox.TabIndex = 0;
			this.loginTextBox.Text = "";
			// 
			// writeTabPage
			// 
			this.writeTabPage.Controls.Add(this.sendToLoginButton);
			this.writeTabPage.Controls.Add(this.extraDataBox);
			this.writeTabPage.Controls.Add(this.label10);
			this.writeTabPage.Controls.Add(this.twitterHandleBox);
			this.writeTabPage.Controls.Add(this.label9);
			this.writeTabPage.Controls.Add(this.twitchNameBox);
			this.writeTabPage.Controls.Add(this.label8);
			this.writeTabPage.Controls.Add(this.srcomNameBox);
			this.writeTabPage.Controls.Add(this.label7);
			this.writeTabPage.Controls.Add(this.countryCodeBox);
			this.writeTabPage.Controls.Add(this.label6);
			this.writeTabPage.Controls.Add(this.displayNameBox);
			this.writeTabPage.Controls.Add(this.label5);
			this.writeTabPage.Controls.Add(this.writeButton);
			this.writeTabPage.Location = new System.Drawing.Point(4, 22);
			this.writeTabPage.Name = "writeTabPage";
			this.writeTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.writeTabPage.Size = new System.Drawing.Size(476, 423);
			this.writeTabPage.TabIndex = 1;
			this.writeTabPage.Text = "Write";
			this.writeTabPage.UseVisualStyleBackColor = true;
			// 
			// extraDataBox
			// 
			this.extraDataBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.extraDataBox.Location = new System.Drawing.Point(10, 244);
			this.extraDataBox.Multiline = true;
			this.extraDataBox.Name = "extraDataBox";
			this.extraDataBox.Size = new System.Drawing.Size(458, 120);
			this.extraDataBox.TabIndex = 13;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(7, 228);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(155, 13);
			this.label10.TabIndex = 12;
			this.label10.Text = "Extra Data (Lines of key=value)";
			// 
			// twitterHandleBox
			// 
			this.twitterHandleBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.twitterHandleBox.Location = new System.Drawing.Point(10, 200);
			this.twitterHandleBox.Name = "twitterHandleBox";
			this.twitterHandleBox.Size = new System.Drawing.Size(458, 20);
			this.twitterHandleBox.TabIndex = 11;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(7, 184);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(76, 13);
			this.label9.TabIndex = 10;
			this.label9.Text = "Twitter Handle";
			// 
			// twitchNameBox
			// 
			this.twitchNameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.twitchNameBox.Location = new System.Drawing.Point(10, 156);
			this.twitchNameBox.Name = "twitchNameBox";
			this.twitchNameBox.Size = new System.Drawing.Size(458, 20);
			this.twitchNameBox.TabIndex = 9;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(7, 140);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(70, 13);
			this.label8.TabIndex = 8;
			this.label8.Text = "Twitch Name";
			// 
			// srcomNameBox
			// 
			this.srcomNameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.srcomNameBox.Location = new System.Drawing.Point(10, 112);
			this.srcomNameBox.Name = "srcomNameBox";
			this.srcomNameBox.Size = new System.Drawing.Size(458, 20);
			this.srcomNameBox.TabIndex = 7;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(7, 96);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(107, 13);
			this.label7.TabIndex = 6;
			this.label7.Text = "Speedrun.com Name";
			// 
			// countryCodeBox
			// 
			this.countryCodeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.countryCodeBox.Location = new System.Drawing.Point(10, 68);
			this.countryCodeBox.Name = "countryCodeBox";
			this.countryCodeBox.Size = new System.Drawing.Size(458, 20);
			this.countryCodeBox.TabIndex = 5;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(7, 52);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(71, 13);
			this.label6.TabIndex = 4;
			this.label6.Text = "Country Code";
			// 
			// displayNameBox
			// 
			this.displayNameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.displayNameBox.Location = new System.Drawing.Point(10, 24);
			this.displayNameBox.Name = "displayNameBox";
			this.displayNameBox.Size = new System.Drawing.Size(458, 20);
			this.displayNameBox.TabIndex = 3;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(7, 7);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Display Name";
			// 
			// settingsTabPage
			// 
			this.settingsTabPage.Controls.Add(this.resetSettingsButton);
			this.settingsTabPage.Controls.Add(this.groupIdBox);
			this.settingsTabPage.Controls.Add(this.label4);
			this.settingsTabPage.Controls.Add(this.deviceIdBox);
			this.settingsTabPage.Controls.Add(this.label3);
			this.settingsTabPage.Controls.Add(this.positionsBox);
			this.settingsTabPage.Controls.Add(this.label2);
			this.settingsTabPage.Controls.Add(this.targetUrlBox);
			this.settingsTabPage.Controls.Add(this.label1);
			this.settingsTabPage.Controls.Add(this.applySettingsButton);
			this.settingsTabPage.Location = new System.Drawing.Point(4, 22);
			this.settingsTabPage.Name = "settingsTabPage";
			this.settingsTabPage.Size = new System.Drawing.Size(476, 423);
			this.settingsTabPage.TabIndex = 2;
			this.settingsTabPage.Text = "Settings";
			this.settingsTabPage.UseVisualStyleBackColor = true;
			// 
			// resetSettingsButton
			// 
			this.resetSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.resetSettingsButton.Location = new System.Drawing.Point(393, 397);
			this.resetSettingsButton.Name = "resetSettingsButton";
			this.resetSettingsButton.Size = new System.Drawing.Size(75, 23);
			this.resetSettingsButton.TabIndex = 10;
			this.resetSettingsButton.Text = "Reset";
			this.resetSettingsButton.UseVisualStyleBackColor = true;
			this.resetSettingsButton.Click += new System.EventHandler(this.ResetSettingsButton_Click);
			// 
			// groupIdBox
			// 
			this.groupIdBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupIdBox.Location = new System.Drawing.Point(8, 164);
			this.groupIdBox.Name = "groupIdBox";
			this.groupIdBox.Size = new System.Drawing.Size(460, 20);
			this.groupIdBox.TabIndex = 8;
			this.groupIdBox.TextChanged += new System.EventHandler(this.SettingTextChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(9, 147);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(50, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Group ID";
			// 
			// deviceIdBox
			// 
			this.deviceIdBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.deviceIdBox.Location = new System.Drawing.Point(8, 124);
			this.deviceIdBox.Name = "deviceIdBox";
			this.deviceIdBox.Size = new System.Drawing.Size(460, 20);
			this.deviceIdBox.TabIndex = 6;
			this.deviceIdBox.TextChanged += new System.EventHandler(this.SettingTextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 108);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(55, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Device ID";
			// 
			// positionsBox
			// 
			this.positionsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.positionsBox.Location = new System.Drawing.Point(8, 64);
			this.positionsBox.Name = "positionsBox";
			this.positionsBox.Size = new System.Drawing.Size(460, 20);
			this.positionsBox.TabIndex = 4;
			this.positionsBox.TextChanged += new System.EventHandler(this.SettingTextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(95, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Available Positions";
			// 
			// targetUrlBox
			// 
			this.targetUrlBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.targetUrlBox.Location = new System.Drawing.Point(8, 20);
			this.targetUrlBox.Name = "targetUrlBox";
			this.targetUrlBox.Size = new System.Drawing.Size(460, 20);
			this.targetUrlBox.TabIndex = 2;
			this.targetUrlBox.TextChanged += new System.EventHandler(this.SettingTextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Target URL";
			// 
			// applySettingsButton
			// 
			this.applySettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.applySettingsButton.Enabled = false;
			this.applySettingsButton.Location = new System.Drawing.Point(8, 397);
			this.applySettingsButton.Name = "applySettingsButton";
			this.applySettingsButton.Size = new System.Drawing.Size(75, 23);
			this.applySettingsButton.TabIndex = 9;
			this.applySettingsButton.Text = "Apply";
			this.applySettingsButton.UseVisualStyleBackColor = true;
			this.applySettingsButton.Click += new System.EventHandler(this.ApplySettingsButton_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(484, 24);
			this.menuStrip1.TabIndex = 5;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
			// 
			// sendToLoginButton
			// 
			this.sendToLoginButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.sendToLoginButton.Location = new System.Drawing.Point(372, 397);
			this.sendToLoginButton.Name = "sendToLoginButton";
			this.sendToLoginButton.Size = new System.Drawing.Size(96, 23);
			this.sendToLoginButton.TabIndex = 15;
			this.sendToLoginButton.Text = "Send to Login";
			this.sendToLoginButton.UseVisualStyleBackColor = true;
			this.sendToLoginButton.Click += new System.EventHandler(this.SendToLoginButton_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(484, 611);
			this.Controls.Add(this.mainSplitContainer);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.MinimumSize = new System.Drawing.Size(500, 600);
			this.Name = "MainForm";
			this.Text = "FlagCarrier for Windows";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.loginTagPage.ResumeLayout(false);
			this.loginTagPage.PerformLayout();
			this.writeTabPage.ResumeLayout(false);
			this.writeTabPage.PerformLayout();
			this.settingsTabPage.ResumeLayout(false);
			this.settingsTabPage.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
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
		private System.Windows.Forms.SplitContainer mainSplitContainer;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage loginTagPage;
		private System.Windows.Forms.TabPage writeTabPage;
		private System.Windows.Forms.TabPage settingsTabPage;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.Button applySettingsButton;
		private System.Windows.Forms.TextBox targetUrlBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox positionsBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox groupIdBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox deviceIdBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button resetSettingsButton;
		private System.Windows.Forms.TextBox twitterHandleBox;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox twitchNameBox;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox srcomNameBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox countryCodeBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox displayNameBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox extraDataBox;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.RichTextBox loginTextBox;
		private System.Windows.Forms.ComboBox positionSelectBox;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Button loginButton;
		private System.Windows.Forms.Button clearButton;
		private System.Windows.Forms.Button sendToLoginButton;
	}
}

