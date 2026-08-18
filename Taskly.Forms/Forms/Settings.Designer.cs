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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            btnSaveChanges = new Button();
            ckGemini = new CheckBox();
            ckLMStudio = new CheckBox();
            grpAIOptions = new GroupBox();
            label1 = new Label();
            txtMessegingDelay = new TextBox();
            ckMessenginRandomCookieSelect = new CheckBox();
            grpSender = new GroupBox();
            btnDownloadBatch = new Button();
            grpSearch = new GroupBox();
            grpAIOptions.SuspendLayout();
            grpSender.SuspendLayout();
            grpSearch.SuspendLayout();
            SuspendLayout();
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveChanges.Location = new Point(609, 430);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(179, 46);
            btnSaveChanges.TabIndex = 4;
            btnSaveChanges.Text = "Save Changes";
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // ckGemini
            // 
            ckGemini.AutoSize = true;
            ckGemini.Location = new Point(22, 36);
            ckGemini.Name = "ckGemini";
            ckGemini.Size = new Size(340, 24);
            ckGemini.TabIndex = 5;
            ckGemini.Text = "Rotate through all API Keys when using Gemini";
            ckGemini.UseVisualStyleBackColor = true;
            // 
            // ckLMStudio
            // 
            ckLMStudio.AutoSize = true;
            ckLMStudio.Location = new Point(22, 66);
            ckLMStudio.Name = "ckLMStudio";
            ckLMStudio.Size = new Size(302, 24);
            ckLMStudio.TabIndex = 6;
            ckLMStudio.Text = "Use LM Studio for AI computing (Offline)";
            ckLMStudio.UseVisualStyleBackColor = true;
            // 
            // grpAIOptions
            // 
            grpAIOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpAIOptions.Controls.Add(ckLMStudio);
            grpAIOptions.Controls.Add(ckGemini);
            grpAIOptions.Location = new Point(12, 321);
            grpAIOptions.Name = "grpAIOptions";
            grpAIOptions.Size = new Size(776, 103);
            grpAIOptions.TabIndex = 7;
            grpAIOptions.TabStop = false;
            grpAIOptions.Text = "AI";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 41);
            label1.Name = "label1";
            label1.Size = new Size(86, 20);
            label1.TabIndex = 0;
            label1.Text = "Delay (Min)";
            // 
            // txtMessegingDelay
            // 
            txtMessegingDelay.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMessegingDelay.Location = new Point(139, 38);
            txtMessegingDelay.Name = "txtMessegingDelay";
            txtMessegingDelay.Size = new Size(614, 27);
            txtMessegingDelay.TabIndex = 1;
            // 
            // ckMessenginRandomCookieSelect
            // 
            ckMessenginRandomCookieSelect.AutoSize = true;
            ckMessenginRandomCookieSelect.Location = new Point(22, 113);
            ckMessenginRandomCookieSelect.Name = "ckMessenginRandomCookieSelect";
            ckMessenginRandomCookieSelect.Size = new Size(511, 24);
            ckMessenginRandomCookieSelect.TabIndex = 4;
            ckMessenginRandomCookieSelect.Text = "Randomly select cookies when sending messages to associated accounts";
            ckMessenginRandomCookieSelect.UseVisualStyleBackColor = true;
            // 
            // grpSender
            // 
            grpSender.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSender.Controls.Add(ckMessenginRandomCookieSelect);
            grpSender.Controls.Add(txtMessegingDelay);
            grpSender.Controls.Add(label1);
            grpSender.Location = new Point(12, 157);
            grpSender.Name = "grpSender";
            grpSender.Size = new Size(776, 158);
            grpSender.TabIndex = 2;
            grpSender.TabStop = false;
            grpSender.Text = "Messaging";
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
            // grpSearch
            // 
            grpSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSearch.Controls.Add(btnDownloadBatch);
            grpSearch.Location = new Point(12, 12);
            grpSearch.Name = "grpSearch";
            grpSearch.Size = new Size(776, 139);
            grpSearch.TabIndex = 0;
            grpSearch.TabStop = false;
            grpSearch.Text = "Extracting";
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 491);
            Controls.Add(btnSaveChanges);
            Controls.Add(grpAIOptions);
            Controls.Add(grpSender);
            Controls.Add(grpSearch);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Settings";
            Text = "Settings";
            Load += Settings_Load_1;
            grpAIOptions.ResumeLayout(false);
            grpAIOptions.PerformLayout();
            grpSender.ResumeLayout(false);
            grpSender.PerformLayout();
            grpSearch.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btnSaveChanges;
        private CheckBox ckGemini;
        private CheckBox ckLMStudio;
        private GroupBox grpAIOptions;
        private Label label1;
        private TextBox txtMessegingDelay;
        private CheckBox ckMessenginRandomCookieSelect;
        private GroupBox grpSender;
        private Button btnDownloadBatch;
        private GroupBox grpSearch;
    }
}