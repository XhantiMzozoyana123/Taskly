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
            cboCookie = new ComboBox();
            lblCookie = new Label();
            cboPlatform = new ComboBox();
            lblPlatform = new Label();
            btnBulkSearch = new Button();
            txtPages = new TextBox();
            btnSearch = new Button();
            lblPages = new Label();
            rtxtQuery = new RichTextBox();
            ckPrivateMode = new CheckBox();
            lblQuery = new Label();
            txtKeywords = new TextBox();
            lblKeywords = new Label();
            grpCookie = new GroupBox();
            cboUsername = new ComboBox();
            rtxtMessage = new RichTextBox();
            menuStrip1 = new MenuStrip();
            dataControlsToolStripMenuItem = new ToolStripMenuItem();
            cookiesToolStripMenuItem = new ToolStripMenuItem();
            serviceWorkersToolStripMenuItem = new ToolStripMenuItem();
            button1 = new Button();
            grpSumary = new GroupBox();
            lblContactedLeads = new Label();
            txtContactedLeads = new TextBox();
            lblUnique = new Label();
            txtUniqueLeads = new TextBox();
            lblCollectLeads = new Label();
            txtCollectedLeads = new TextBox();
            ckMessageRotate = new CheckBox();
            btnDataControls = new Button();
            ckAccountRotate = new CheckBox();
            grpDM = new GroupBox();
            btnRemoveGroup = new Button();
            btnAddGroup = new Button();
            lblMessagingGroup = new Label();
            cboMessagingGroup = new ComboBox();
            grpSearchInput.SuspendLayout();
            grpCookie.SuspendLayout();
            menuStrip1.SuspendLayout();
            grpSumary.SuspendLayout();
            grpDM.SuspendLayout();
            SuspendLayout();
            // 
            // grpSearchInput
            // 
            grpSearchInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSearchInput.Controls.Add(cboCookie);
            grpSearchInput.Controls.Add(lblCookie);
            grpSearchInput.Controls.Add(cboPlatform);
            grpSearchInput.Controls.Add(lblPlatform);
            grpSearchInput.Controls.Add(btnBulkSearch);
            grpSearchInput.Controls.Add(txtPages);
            grpSearchInput.Controls.Add(btnSearch);
            grpSearchInput.Controls.Add(lblPages);
            grpSearchInput.Controls.Add(rtxtQuery);
            grpSearchInput.Controls.Add(ckPrivateMode);
            grpSearchInput.Controls.Add(lblQuery);
            grpSearchInput.Controls.Add(txtKeywords);
            grpSearchInput.Controls.Add(lblKeywords);
            grpSearchInput.Location = new Point(12, 38);
            grpSearchInput.Name = "grpSearchInput";
            grpSearchInput.Size = new Size(1124, 329);
            grpSearchInput.TabIndex = 0;
            grpSearchInput.TabStop = false;
            grpSearchInput.Text = "Search Input";
            // 
            // cboCookie
            // 
            cboCookie.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboCookie.FormattingEnabled = true;
            cboCookie.Location = new Point(176, 141);
            cboCookie.Name = "cboCookie";
            cboCookie.Size = new Size(730, 28);
            cboCookie.TabIndex = 17;
            // 
            // lblCookie
            // 
            lblCookie.AutoSize = true;
            lblCookie.Location = new Point(23, 144);
            lblCookie.Name = "lblCookie";
            lblCookie.Size = new Size(55, 20);
            lblCookie.TabIndex = 16;
            lblCookie.Text = "Cookie";
            // 
            // cboPlatform
            // 
            cboPlatform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboPlatform.FormattingEnabled = true;
            cboPlatform.Location = new Point(176, 107);
            cboPlatform.Name = "cboPlatform";
            cboPlatform.Size = new Size(730, 28);
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
            // btnBulkSearch
            // 
            btnBulkSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBulkSearch.Location = new Point(912, 103);
            btnBulkSearch.Name = "btnBulkSearch";
            btnBulkSearch.Size = new Size(188, 56);
            btnBulkSearch.TabIndex = 2;
            btnBulkSearch.Text = "Bulk Search";
            btnBulkSearch.UseVisualStyleBackColor = true;
            // 
            // txtPages
            // 
            txtPages.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPages.Location = new Point(176, 74);
            txtPages.Name = "txtPages";
            txtPages.Size = new Size(730, 27);
            txtPages.TabIndex = 13;
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSearch.Location = new Point(912, 41);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(188, 56);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
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
            rtxtQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtxtQuery.Location = new Point(176, 173);
            rtxtQuery.Name = "rtxtQuery";
            rtxtQuery.Size = new Size(730, 127);
            rtxtQuery.TabIndex = 11;
            rtxtQuery.Text = "";
            // 
            // ckPrivateMode
            // 
            ckPrivateMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ckPrivateMode.AutoSize = true;
            ckPrivateMode.Location = new Point(912, 173);
            ckPrivateMode.Name = "ckPrivateMode";
            ckPrivateMode.Size = new Size(119, 24);
            ckPrivateMode.TabIndex = 10;
            ckPrivateMode.Text = "Private Mode";
            ckPrivateMode.UseVisualStyleBackColor = true;
            // 
            // lblQuery
            // 
            lblQuery.AutoSize = true;
            lblQuery.Location = new Point(23, 173);
            lblQuery.Name = "lblQuery";
            lblQuery.Size = new Size(48, 20);
            lblQuery.TabIndex = 2;
            lblQuery.Text = "Query";
            // 
            // txtKeywords
            // 
            txtKeywords.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtKeywords.Location = new Point(176, 41);
            txtKeywords.Name = "txtKeywords";
            txtKeywords.Size = new Size(730, 27);
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
            // grpCookie
            // 
            grpCookie.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            grpCookie.Controls.Add(cboUsername);
            grpCookie.Location = new Point(12, 373);
            grpCookie.Name = "grpCookie";
            grpCookie.Size = new Size(388, 90);
            grpCookie.TabIndex = 4;
            grpCookie.TabStop = false;
            grpCookie.Text = "Selected Cookie";
            // 
            // cboUsername
            // 
            cboUsername.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cboUsername.FormattingEnabled = true;
            cboUsername.Location = new Point(23, 38);
            cboUsername.Name = "cboUsername";
            cboUsername.Size = new Size(350, 28);
            cboUsername.TabIndex = 0;
            // 
            // rtxtMessage
            // 
            rtxtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtxtMessage.Location = new Point(17, 29);
            rtxtMessage.Name = "rtxtMessage";
            rtxtMessage.Size = new Size(698, 178);
            rtxtMessage.TabIndex = 5;
            rtxtMessage.Text = "";
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { dataControlsToolStripMenuItem, cookiesToolStripMenuItem, serviceWorkersToolStripMenuItem });
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
            dataControlsToolStripMenuItem.Click += dataControlsToolStripMenuItem_Click;
            // 
            // cookiesToolStripMenuItem
            // 
            cookiesToolStripMenuItem.Name = "cookiesToolStripMenuItem";
            cookiesToolStripMenuItem.Size = new Size(75, 24);
            cookiesToolStripMenuItem.Text = "Cookies";
            cookiesToolStripMenuItem.Click += cookiesToolStripMenuItem_Click;
            // 
            // serviceWorkersToolStripMenuItem
            // 
            serviceWorkersToolStripMenuItem.Name = "serviceWorkersToolStripMenuItem";
            serviceWorkersToolStripMenuItem.Size = new Size(127, 24);
            serviceWorkersToolStripMenuItem.Text = "Service Workers";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Location = new Point(983, 634);
            button1.Name = "button1";
            button1.Size = new Size(153, 47);
            button1.TabIndex = 7;
            button1.Text = "Send";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // grpSumary
            // 
            grpSumary.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            grpSumary.Controls.Add(lblContactedLeads);
            grpSumary.Controls.Add(txtContactedLeads);
            grpSumary.Controls.Add(lblUnique);
            grpSumary.Controls.Add(txtUniqueLeads);
            grpSumary.Controls.Add(lblCollectLeads);
            grpSumary.Controls.Add(txtCollectedLeads);
            grpSumary.Location = new Point(12, 469);
            grpSumary.Name = "grpSumary";
            grpSumary.Size = new Size(388, 159);
            grpSumary.TabIndex = 5;
            grpSumary.TabStop = false;
            grpSumary.Text = "Summary";
            // 
            // lblContactedLeads
            // 
            lblContactedLeads.AutoSize = true;
            lblContactedLeads.Location = new Point(23, 113);
            lblContactedLeads.Name = "lblContactedLeads";
            lblContactedLeads.Size = new Size(119, 20);
            lblContactedLeads.TabIndex = 23;
            lblContactedLeads.Text = "Contacted Leads";
            // 
            // txtContactedLeads
            // 
            txtContactedLeads.Location = new Point(166, 110);
            txtContactedLeads.Name = "txtContactedLeads";
            txtContactedLeads.ReadOnly = true;
            txtContactedLeads.Size = new Size(207, 27);
            txtContactedLeads.TabIndex = 22;
            // 
            // lblUnique
            // 
            lblUnique.AutoSize = true;
            lblUnique.Location = new Point(23, 80);
            lblUnique.Name = "lblUnique";
            lblUnique.Size = new Size(98, 20);
            lblUnique.TabIndex = 21;
            lblUnique.Text = "Unique Leads";
            // 
            // txtUniqueLeads
            // 
            txtUniqueLeads.Location = new Point(166, 77);
            txtUniqueLeads.Name = "txtUniqueLeads";
            txtUniqueLeads.ReadOnly = true;
            txtUniqueLeads.Size = new Size(207, 27);
            txtUniqueLeads.TabIndex = 20;
            // 
            // lblCollectLeads
            // 
            lblCollectLeads.AutoSize = true;
            lblCollectLeads.Location = new Point(23, 47);
            lblCollectLeads.Name = "lblCollectLeads";
            lblCollectLeads.Size = new Size(114, 20);
            lblCollectLeads.TabIndex = 19;
            lblCollectLeads.Text = "Collected Leads";
            // 
            // txtCollectedLeads
            // 
            txtCollectedLeads.Location = new Point(166, 44);
            txtCollectedLeads.Name = "txtCollectedLeads";
            txtCollectedLeads.ReadOnly = true;
            txtCollectedLeads.Size = new Size(207, 27);
            txtCollectedLeads.TabIndex = 18;
            // 
            // ckMessageRotate
            // 
            ckMessageRotate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ckMessageRotate.AutoSize = true;
            ckMessageRotate.Location = new Point(575, 646);
            ckMessageRotate.Name = "ckMessageRotate";
            ckMessageRotate.Size = new Size(163, 24);
            ckMessageRotate.TabIndex = 18;
            ckMessageRotate.Text = "Messaging Rotation";
            ckMessageRotate.UseVisualStyleBackColor = true;
            // 
            // btnDataControls
            // 
            btnDataControls.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDataControls.Location = new Point(12, 634);
            btnDataControls.Name = "btnDataControls";
            btnDataControls.Size = new Size(153, 47);
            btnDataControls.TabIndex = 19;
            btnDataControls.Text = "Data Controls";
            btnDataControls.UseVisualStyleBackColor = true;
            btnDataControls.Click += btnDataControls_Click;
            // 
            // ckAccountRotate
            // 
            ckAccountRotate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ckAccountRotate.AutoSize = true;
            ckAccountRotate.Location = new Point(406, 646);
            ckAccountRotate.Name = "ckAccountRotate";
            ckAccountRotate.Size = new Size(146, 24);
            ckAccountRotate.TabIndex = 20;
            ckAccountRotate.Text = "Account Rotation";
            ckAccountRotate.UseVisualStyleBackColor = true;
            // 
            // grpDM
            // 
            grpDM.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpDM.Controls.Add(btnRemoveGroup);
            grpDM.Controls.Add(btnAddGroup);
            grpDM.Controls.Add(lblMessagingGroup);
            grpDM.Controls.Add(cboMessagingGroup);
            grpDM.Controls.Add(rtxtMessage);
            grpDM.Location = new Point(406, 373);
            grpDM.Name = "grpDM";
            grpDM.Size = new Size(730, 255);
            grpDM.TabIndex = 21;
            grpDM.TabStop = false;
            grpDM.Text = "Direct Message";
            // 
            // btnRemoveGroup
            // 
            btnRemoveGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRemoveGroup.Location = new Point(616, 212);
            btnRemoveGroup.Name = "btnRemoveGroup";
            btnRemoveGroup.Size = new Size(99, 28);
            btnRemoveGroup.TabIndex = 23;
            btnRemoveGroup.Text = "Remove";
            btnRemoveGroup.UseVisualStyleBackColor = true;
            btnRemoveGroup.Click += btnRemoveGroup_Click;
            // 
            // btnAddGroup
            // 
            btnAddGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAddGroup.Location = new Point(511, 212);
            btnAddGroup.Name = "btnAddGroup";
            btnAddGroup.Size = new Size(99, 28);
            btnAddGroup.TabIndex = 22;
            btnAddGroup.Text = "Add";
            btnAddGroup.UseVisualStyleBackColor = true;
            btnAddGroup.Click += btnAddGroup_Click;
            // 
            // lblMessagingGroup
            // 
            lblMessagingGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblMessagingGroup.AutoSize = true;
            lblMessagingGroup.Location = new Point(17, 216);
            lblMessagingGroup.Name = "lblMessagingGroup";
            lblMessagingGroup.Size = new Size(118, 20);
            lblMessagingGroup.TabIndex = 20;
            lblMessagingGroup.Text = "Message Groups";
            // 
            // cboMessagingGroup
            // 
            cboMessagingGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cboMessagingGroup.FormattingEnabled = true;
            cboMessagingGroup.Location = new Point(141, 213);
            cboMessagingGroup.Name = "cboMessagingGroup";
            cboMessagingGroup.Size = new Size(364, 28);
            cboMessagingGroup.TabIndex = 1;
            cboMessagingGroup.SelectedIndexChanged += cboMessagingGroup_SelectedIndexChanged;
            // 
            // Taskly
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1148, 693);
            Controls.Add(grpDM);
            Controls.Add(ckAccountRotate);
            Controls.Add(btnDataControls);
            Controls.Add(ckMessageRotate);
            Controls.Add(grpSumary);
            Controls.Add(button1);
            Controls.Add(grpCookie);
            Controls.Add(grpSearchInput);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Taskly";
            Text = "Taskly";
            Load += Taskly_Load;
            grpSearchInput.ResumeLayout(false);
            grpSearchInput.PerformLayout();
            grpCookie.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            grpSumary.ResumeLayout(false);
            grpSumary.PerformLayout();
            grpDM.ResumeLayout(false);
            grpDM.PerformLayout();
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
        private GroupBox grpCookie;
        private RichTextBox rtxtMessage;
        private TextBox txtPages;
        private Label lblPages;
        private RichTextBox rtxtQuery;
        private CheckBox ckPrivateMode;
        private Label lblPlatform;
        private ComboBox cboPlatform;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem dataControlsToolStripMenuItem;
        private Button button1;
        private GroupBox grpSumary;
        private ComboBox cboUsername;
        private ToolStripMenuItem cookiesToolStripMenuItem;
        private ToolStripMenuItem serviceWorkersToolStripMenuItem;
        private ComboBox cboCookie;
        private Label lblCookie;
        private CheckBox ckMessageRotate;
        private Button btnDataControls;
        private CheckBox ckAccountRotate;
        private Label lblCollectLeads;
        private TextBox txtCollectedLeads;
        private Label lblContactedLeads;
        private TextBox txtContactedLeads;
        private Label lblUnique;
        private TextBox txtUniqueLeads;
        private GroupBox grpDM;
        private ComboBox cboMessagingGroup;
        private Label lblMessagingGroup;
        private Button btnRemoveGroup;
        private Button btnAddGroup;
    }
}