namespace Taskly.Forms.Forms
{
    partial class Taskly
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
            grpSearchInput = new GroupBox();
            cboPlatform = new ComboBox();
            lblPlatform = new Label();
            txtPages = new TextBox();
            lblPages = new Label();
            rtxtQuery = new RichTextBox();
            ckPrivateMode = new CheckBox();
            lblQuery = new Label();
            txtKeywords = new TextBox();
            lblKeywords = new Label();
            btnSearch = new Button();
            btnBulkSearch = new Button();
            grpAccounts = new GroupBox();
            richTextBox1 = new RichTextBox();
            menuStrip1 = new MenuStrip();
            dataControlsToolStripMenuItem = new ToolStripMenuItem();
            accountsToolStripMenuItem = new ToolStripMenuItem();
            button1 = new Button();
            grpSumary = new GroupBox();
            grpSearchInput.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // grpSearchInput
            // 
            grpSearchInput.Controls.Add(cboPlatform);
            grpSearchInput.Controls.Add(lblPlatform);
            grpSearchInput.Controls.Add(txtPages);
            grpSearchInput.Controls.Add(lblPages);
            grpSearchInput.Controls.Add(rtxtQuery);
            grpSearchInput.Controls.Add(ckPrivateMode);
            grpSearchInput.Controls.Add(lblQuery);
            grpSearchInput.Controls.Add(txtKeywords);
            grpSearchInput.Controls.Add(lblKeywords);
            grpSearchInput.Location = new Point(12, 38);
            grpSearchInput.Name = "grpSearchInput";
            grpSearchInput.Size = new Size(930, 329);
            grpSearchInput.TabIndex = 0;
            grpSearchInput.TabStop = false;
            grpSearchInput.Text = "Search Input";
            // 
            // cboPlatform
            // 
            cboPlatform.FormattingEnabled = true;
            cboPlatform.Location = new Point(176, 107);
            cboPlatform.Name = "cboPlatform";
            cboPlatform.Size = new Size(586, 28);
            cboPlatform.TabIndex = 15;
            // 
            // lblPlatform
            // 
            lblPlatform.AutoSize = true;
            lblPlatform.Location = new Point(23, 110);
            lblPlatform.Name = "lblPlatform";
            lblPlatform.Size = new Size(66, 20);
            lblPlatform.TabIndex = 14;
            lblPlatform.Text = "Platform";
            // 
            // txtPages
            // 
            txtPages.Location = new Point(176, 74);
            txtPages.Name = "txtPages";
            txtPages.Size = new Size(586, 27);
            txtPages.TabIndex = 13;
            // 
            // lblPages
            // 
            lblPages.AutoSize = true;
            lblPages.Location = new Point(23, 77);
            lblPages.Name = "lblPages";
            lblPages.Size = new Size(47, 20);
            lblPages.TabIndex = 12;
            lblPages.Text = "Pages";
            // 
            // rtxtQuery
            // 
            rtxtQuery.Location = new Point(176, 141);
            rtxtQuery.Name = "rtxtQuery";
            rtxtQuery.Size = new Size(586, 159);
            rtxtQuery.TabIndex = 11;
            rtxtQuery.Text = "";
            // 
            // ckPrivateMode
            // 
            ckPrivateMode.AutoSize = true;
            ckPrivateMode.Location = new Point(781, 74);
            ckPrivateMode.Name = "ckPrivateMode";
            ckPrivateMode.Size = new Size(119, 24);
            ckPrivateMode.TabIndex = 10;
            ckPrivateMode.Text = "Private Mode";
            ckPrivateMode.UseVisualStyleBackColor = true;
            // 
            // lblQuery
            // 
            lblQuery.AutoSize = true;
            lblQuery.Location = new Point(23, 141);
            lblQuery.Name = "lblQuery";
            lblQuery.Size = new Size(48, 20);
            lblQuery.TabIndex = 2;
            lblQuery.Text = "Query";
            // 
            // txtKeywords
            // 
            txtKeywords.Location = new Point(176, 41);
            txtKeywords.Name = "txtKeywords";
            txtKeywords.Size = new Size(586, 27);
            txtKeywords.TabIndex = 1;
            // 
            // lblKeywords
            // 
            lblKeywords.AutoSize = true;
            lblKeywords.Location = new Point(23, 44);
            lblKeywords.Name = "lblKeywords";
            lblKeywords.Size = new Size(73, 20);
            lblKeywords.TabIndex = 0;
            lblKeywords.Text = "Keywords";
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(948, 50);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(188, 56);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnBulkSearch
            // 
            btnBulkSearch.Location = new Point(948, 112);
            btnBulkSearch.Name = "btnBulkSearch";
            btnBulkSearch.Size = new Size(188, 56);
            btnBulkSearch.TabIndex = 2;
            btnBulkSearch.Text = "Bulk Search";
            btnBulkSearch.UseVisualStyleBackColor = true;
            // 
            // grpAccounts
            // 
            grpAccounts.Location = new Point(12, 373);
            grpAccounts.Name = "grpAccounts";
            grpAccounts.Size = new Size(332, 123);
            grpAccounts.TabIndex = 4;
            grpAccounts.TabStop = false;
            grpAccounts.Text = "Account";
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(350, 373);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(786, 255);
            richTextBox1.TabIndex = 5;
            richTextBox1.Text = "";
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { dataControlsToolStripMenuItem, accountsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1148, 28);
            menuStrip1.TabIndex = 6;
            menuStrip1.Text = "menuStrip1";
            // 
            // dataControlsToolStripMenuItem
            // 
            dataControlsToolStripMenuItem.Name = "dataControlsToolStripMenuItem";
            dataControlsToolStripMenuItem.Size = new Size(114, 24);
            dataControlsToolStripMenuItem.Text = "Data Controls";
            // 
            // accountsToolStripMenuItem
            // 
            accountsToolStripMenuItem.Name = "accountsToolStripMenuItem";
            accountsToolStripMenuItem.Size = new Size(83, 24);
            accountsToolStripMenuItem.Text = "Accounts";
            accountsToolStripMenuItem.Click += accountsToolStripMenuItem_Click;
            // 
            // button1
            // 
            button1.Location = new Point(983, 634);
            button1.Name = "button1";
            button1.Size = new Size(153, 47);
            button1.TabIndex = 7;
            button1.Text = "Send";
            button1.UseVisualStyleBackColor = true;
            // 
            // grpSumary
            // 
            grpSumary.Location = new Point(12, 502);
            grpSumary.Name = "grpSumary";
            grpSumary.Size = new Size(332, 126);
            grpSumary.TabIndex = 5;
            grpSumary.TabStop = false;
            grpSumary.Text = "Summary";
            // 
            // Taskly
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1148, 693);
            Controls.Add(grpSumary);
            Controls.Add(button1);
            Controls.Add(richTextBox1);
            Controls.Add(grpAccounts);
            Controls.Add(btnBulkSearch);
            Controls.Add(btnSearch);
            Controls.Add(grpSearchInput);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Taskly";
            Text = "Taskly";
            Load += Taskly_Load;
            grpSearchInput.ResumeLayout(false);
            grpSearchInput.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox grpSearchInput;
        private Label lblQuery;
        private TextBox txtKeywords;
        private Label lblKeywords;
        private Button btnSearch;
        private Button btnBulkSearch;
        private GroupBox grpAccounts;
        private RichTextBox richTextBox1;
        private TextBox txtPages;
        private Label lblPages;
        private RichTextBox rtxtQuery;
        private CheckBox ckPrivateMode;
        private Label lblPlatform;
        private ComboBox cboPlatform;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem dataControlsToolStripMenuItem;
        private ToolStripMenuItem accountsToolStripMenuItem;
        private Button button1;
        private GroupBox grpSumary;
    }
}