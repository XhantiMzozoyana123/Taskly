namespace Taskly.Forms.Forms
{
    partial class Campaign_Contents
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
            grpCampaign = new GroupBox();
            rtxtDescription = new RichTextBox();
            btnMessages = new Button();
            btnDeleteALL = new Button();
            btnDelete = new Button();
            btnUpdate = new Button();
            dgvContent = new DataGridView();
            grpCampaign.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvContent).BeginInit();
            SuspendLayout();
            // 
            // grpCampaign
            // 
            grpCampaign.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpCampaign.Controls.Add(rtxtDescription);
            grpCampaign.Location = new Point(916, 12);
            grpCampaign.Name = "grpCampaign";
            grpCampaign.Size = new Size(478, 633);
            grpCampaign.TabIndex = 23;
            grpCampaign.TabStop = false;
            grpCampaign.Text = "Message";
            // 
            // rtxtDescription
            // 
            rtxtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtxtDescription.Location = new Point(17, 48);
            rtxtDescription.Name = "rtxtDescription";
            rtxtDescription.Size = new Size(442, 568);
            rtxtDescription.TabIndex = 13;
            rtxtDescription.Text = "";
            // 
            // btnMessages
            // 
            btnMessages.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnMessages.Location = new Point(1220, 651);
            btnMessages.Name = "btnMessages";
            btnMessages.Size = new Size(174, 46);
            btnMessages.TabIndex = 22;
            btnMessages.Text = "Add Messages";
            btnMessages.UseVisualStyleBackColor = true;
            btnMessages.Click += btnMessages_Click;
            // 
            // btnDeleteALL
            // 
            btnDeleteALL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteALL.Location = new Point(300, 651);
            btnDeleteALL.Name = "btnDeleteALL";
            btnDeleteALL.Size = new Size(138, 46);
            btnDeleteALL.TabIndex = 21;
            btnDeleteALL.Text = "Delete ALL";
            btnDeleteALL.UseVisualStyleBackColor = true;
            btnDeleteALL.Click += btnDeleteALL_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(156, 651);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(138, 46);
            btnDelete.TabIndex = 20;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpdate.Location = new Point(12, 651);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(138, 46);
            btnUpdate.TabIndex = 19;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // dgvContent
            // 
            dgvContent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvContent.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvContent.Location = new Point(12, 12);
            dgvContent.Name = "dgvContent";
            dgvContent.RowHeadersWidth = 51;
            dgvContent.Size = new Size(898, 633);
            dgvContent.TabIndex = 18;
            // 
            // Campaign_Contents
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1406, 709);
            Controls.Add(grpCampaign);
            Controls.Add(btnMessages);
            Controls.Add(btnDeleteALL);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(dgvContent);
            Name = "Campaign_Contents";
            Text = "Campaign_Contents";
            Load += Campaign_Contents_Load;
            grpCampaign.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvContent).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpCampaign;
        private RichTextBox rtxtDescription;
        private Label lblDescription;
        private Button btnMessages;
        private Button btnDeleteALL;
        private Button btnDelete;
        private Button btnUpdate;
        private DataGridView dgvContent;
    }
}