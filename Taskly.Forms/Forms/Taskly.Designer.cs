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
            ckMultiPlatform = new CheckBox();
            cboCookie = new ComboBox();
            lblCookie = new Label();
            btnBulkSearch = new Button();
            txtPages = new TextBox();
            btnSearch = new Button();
            lblPages = new Label();
            rtxtQuery = new RichTextBox();
            ckPrivateMode = new CheckBox();
            lblQuery = new Label();
            txtKeywords = new TextBox();
            lblKeywords = new Label();
            rtxtMessage = new RichTextBox();
            menuStrip1 = new MenuStrip();
            dataControlsToolStripMenuItem = new ToolStripMenuItem();
            contentToolStripMenuItem = new ToolStripMenuItem();
            templatesToolStripMenuItem1 = new ToolStripMenuItem();
            customMessagesToolStripMenuItem1 = new ToolStripMenuItem();
            customMessagesToolStripMenuItem2 = new ToolStripMenuItem();
            cookiesToolStripMenuItem1 = new ToolStripMenuItem();
            campaignToolStripMenuItem = new ToolStripMenuItem();
            campaignToolStripMenuItem1 = new ToolStripMenuItem();
            sequencesToolStripMenuItem = new ToolStripMenuItem();
            messagesToolStripMenuItem = new ToolStripMenuItem();
            domainsToolStripMenuItem = new ToolStripMenuItem();
            integrationsToolStripMenuItem = new ToolStripMenuItem();
            googleAIGeminiToolStripMenuItem = new ToolStripMenuItem();
            lMStudioToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            button1 = new Button();
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
            lstLogs = new ListBox();
            btnClearLogs = new Button();
            lblTemplates = new Label();
            cboTemplates = new ComboBox();
            btnRefresh = new Button();
            btnCampaigns = new Button();
            grpSearchInput.SuspendLayout();
            menuStrip1.SuspendLayout();
            grpDM.SuspendLayout();
            SuspendLayout();
            // 
            // grpSearchInput
            // 
            grpSearchInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSearchInput.Controls.Add(ckMultiPlatform);
            grpSearchInput.Controls.Add(cboCookie);
            grpSearchInput.Controls.Add(lblCookie);
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
            grpSearchInput.Size = new Size(1124, 279);
            grpSearchInput.TabIndex = 0;
            grpSearchInput.TabStop = false;
            grpSearchInput.Text = "Search Input";
            // 
            // ckMultiPlatform
            // 
            ckMultiPlatform.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ckMultiPlatform.AutoSize = true;
            ckMultiPlatform.Location = new Point(912, 165);
            ckMultiPlatform.Name = "ckMultiPlatform";
            ckMultiPlatform.Size = new Size(116, 24);
            ckMultiPlatform.TabIndex = 18;
            ckMultiPlatform.Text = "All Platforms";
            ckMultiPlatform.UseVisualStyleBackColor = true;
            // 
            // cboCookie
            // 
            cboCookie.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboCookie.FormattingEnabled = true;
            cboCookie.Location = new Point(176, 107);
            cboCookie.Name = "cboCookie";
            cboCookie.Size = new Size(730, 28);
            cboCookie.TabIndex = 17;
            // 
            // lblCookie
            // 
            lblCookie.AutoSize = true;
            lblCookie.Location = new Point(23, 110);
            lblCookie.Name = "lblCookie";
            lblCookie.Size = new Size(55, 20);
            lblCookie.TabIndex = 16;
            lblCookie.Text = "Cookie";
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
            btnBulkSearch.Click += btnBulkSearch_Click;
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
            rtxtQuery.Location = new Point(176, 139);
            rtxtQuery.Name = "rtxtQuery";
            rtxtQuery.Size = new Size(730, 127);
            rtxtQuery.TabIndex = 11;
            rtxtQuery.Text = "";
            // 
            // ckPrivateMode
            // 
            ckPrivateMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ckPrivateMode.AutoSize = true;
            ckPrivateMode.Location = new Point(912, 195);
            ckPrivateMode.Name = "ckPrivateMode";
            ckPrivateMode.Size = new Size(119, 24);
            ckPrivateMode.TabIndex = 10;
            ckPrivateMode.Text = "Private Mode";
            ckPrivateMode.UseVisualStyleBackColor = true;
            // 
            // lblQuery
            // 
            lblQuery.AutoSize = true;
            lblQuery.Location = new Point(23, 139);
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
            // rtxtMessage
            // 
            rtxtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtxtMessage.Location = new Point(17, 29);
            rtxtMessage.Name = "rtxtMessage";
            rtxtMessage.Size = new Size(620, 238);
            rtxtMessage.TabIndex = 5;
            rtxtMessage.Text = "";
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { dataControlsToolStripMenuItem, cookiesToolStripMenuItem1, contentToolStripMenuItem, campaignToolStripMenuItem, domainsToolStripMenuItem, integrationsToolStripMenuItem, settingsToolStripMenuItem });
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
            // contentToolStripMenuItem
            // 
            contentToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { templatesToolStripMenuItem1, customMessagesToolStripMenuItem1, customMessagesToolStripMenuItem2 });
            contentToolStripMenuItem.Name = "contentToolStripMenuItem";
            contentToolStripMenuItem.Size = new Size(75, 24);
            contentToolStripMenuItem.Text = "Content";
            // 
            // templatesToolStripMenuItem1
            // 
            templatesToolStripMenuItem1.Name = "templatesToolStripMenuItem1";
            templatesToolStripMenuItem1.Size = new Size(224, 26);
            templatesToolStripMenuItem1.Text = "Templates";
            templatesToolStripMenuItem1.Click += templatesToolStripMenuItem1_Click;
            // 
            // customMessagesToolStripMenuItem1
            // 
            customMessagesToolStripMenuItem1.Name = "customMessagesToolStripMenuItem1";
            customMessagesToolStripMenuItem1.Size = new Size(224, 26);
            customMessagesToolStripMenuItem1.Text = "Icebreakers";
            // 
            // customMessagesToolStripMenuItem2
            // 
            customMessagesToolStripMenuItem2.Name = "customMessagesToolStripMenuItem2";
            customMessagesToolStripMenuItem2.Size = new Size(224, 26);
            customMessagesToolStripMenuItem2.Text = "Custom Messages";
            customMessagesToolStripMenuItem2.Click += customMessagesToolStripMenuItem2_Click;
            // 
            // cookiesToolStripMenuItem1
            // 
            cookiesToolStripMenuItem1.Name = "cookiesToolStripMenuItem1";
            cookiesToolStripMenuItem1.Size = new Size(75, 24);
            cookiesToolStripMenuItem1.Text = "Cookies";
            cookiesToolStripMenuItem1.Click += cookiesToolStripMenuItem1_Click;
            // 
            // campaignToolStripMenuItem
            // 
            campaignToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { campaignToolStripMenuItem1, sequencesToolStripMenuItem, messagesToolStripMenuItem });
            campaignToolStripMenuItem.Name = "campaignToolStripMenuItem";
            campaignToolStripMenuItem.Size = new Size(97, 24);
            campaignToolStripMenuItem.Text = "Campaigns";
            // 
            // campaignToolStripMenuItem1
            // 
            campaignToolStripMenuItem1.Name = "campaignToolStripMenuItem1";
            campaignToolStripMenuItem1.Size = new Size(234, 26);
            campaignToolStripMenuItem1.Text = "Campaign";
            campaignToolStripMenuItem1.Click += campaignToolStripMenuItem1_Click;
            // 
            // sequencesToolStripMenuItem
            // 
            sequencesToolStripMenuItem.Name = "sequencesToolStripMenuItem";
            sequencesToolStripMenuItem.Size = new Size(234, 26);
            sequencesToolStripMenuItem.Text = "Campaign Sequences";
            // 
            // messagesToolStripMenuItem
            // 
            messagesToolStripMenuItem.Name = "messagesToolStripMenuItem";
            messagesToolStripMenuItem.Size = new Size(234, 26);
            messagesToolStripMenuItem.Text = "Campaign Messages";
            // 
            // domainsToolStripMenuItem
            // 
            domainsToolStripMenuItem.Name = "domainsToolStripMenuItem";
            domainsToolStripMenuItem.Size = new Size(82, 24);
            domainsToolStripMenuItem.Text = "Domains";
            domainsToolStripMenuItem.Click += domainsToolStripMenuItem_Click;
            // 
            // integrationsToolStripMenuItem
            // 
            integrationsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { googleAIGeminiToolStripMenuItem, lMStudioToolStripMenuItem });
            integrationsToolStripMenuItem.Name = "integrationsToolStripMenuItem";
            integrationsToolStripMenuItem.Size = new Size(102, 24);
            integrationsToolStripMenuItem.Text = "Integrations";
            // 
            // googleAIGeminiToolStripMenuItem
            // 
            googleAIGeminiToolStripMenuItem.Name = "googleAIGeminiToolStripMenuItem";
            googleAIGeminiToolStripMenuItem.Size = new Size(220, 26);
            googleAIGeminiToolStripMenuItem.Text = "Google AI (Gemini)";
            googleAIGeminiToolStripMenuItem.Click += googleAIGeminiToolStripMenuItem_Click;
            // 
            // lMStudioToolStripMenuItem
            // 
            lMStudioToolStripMenuItem.Name = "lMStudioToolStripMenuItem";
            lMStudioToolStripMenuItem.Size = new Size(220, 26);
            lMStudioToolStripMenuItem.Text = "LM Studio";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(76, 24);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Location = new Point(983, 740);
            button1.Name = "button1";
            button1.Size = new Size(153, 47);
            button1.TabIndex = 7;
            button1.Text = "Send";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // lblContactedLeads
            // 
            lblContactedLeads.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblContactedLeads.AutoSize = true;
            lblContactedLeads.Location = new Point(21, 674);
            lblContactedLeads.Name = "lblContactedLeads";
            lblContactedLeads.Size = new Size(119, 20);
            lblContactedLeads.TabIndex = 23;
            lblContactedLeads.Text = "Contacted Leads";
            // 
            // txtContactedLeads
            // 
            txtContactedLeads.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtContactedLeads.Location = new Point(164, 671);
            txtContactedLeads.Name = "txtContactedLeads";
            txtContactedLeads.ReadOnly = true;
            txtContactedLeads.Size = new Size(303, 27);
            txtContactedLeads.TabIndex = 22;
            // 
            // lblUnique
            // 
            lblUnique.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblUnique.AutoSize = true;
            lblUnique.Location = new Point(21, 641);
            lblUnique.Name = "lblUnique";
            lblUnique.Size = new Size(98, 20);
            lblUnique.TabIndex = 21;
            lblUnique.Text = "Unique Leads";
            // 
            // txtUniqueLeads
            // 
            txtUniqueLeads.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtUniqueLeads.Location = new Point(164, 638);
            txtUniqueLeads.Name = "txtUniqueLeads";
            txtUniqueLeads.ReadOnly = true;
            txtUniqueLeads.Size = new Size(303, 27);
            txtUniqueLeads.TabIndex = 20;
            // 
            // lblCollectLeads
            // 
            lblCollectLeads.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCollectLeads.AutoSize = true;
            lblCollectLeads.Location = new Point(21, 608);
            lblCollectLeads.Name = "lblCollectLeads";
            lblCollectLeads.Size = new Size(114, 20);
            lblCollectLeads.TabIndex = 19;
            lblCollectLeads.Text = "Collected Leads";
            // 
            // txtCollectedLeads
            // 
            txtCollectedLeads.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtCollectedLeads.Location = new Point(164, 605);
            txtCollectedLeads.Name = "txtCollectedLeads";
            txtCollectedLeads.ReadOnly = true;
            txtCollectedLeads.Size = new Size(303, 27);
            txtCollectedLeads.TabIndex = 18;
            // 
            // ckMessageRotate
            // 
            ckMessageRotate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ckMessageRotate.AutoSize = true;
            ckMessageRotate.Location = new Point(17, 273);
            ckMessageRotate.Name = "ckMessageRotate";
            ckMessageRotate.Size = new Size(163, 24);
            ckMessageRotate.TabIndex = 18;
            ckMessageRotate.Text = "Messaging Rotation";
            ckMessageRotate.UseVisualStyleBackColor = true;
            // 
            // btnDataControls
            // 
            btnDataControls.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDataControls.Location = new Point(12, 740);
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
            ckAccountRotate.Location = new Point(186, 273);
            ckAccountRotate.Name = "ckAccountRotate";
            ckAccountRotate.Size = new Size(138, 24);
            ckAccountRotate.TabIndex = 20;
            ckAccountRotate.Text = "Cookie Rotation";
            ckAccountRotate.UseVisualStyleBackColor = true;
            // 
            // grpDM
            // 
            grpDM.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpDM.Controls.Add(rtxtMessage);
            grpDM.Controls.Add(ckAccountRotate);
            grpDM.Controls.Add(ckMessageRotate);
            grpDM.Location = new Point(484, 431);
            grpDM.Name = "grpDM";
            grpDM.Size = new Size(652, 303);
            grpDM.TabIndex = 21;
            grpDM.TabStop = false;
            grpDM.Text = "Direct Message";
            // 
            // btnRemoveGroup
            // 
            btnRemoveGroup.Location = new Point(368, 509);
            btnRemoveGroup.Name = "btnRemoveGroup";
            btnRemoveGroup.Size = new Size(99, 28);
            btnRemoveGroup.TabIndex = 23;
            btnRemoveGroup.Text = "Remove";
            btnRemoveGroup.UseVisualStyleBackColor = true;
            btnRemoveGroup.Click += btnRemoveGroup_Click;
            // 
            // btnAddGroup
            // 
            btnAddGroup.Location = new Point(263, 509);
            btnAddGroup.Name = "btnAddGroup";
            btnAddGroup.Size = new Size(99, 28);
            btnAddGroup.TabIndex = 22;
            btnAddGroup.Text = "Add";
            btnAddGroup.UseVisualStyleBackColor = true;
            btnAddGroup.Click += btnAddGroup_Click;
            // 
            // lblMessagingGroup
            // 
            lblMessagingGroup.AutoSize = true;
            lblMessagingGroup.Location = new Point(18, 478);
            lblMessagingGroup.Name = "lblMessagingGroup";
            lblMessagingGroup.Size = new Size(118, 20);
            lblMessagingGroup.TabIndex = 20;
            lblMessagingGroup.Text = "Message Groups";
            // 
            // cboMessagingGroup
            // 
            cboMessagingGroup.FormattingEnabled = true;
            cboMessagingGroup.Location = new Point(164, 475);
            cboMessagingGroup.Name = "cboMessagingGroup";
            cboMessagingGroup.Size = new Size(303, 28);
            cboMessagingGroup.TabIndex = 1;
            cboMessagingGroup.SelectedIndexChanged += cboMessagingGroup_SelectedIndexChanged;
            // 
            // lstLogs
            // 
            lstLogs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstLogs.FormattingEnabled = true;
            lstLogs.Location = new Point(12, 323);
            lstLogs.Name = "lstLogs";
            lstLogs.Size = new Size(930, 104);
            lstLogs.TabIndex = 25;
            // 
            // btnClearLogs
            // 
            btnClearLogs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearLogs.Location = new Point(948, 321);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new Size(188, 104);
            btnClearLogs.TabIndex = 19;
            btnClearLogs.Text = "Clear Logs";
            btnClearLogs.UseVisualStyleBackColor = true;
            btnClearLogs.Click += btnClearLogs_Click;
            // 
            // lblTemplates
            // 
            lblTemplates.AutoSize = true;
            lblTemplates.Location = new Point(18, 444);
            lblTemplates.Name = "lblTemplates";
            lblTemplates.Size = new Size(77, 20);
            lblTemplates.TabIndex = 27;
            lblTemplates.Text = "Templates";
            // 
            // cboTemplates
            // 
            cboTemplates.FormattingEnabled = true;
            cboTemplates.Location = new Point(164, 441);
            cboTemplates.Name = "cboTemplates";
            cboTemplates.Size = new Size(303, 28);
            cboTemplates.TabIndex = 26;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRefresh.Location = new Point(368, 706);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(99, 28);
            btnRefresh.TabIndex = 28;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnCampaigns
            // 
            btnCampaigns.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCampaigns.Location = new Point(171, 740);
            btnCampaigns.Name = "btnCampaigns";
            btnCampaigns.Size = new Size(153, 47);
            btnCampaigns.TabIndex = 29;
            btnCampaigns.Text = "Campaigns";
            btnCampaigns.UseVisualStyleBackColor = true;
            btnCampaigns.Click += btnCampaigns_Click;
            // 
            // Taskly
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1148, 799);
            Controls.Add(btnCampaigns);
            Controls.Add(btnRefresh);
            Controls.Add(lblTemplates);
            Controls.Add(cboTemplates);
            Controls.Add(lblContactedLeads);
            Controls.Add(btnClearLogs);
            Controls.Add(txtContactedLeads);
            Controls.Add(btnRemoveGroup);
            Controls.Add(lblUnique);
            Controls.Add(txtUniqueLeads);
            Controls.Add(lblCollectLeads);
            Controls.Add(btnAddGroup);
            Controls.Add(txtCollectedLeads);
            Controls.Add(lstLogs);
            Controls.Add(grpDM);
            Controls.Add(btnDataControls);
            Controls.Add(lblMessagingGroup);
            Controls.Add(cboMessagingGroup);
            Controls.Add(button1);
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
        private RichTextBox rtxtMessage;
        private TextBox txtPages;
        private Label lblPages;
        private RichTextBox rtxtQuery;
        private CheckBox ckPrivateMode;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem dataControlsToolStripMenuItem;
        private Button button1;
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
        private CheckBox ckMultiPlatform;
        private ListBox lstLogs;
        private Button btnClearLogs;
        private Label lblTemplates;
        private ComboBox cboTemplates;
        private Button btnRefresh;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem contentToolStripMenuItem;
        private ToolStripMenuItem templatesToolStripMenuItem1;
        private ToolStripMenuItem customMessagesToolStripMenuItem1;
        private ToolStripMenuItem customMessagesToolStripMenuItem2;
        private ToolStripMenuItem cookiesToolStripMenuItem1;
        private ToolStripMenuItem domainsToolStripMenuItem;
        private ToolStripMenuItem integrationsToolStripMenuItem;
        private ToolStripMenuItem googleAIGeminiToolStripMenuItem;
        private ToolStripMenuItem lMStudioToolStripMenuItem;
        private ToolStripMenuItem campaignToolStripMenuItem;
        private ToolStripMenuItem campaignToolStripMenuItem1;
        private ToolStripMenuItem sequencesToolStripMenuItem;
        private ToolStripMenuItem messagesToolStripMenuItem;
        private Button btnCampaigns;
    }
}