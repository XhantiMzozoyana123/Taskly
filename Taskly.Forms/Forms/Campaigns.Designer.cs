namespace Taskly.Forms.Forms
{
    partial class Campaigns
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
            dgvCampaigns = new DataGridView();
            btnUpdate = new Button();
            btnDelete = new Button();
            btnDeleteALL = new Button();
            btnSequence = new Button();
            grpCampaign = new GroupBox();
            lblEndDate = new Label();
            dtEndDate = new DateTimePicker();
            lblStartDate = new Label();
            dtStartDate = new DateTimePicker();
            rtxtDescription = new RichTextBox();
            lblDescription = new Label();
            txtName = new TextBox();
            lblName = new Label();
            btnSubmit = new Button();
            btnRunCampaign = new Button();
            btnPauseCampaign = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvCampaigns).BeginInit();
            grpCampaign.SuspendLayout();
            SuspendLayout();
            // 
            // dgvCampaigns
            // 
            dgvCampaigns.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvCampaigns.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCampaigns.Location = new Point(12, 12);
            dgvCampaigns.Name = "dgvCampaigns";
            dgvCampaigns.RowHeadersWidth = 51;
            dgvCampaigns.Size = new Size(884, 613);
            dgvCampaigns.TabIndex = 0;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpdate.Location = new Point(12, 631);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(106, 46);
            btnUpdate.TabIndex = 1;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(124, 631);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(106, 46);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnDeleteALL
            // 
            btnDeleteALL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteALL.Location = new Point(236, 631);
            btnDeleteALL.Name = "btnDeleteALL";
            btnDeleteALL.Size = new Size(112, 46);
            btnDeleteALL.TabIndex = 3;
            btnDeleteALL.Text = "Delete ALL";
            btnDeleteALL.UseVisualStyleBackColor = true;
            btnDeleteALL.Click += btnDeleteALL_Click;
            // 
            // btnSequence
            // 
            btnSequence.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSequence.Location = new Point(1014, 631);
            btnSequence.Name = "btnSequence";
            btnSequence.Size = new Size(174, 46);
            btnSequence.TabIndex = 4;
            btnSequence.Text = "Add Sequence";
            btnSequence.UseVisualStyleBackColor = true;
            btnSequence.Click += btnSequence_Click;
            // 
            // grpCampaign
            // 
            grpCampaign.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpCampaign.Controls.Add(lblEndDate);
            grpCampaign.Controls.Add(dtEndDate);
            grpCampaign.Controls.Add(lblStartDate);
            grpCampaign.Controls.Add(dtStartDate);
            grpCampaign.Controls.Add(rtxtDescription);
            grpCampaign.Controls.Add(lblDescription);
            grpCampaign.Controls.Add(txtName);
            grpCampaign.Controls.Add(lblName);
            grpCampaign.Location = new Point(902, 12);
            grpCampaign.Name = "grpCampaign";
            grpCampaign.Size = new Size(468, 613);
            grpCampaign.TabIndex = 7;
            grpCampaign.TabStop = false;
            grpCampaign.Text = "Add New Campaign";
            // 
            // lblEndDate
            // 
            lblEndDate.AutoSize = true;
            lblEndDate.Location = new Point(15, 547);
            lblEndDate.Name = "lblEndDate";
            lblEndDate.Size = new Size(70, 20);
            lblEndDate.TabIndex = 17;
            lblEndDate.Text = "End Date";
            // 
            // dtEndDate
            // 
            dtEndDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtEndDate.Location = new Point(131, 542);
            dtEndDate.Name = "dtEndDate";
            dtEndDate.Size = new Size(318, 27);
            dtEndDate.TabIndex = 16;
            // 
            // lblStartDate
            // 
            lblStartDate.AutoSize = true;
            lblStartDate.Location = new Point(15, 514);
            lblStartDate.Name = "lblStartDate";
            lblStartDate.Size = new Size(76, 20);
            lblStartDate.TabIndex = 15;
            lblStartDate.Text = "Start Date";
            // 
            // dtStartDate
            // 
            dtStartDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtStartDate.Location = new Point(131, 509);
            dtStartDate.Name = "dtStartDate";
            dtStartDate.Size = new Size(318, 27);
            dtStartDate.TabIndex = 14;
            // 
            // rtxtDescription
            // 
            rtxtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtxtDescription.Location = new Point(131, 74);
            rtxtDescription.Name = "rtxtDescription";
            rtxtDescription.Size = new Size(318, 429);
            rtxtDescription.TabIndex = 13;
            rtxtDescription.Text = "";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(15, 74);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(85, 20);
            lblDescription.TabIndex = 12;
            lblDescription.Text = "Description";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(131, 38);
            txtName.Name = "txtName";
            txtName.Size = new Size(318, 27);
            txtName.TabIndex = 11;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(15, 41);
            lblName.Name = "lblName";
            lblName.Size = new Size(49, 20);
            lblName.TabIndex = 10;
            lblName.Text = "Name";
            // 
            // btnSubmit
            // 
            btnSubmit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSubmit.Location = new Point(1194, 631);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(176, 46);
            btnSubmit.TabIndex = 9;
            btnSubmit.Text = "Submit Campaign";
            btnSubmit.UseVisualStyleBackColor = true;
            btnSubmit.Click += btnSubmit_Click;
            // 
            // btnRunCampaign
            // 
            btnRunCampaign.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRunCampaign.Location = new Point(751, 631);
            btnRunCampaign.Name = "btnRunCampaign";
            btnRunCampaign.Size = new Size(145, 46);
            btnRunCampaign.TabIndex = 10;
            btnRunCampaign.Text = "Run Campaign";
            btnRunCampaign.UseVisualStyleBackColor = true;
            btnRunCampaign.Click += btnRunCampaign_Click;
            // 
            // btnPauseCampaign
            // 
            btnPauseCampaign.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPauseCampaign.Location = new Point(548, 631);
            btnPauseCampaign.Name = "btnPauseCampaign";
            btnPauseCampaign.Size = new Size(197, 46);
            btnPauseCampaign.TabIndex = 11;
            btnPauseCampaign.Text = "Pause/Play Campaign";
            btnPauseCampaign.UseVisualStyleBackColor = true;
            btnPauseCampaign.Click += button1_Click;
            // 
            // Campaigns
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1382, 689);
            Controls.Add(btnPauseCampaign);
            Controls.Add(btnRunCampaign);
            Controls.Add(grpCampaign);
            Controls.Add(btnSequence);
            Controls.Add(btnDeleteALL);
            Controls.Add(btnSubmit);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(dgvCampaigns);
            Name = "Campaigns";
            Text = "Campaigns";
            Load += Campaigns_Load;
            ((System.ComponentModel.ISupportInitialize)dgvCampaigns).EndInit();
            grpCampaign.ResumeLayout(false);
            grpCampaign.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvCampaigns;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnDeleteALL;
        private Button btnSequence;
        private GroupBox grpCampaign;
        private Button btnSubmit;
        private TextBox textBox2;
        private Label lblDescription;
        private TextBox txtName;
        private Label lblName;
        private Label lblEndDate;
        private DateTimePicker dtEndDate;
        private Label lblStartDate;
        private DateTimePicker dtStartDate;
        private RichTextBox rtxtDescription;
        private Button btnRunCampaign;
        private Button btnPauseCampaign;
    }
}