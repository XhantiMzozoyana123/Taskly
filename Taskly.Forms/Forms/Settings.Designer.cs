namespace Taskly.Forms.Forms
{
    partial class Settings
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
            grpSearch = new GroupBox();
            ckSearchCookieRotate = new CheckBox();
            ckSearchDomainRotate = new CheckBox();
            btnDownloadBatch = new Button();
            grpSender = new GroupBox();
            ckMessenginRandomCookieSelect = new CheckBox();
            txtMessegingDelay = new TextBox();
            label1 = new Label();
            grpAIOptions = new GroupBox();
            ckLMStudio = new CheckBox();
            ckGemini = new CheckBox();
            btnSaveChanges = new Button();
            grpDomainOptions = new GroupBox();
            txtDomain = new TextBox();
            lblMasterDomain = new Label();
            button1 = new Button();
            ckHttpMode = new CheckBox();
            grpSearch.SuspendLayout();
            grpSender.SuspendLayout();
            grpAIOptions.SuspendLayout();
            grpDomainOptions.SuspendLayout();
            SuspendLayout();
            // 
            // grpSearch
            // 
            grpSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSearch.Controls.Add(ckHttpMode);
            grpSearch.Controls.Add(ckSearchCookieRotate);
            grpSearch.Controls.Add(ckSearchDomainRotate);
            grpSearch.Controls.Add(btnDownloadBatch);
            grpSearch.Location = new Point(12, 12);
            grpSearch.Name = "grpSearch";
            grpSearch.Size = new Size(776, 163);
            grpSearch.TabIndex = 0;
            grpSearch.TabStop = false;
            grpSearch.Text = "Search Options";
            // 
            // ckSearchCookieRotate
            // 
            ckSearchCookieRotate.AutoSize = true;
            ckSearchCookieRotate.Location = new Point(22, 103);
            ckSearchCookieRotate.Name = "ckSearchCookieRotate";
            ckSearchCookieRotate.Size = new Size(417, 24);
            ckSearchCookieRotate.TabIndex = 3;
            ckSearchCookieRotate.Text = "Rotate through all cookies when extracting batches online.";
            ckSearchCookieRotate.UseVisualStyleBackColor = true;
            // 
            // ckSearchDomainRotate
            // 
            ckSearchDomainRotate.AutoSize = true;
            ckSearchDomainRotate.Location = new Point(22, 73);
            ckSearchDomainRotate.Name = "ckSearchDomainRotate";
            ckSearchDomainRotate.Size = new Size(424, 24);
            ckSearchDomainRotate.TabIndex = 2;
            ckSearchDomainRotate.Text = "Rotate through all domains when extracting batches online.";
            ckSearchDomainRotate.UseVisualStyleBackColor = true;
            // 
            // btnDownloadBatch
            // 
            btnDownloadBatch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDownloadBatch.Location = new Point(516, 51);
            btnDownloadBatch.Name = "btnDownloadBatch";
            btnDownloadBatch.Size = new Size(240, 46);
            btnDownloadBatch.TabIndex = 1;
            btnDownloadBatch.Text = "Download Batch Search File";
            btnDownloadBatch.UseVisualStyleBackColor = true;
            btnDownloadBatch.Click += btnDownloadBatch_Click;
            // 
            // grpSender
            // 
            grpSender.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSender.Controls.Add(ckMessenginRandomCookieSelect);
            grpSender.Controls.Add(txtMessegingDelay);
            grpSender.Controls.Add(label1);
            grpSender.Location = new Point(12, 279);
            grpSender.Name = "grpSender";
            grpSender.Size = new Size(776, 125);
            grpSender.TabIndex = 2;
            grpSender.TabStop = false;
            grpSender.Text = "Messaging Options";
            // 
            // ckMessenginRandomCookieSelect
            // 
            ckMessenginRandomCookieSelect.AutoSize = true;
            ckMessenginRandomCookieSelect.Location = new Point(22, 82);
            ckMessenginRandomCookieSelect.Name = "ckMessenginRandomCookieSelect";
            ckMessenginRandomCookieSelect.Size = new Size(514, 24);
            ckMessenginRandomCookieSelect.TabIndex = 4;
            ckMessenginRandomCookieSelect.Text = "Randomly select cookies when sending messages to associated accounts.";
            ckMessenginRandomCookieSelect.UseVisualStyleBackColor = true;
            // 
            // txtMessegingDelay
            // 
            txtMessegingDelay.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMessegingDelay.Location = new Point(142, 35);
            txtMessegingDelay.Name = "txtMessegingDelay";
            txtMessegingDelay.Size = new Size(614, 27);
            txtMessegingDelay.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 38);
            label1.Name = "label1";
            label1.Size = new Size(86, 20);
            label1.TabIndex = 0;
            label1.Text = "Delay (Min)";
            // 
            // grpAIOptions
            // 
            grpAIOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpAIOptions.Controls.Add(ckLMStudio);
            grpAIOptions.Controls.Add(ckGemini);
            grpAIOptions.Location = new Point(12, 410);
            grpAIOptions.Name = "grpAIOptions";
            grpAIOptions.Size = new Size(776, 134);
            grpAIOptions.TabIndex = 7;
            grpAIOptions.TabStop = false;
            grpAIOptions.Text = "AI Options";
            // 
            // ckLMStudio
            // 
            ckLMStudio.AutoSize = true;
            ckLMStudio.Location = new Point(19, 81);
            ckLMStudio.Name = "ckLMStudio";
            ckLMStudio.Size = new Size(302, 24);
            ckLMStudio.TabIndex = 6;
            ckLMStudio.Text = "Use LM Studio for AI computing (Offline)";
            ckLMStudio.UseVisualStyleBackColor = true;
            // 
            // ckGemini
            // 
            ckGemini.AutoSize = true;
            ckGemini.Location = new Point(19, 51);
            ckGemini.Name = "ckGemini";
            ckGemini.Size = new Size(343, 24);
            ckGemini.TabIndex = 5;
            ckGemini.Text = "Rotate through all API Keys when using Gemini.";
            ckGemini.UseVisualStyleBackColor = true;
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveChanges.Location = new Point(609, 566);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(179, 46);
            btnSaveChanges.TabIndex = 4;
            btnSaveChanges.Text = "Save Changes";
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // grpDomainOptions
            // 
            grpDomainOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpDomainOptions.Controls.Add(txtDomain);
            grpDomainOptions.Controls.Add(lblMasterDomain);
            grpDomainOptions.Controls.Add(button1);
            grpDomainOptions.Location = new Point(12, 181);
            grpDomainOptions.Name = "grpDomainOptions";
            grpDomainOptions.Size = new Size(776, 92);
            grpDomainOptions.TabIndex = 4;
            grpDomainOptions.TabStop = false;
            grpDomainOptions.Text = "Domain Options";
            // 
            // txtDomain
            // 
            txtDomain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDomain.Location = new Point(139, 37);
            txtDomain.Name = "txtDomain";
            txtDomain.Size = new Size(614, 27);
            txtDomain.TabIndex = 5;
            // 
            // lblMasterDomain
            // 
            lblMasterDomain.AutoSize = true;
            lblMasterDomain.Location = new Point(22, 40);
            lblMasterDomain.Name = "lblMasterDomain";
            lblMasterDomain.Size = new Size(111, 20);
            lblMasterDomain.TabIndex = 2;
            lblMasterDomain.Text = "Master Domain";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(1092, 51);
            button1.Name = "button1";
            button1.Size = new Size(240, 46);
            button1.TabIndex = 1;
            button1.Text = "Download Batch Search File";
            button1.UseVisualStyleBackColor = true;
            // 
            // ckHttpMode
            // 
            ckHttpMode.AutoSize = true;
            ckHttpMode.Location = new Point(22, 43);
            ckHttpMode.Name = "ckHttpMode";
            ckHttpMode.Size = new Size(318, 24);
            ckHttpMode.TabIndex = 4;
            ckHttpMode.Text = "Process all files and data extractions online.";
            ckHttpMode.UseVisualStyleBackColor = true;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 624);
            Controls.Add(grpDomainOptions);
            Controls.Add(btnSaveChanges);
            Controls.Add(grpAIOptions);
            Controls.Add(grpSender);
            Controls.Add(grpSearch);
            Name = "Settings";
            Text = "Settings";
            Load += Settings_Load_1;
            grpSearch.ResumeLayout(false);
            grpSearch.PerformLayout();
            grpSender.ResumeLayout(false);
            grpSender.PerformLayout();
            grpAIOptions.ResumeLayout(false);
            grpAIOptions.PerformLayout();
            grpDomainOptions.ResumeLayout(false);
            grpDomainOptions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpSearch;
        private Button btnDownloadBatch;
        private GroupBox grpSender;
        private TextBox txtMessegingDelay;
        private Label label1;
        private CheckBox ckSearchDomainRotate;
        private CheckBox ckSearchCookieRotate;
        private CheckBox ckMessenginRandomCookieSelect;
        private GroupBox grpAIOptions;
        private Button btnSaveChanges;
        private CheckBox ckGemini;
        private CheckBox ckLMStudio;
        private GroupBox grpDomainOptions;
        private TextBox txtDomain;
        private Label lblMasterDomain;
        private Button button1;
        private CheckBox ckHttpMode;
    }
}