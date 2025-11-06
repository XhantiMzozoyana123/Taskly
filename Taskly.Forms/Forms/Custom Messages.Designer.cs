namespace Taskly.Forms.Forms
{
    partial class Custom_Messages
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
            dgvData = new DataGridView();
            btnUpdate = new Button();
            btnDelete = new Button();
            btnDeleteALL = new Button();
            btnAIGenMessages = new Button();
            btnExportCSV = new Button();
            btnImportCSV = new Button();
            prgLoad = new ProgressBar();
            grpQuery = new GroupBox();
            rtxtQuery = new RichTextBox();
            lstLogs = new ListBox();
            btnClearLogs = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            grpQuery.SuspendLayout();
            SuspendLayout();
            // 
            // dgvData
            // 
            dgvData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            dgvData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Location = new Point(354, 12);
            dgvData.Name = "dgvData";
            dgvData.RowHeadersWidth = 51;
            dgvData.Size = new Size(1000, 423);
            dgvData.TabIndex = 0;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpdate.Location = new Point(12, 618);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(165, 44);
            btnUpdate.TabIndex = 1;
            btnUpdate.Text = "Update";
            btnUpdate.TextImageRelation = TextImageRelation.ImageAboveText;
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(183, 618);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(165, 44);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnDeleteALL
            // 
            btnDeleteALL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteALL.Location = new Point(354, 618);
            btnDeleteALL.Name = "btnDeleteALL";
            btnDeleteALL.Size = new Size(165, 44);
            btnDeleteALL.TabIndex = 3;
            btnDeleteALL.Text = "Delete ALL";
            btnDeleteALL.UseVisualStyleBackColor = true;
            btnDeleteALL.Click += btnDeleteALL_Click;
            // 
            // btnAIGenMessages
            // 
            btnAIGenMessages.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAIGenMessages.Location = new Point(6, 550);
            btnAIGenMessages.Name = "btnAIGenMessages";
            btnAIGenMessages.Size = new Size(324, 39);
            btnAIGenMessages.TabIndex = 4;
            btnAIGenMessages.Text = "Generate AI Messages";
            btnAIGenMessages.UseVisualStyleBackColor = true;
            btnAIGenMessages.Click += btnAIGenMessages_Click;
            // 
            // btnExportCSV
            // 
            btnExportCSV.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportCSV.Location = new Point(1189, 618);
            btnExportCSV.Name = "btnExportCSV";
            btnExportCSV.Size = new Size(165, 44);
            btnExportCSV.TabIndex = 5;
            btnExportCSV.Text = "Export CSV";
            btnExportCSV.UseVisualStyleBackColor = true;
            btnExportCSV.Click += btnExportCSV_Click;
            // 
            // btnImportCSV
            // 
            btnImportCSV.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnImportCSV.Location = new Point(1018, 618);
            btnImportCSV.Name = "btnImportCSV";
            btnImportCSV.Size = new Size(165, 44);
            btnImportCSV.TabIndex = 6;
            btnImportCSV.Text = "Import CSV";
            btnImportCSV.UseVisualStyleBackColor = true;
            btnImportCSV.Click += btnImportCSV_Click;
            // 
            // prgLoad
            // 
            prgLoad.Location = new Point(354, 441);
            prgLoad.Name = "prgLoad";
            prgLoad.Size = new Size(1000, 31);
            prgLoad.TabIndex = 7;
            // 
            // grpQuery
            // 
            grpQuery.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpQuery.Controls.Add(rtxtQuery);
            grpQuery.Controls.Add(btnAIGenMessages);
            grpQuery.Location = new Point(12, 7);
            grpQuery.Name = "grpQuery";
            grpQuery.Size = new Size(336, 595);
            grpQuery.TabIndex = 8;
            grpQuery.TabStop = false;
            grpQuery.Text = "Query";
            // 
            // rtxtQuery
            // 
            rtxtQuery.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            rtxtQuery.Location = new Point(6, 26);
            rtxtQuery.Name = "rtxtQuery";
            rtxtQuery.Size = new Size(324, 502);
            rtxtQuery.TabIndex = 0;
            rtxtQuery.Text = "";
            // 
            // lstLogs
            // 
            lstLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstLogs.FormattingEnabled = true;
            lstLogs.Location = new Point(354, 478);
            lstLogs.Name = "lstLogs";
            lstLogs.Size = new Size(829, 124);
            lstLogs.TabIndex = 9;
            // 
            // btnClearLogs
            // 
            btnClearLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClearLogs.Location = new Point(1189, 478);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new Size(165, 124);
            btnClearLogs.TabIndex = 5;
            btnClearLogs.Text = "Clear Logs";
            btnClearLogs.UseVisualStyleBackColor = true;
            btnClearLogs.Click += btnClearLogs_Click;
            // 
            // Custom_Messages
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1366, 674);
            Controls.Add(btnClearLogs);
            Controls.Add(lstLogs);
            Controls.Add(grpQuery);
            Controls.Add(prgLoad);
            Controls.Add(btnImportCSV);
            Controls.Add(btnExportCSV);
            Controls.Add(btnDeleteALL);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(dgvData);
            Name = "Custom_Messages";
            Text = "Custom_Messages";
            Load += Custom_Messages_Load;
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            grpQuery.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvData;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnDeleteALL;
        private Button btnAIGenMessages;
        private Button btnExportCSV;
        private Button btnImportCSV;
        private ProgressBar prgLoad;
        private GroupBox grpQuery;
        private RichTextBox rtxtQuery;
        private ListBox lstLogs;
        private Button btnClearLogs;
    }
}