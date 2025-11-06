namespace Taskly.Forms.Forms
{
    partial class Campaign_Messages
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Campaign_Messages));
            grpCampaign = new GroupBox();
            lblDelay = new Label();
            txtDelay = new TextBox();
            btnMessages = new Button();
            ckMessageRotation = new CheckBox();
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
            grpCampaign.Controls.Add(lblDelay);
            grpCampaign.Controls.Add(txtDelay);
            grpCampaign.Controls.Add(btnMessages);
            grpCampaign.Controls.Add(ckMessageRotation);
            grpCampaign.Location = new Point(1133, 12);
            grpCampaign.Name = "grpCampaign";
            grpCampaign.Size = new Size(243, 248);
            grpCampaign.TabIndex = 23;
            grpCampaign.TabStop = false;
            grpCampaign.Text = "Add New Campaign Sequence";
            // 
            // lblDelay
            // 
            lblDelay.AutoSize = true;
            lblDelay.Location = new Point(16, 115);
            lblDelay.Name = "lblDelay";
            lblDelay.Size = new Size(86, 20);
            lblDelay.TabIndex = 24;
            lblDelay.Text = "Delay (Min)";
            // 
            // txtDelay
            // 
            txtDelay.Location = new Point(16, 138);
            txtDelay.Name = "txtDelay";
            txtDelay.Size = new Size(203, 27);
            txtDelay.TabIndex = 23;
            // 
            // btnMessages
            // 
            btnMessages.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnMessages.Location = new Point(16, 175);
            btnMessages.Name = "btnMessages";
            btnMessages.Size = new Size(203, 46);
            btnMessages.TabIndex = 22;
            btnMessages.Text = "Add New Content";
            btnMessages.UseVisualStyleBackColor = true;
            btnMessages.Click += btnManageContent_Click;
            // 
            // ckMessageRotation
            // 
            ckMessageRotation.AutoSize = true;
            ckMessageRotation.Location = new Point(16, 66);
            ckMessageRotation.Name = "ckMessageRotation";
            ckMessageRotation.Size = new Size(150, 24);
            ckMessageRotation.TabIndex = 16;
            ckMessageRotation.Text = "Message Rotation";
            ckMessageRotation.UseVisualStyleBackColor = true;
            // 
            // btnDeleteALL
            // 
            btnDeleteALL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteALL.Location = new Point(300, 569);
            btnDeleteALL.Name = "btnDeleteALL";
            btnDeleteALL.Size = new Size(138, 46);
            btnDeleteALL.TabIndex = 21;
            btnDeleteALL.Text = "Delete ALL";
            btnDeleteALL.UseVisualStyleBackColor = true;
            // 
            // btnSubmit
            // 
            btnSubmit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSubmit.Location = new Point(1200, 573);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(176, 46);
            btnSubmit.TabIndex = 24;
            btnSubmit.Text = "Submit Sequence";
            btnSubmit.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(156, 569);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(138, 46);
            btnDelete.TabIndex = 20;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpdate.Location = new Point(12, 569);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(138, 46);
            btnUpdate.TabIndex = 19;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // dgvCampaigns
            // 
            dgvCampaigns.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvCampaigns.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCampaigns.Location = new Point(12, 12);
            dgvCampaigns.Name = "dgvCampaigns";
            dgvCampaigns.RowHeadersWidth = 51;
            dgvCampaigns.Size = new Size(1115, 551);
            dgvCampaigns.TabIndex = 18;
            // 
            // Campaign_Messages
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1388, 631);
            Controls.Add(grpCampaign);
            Controls.Add(btnDeleteALL);
            Controls.Add(btnSubmit);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(dgvCampaigns);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Campaign_Messages";
            Text = "Campaign_Messages";
            Load += Campaign_Messages_Load;
            grpCampaign.ResumeLayout(false);
            grpCampaign.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCampaigns).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpCampaign;
        private Button btnMessages;
        private CheckBox ckMessageRotation;
        private Button btnDeleteALL;
        private Button btnSubmit;
        private Button btnDelete;
        private Button btnUpdate;
        private DataGridView dgvCampaigns;
        private Label lblDelay;
        private TextBox txtDelay;
    }
}