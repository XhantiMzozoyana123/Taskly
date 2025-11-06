namespace Taskly.Forms.Forms
{
    partial class Campaign_Sequences
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Campaign_Sequences));
            grpCampaign = new GroupBox();
            txtName = new TextBox();
            lblName = new Label();
            ckCookieRotation = new CheckBox();
            txtDelay = new TextBox();
            lblDelay = new Label();
            rtxtDescription = new RichTextBox();
            lblDescription = new Label();
            btnMessages = new Button();
            btnDeleteALL = new Button();
            btnSubmit = new Button();
            btnDelete = new Button();
            btnUpdate = new Button();
            dgvCampaigns = new DataGridView();
            grpCampaign.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCampaigns).BeginInit();
            SuspendLayout();
            // 
            // grpCampaign
            // 
            grpCampaign.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpCampaign.Controls.Add(txtName);
            grpCampaign.Controls.Add(lblName);
            grpCampaign.Controls.Add(ckCookieRotation);
            grpCampaign.Controls.Add(txtDelay);
            grpCampaign.Controls.Add(lblDelay);
            grpCampaign.Controls.Add(rtxtDescription);
            grpCampaign.Controls.Add(lblDescription);
            grpCampaign.Location = new Point(942, 12);
            grpCampaign.Name = "grpCampaign";
            grpCampaign.Size = new Size(478, 622);
            grpCampaign.TabIndex = 15;
            grpCampaign.TabStop = false;
            grpCampaign.Text = "Add New Campaign Sequence";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(131, 44);
            txtName.Name = "txtName";
            txtName.Size = new Size(324, 27);
            txtName.TabIndex = 18;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(15, 47);
            lblName.Name = "lblName";
            lblName.Size = new Size(49, 20);
            lblName.TabIndex = 17;
            lblName.Text = "Name";
            // 
            // ckCookieRotation
            // 
            ckCookieRotation.AutoSize = true;
            ckCookieRotation.Location = new Point(15, 577);
            ckCookieRotation.Name = "ckCookieRotation";
            ckCookieRotation.Size = new Size(138, 24);
            ckCookieRotation.TabIndex = 16;
            ckCookieRotation.Text = "Cookie Rotation";
            ckCookieRotation.UseVisualStyleBackColor = true;
            // 
            // txtDelay
            // 
            txtDelay.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDelay.Location = new Point(131, 526);
            txtDelay.Name = "txtDelay";
            txtDelay.Size = new Size(324, 27);
            txtDelay.TabIndex = 15;
            // 
            // lblDelay
            // 
            lblDelay.AutoSize = true;
            lblDelay.Location = new Point(15, 533);
            lblDelay.Name = "lblDelay";
            lblDelay.Size = new Size(100, 20);
            lblDelay.TabIndex = 14;
            lblDelay.Text = "Delay (Hours)";
            // 
            // rtxtDescription
            // 
            rtxtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtxtDescription.Location = new Point(131, 80);
            rtxtDescription.Name = "rtxtDescription";
            rtxtDescription.Size = new Size(324, 429);
            rtxtDescription.TabIndex = 13;
            rtxtDescription.Text = "";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(15, 83);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(85, 20);
            lblDescription.TabIndex = 12;
            lblDescription.Text = "Description";
            // 
            // btnMessages
            // 
            btnMessages.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnMessages.Location = new Point(1064, 640);
            btnMessages.Name = "btnMessages";
            btnMessages.Size = new Size(174, 46);
            btnMessages.TabIndex = 14;
            btnMessages.Text = "Add Messages";
            btnMessages.UseVisualStyleBackColor = true;
            btnMessages.Click += btnMessages_Click;
            // 
            // btnDeleteALL
            // 
            btnDeleteALL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteALL.Location = new Point(300, 640);
            btnDeleteALL.Name = "btnDeleteALL";
            btnDeleteALL.Size = new Size(138, 46);
            btnDeleteALL.TabIndex = 13;
            btnDeleteALL.Text = "Delete ALL";
            btnDeleteALL.UseVisualStyleBackColor = true;
            btnDeleteALL.Click += btnDeleteALL_Click;
            // 
            // btnSubmit
            // 
            btnSubmit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSubmit.Location = new Point(1244, 640);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(176, 46);
            btnSubmit.TabIndex = 17;
            btnSubmit.Text = "Submit Sequence";
            btnSubmit.UseVisualStyleBackColor = true;
            btnSubmit.Click += btnSubmit_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(156, 640);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(138, 46);
            btnDelete.TabIndex = 12;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpdate.Location = new Point(12, 640);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(138, 46);
            btnUpdate.TabIndex = 11;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // dgvCampaigns
            // 
            dgvCampaigns.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvCampaigns.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCampaigns.Location = new Point(12, 12);
            dgvCampaigns.Name = "dgvCampaigns";
            dgvCampaigns.RowHeadersWidth = 51;
            dgvCampaigns.Size = new Size(924, 622);
            dgvCampaigns.TabIndex = 10;
            // 
            // Campaign_Sequences
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1432, 698);
            Controls.Add(grpCampaign);
            Controls.Add(btnMessages);
            Controls.Add(btnDeleteALL);
            Controls.Add(btnSubmit);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(dgvCampaigns);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Campaign_Sequences";
            Text = "Campaign_Sequences";
            Load += Campaign_Sequences_Load;
            grpCampaign.ResumeLayout(false);
            grpCampaign.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCampaigns).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private GroupBox grpCampaign;
        private RichTextBox rtxtDescription;
        private Label lblDescription;
        private Button btnMessages;
        private Button btnDeleteALL;
        private Button btnSubmit;
        private Button btnDelete;
        private Button btnUpdate;
        private DataGridView dgvCampaigns;
        private TextBox txtDelay;
        private Label lblDelay;
        private CheckBox ckCookieRotation;
        private TextBox txtName;
        private Label lblName;
    }
}