namespace Taskly.Forms.Forms
{
    partial class Cookies
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
            dgvCookies = new DataGridView();
            btnUpload = new Button();
            btnDeleteALL = new Button();
            btnDelete = new Button();
            btnUpdate = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvCookies).BeginInit();
            SuspendLayout();
            // 
            // dgvCookies
            // 
            dgvCookies.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvCookies.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCookies.Location = new Point(12, 12);
            dgvCookies.Name = "dgvCookies";
            dgvCookies.RowHeadersWidth = 51;
            dgvCookies.Size = new Size(1034, 506);
            dgvCookies.TabIndex = 0;
            // 
            // btnUpload
            // 
            btnUpload.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpload.Location = new Point(12, 524);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(247, 49);
            btnUpload.TabIndex = 1;
            btnUpload.Text = "Upload JSON (Cookie Data)";
            btnUpload.UseVisualStyleBackColor = true;
            btnUpload.Click += btnUpload_Click;
            // 
            // btnDeleteALL
            // 
            btnDeleteALL.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDeleteALL.Location = new Point(879, 524);
            btnDeleteALL.Name = "btnDeleteALL";
            btnDeleteALL.Size = new Size(167, 49);
            btnDeleteALL.TabIndex = 2;
            btnDeleteALL.Text = "Delete ALL";
            btnDeleteALL.UseVisualStyleBackColor = true;
            btnDeleteALL.Click += btnDeleteALL_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDelete.Location = new Point(706, 524);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(167, 49);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUpdate.Location = new Point(533, 524);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(167, 49);
            btnUpdate.TabIndex = 4;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // Cookies
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1057, 582);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(btnDeleteALL);
            Controls.Add(btnUpload);
            Controls.Add(dgvCookies);
            Name = "Cookies";
            Text = "Cookies";
            Load += Cookies_Load;
            ((System.ComponentModel.ISupportInitialize)dgvCookies).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvCookies;
        private Button btnUpload;
        private Button btnDeleteALL;
        private Button btnDelete;
        private Button btnUpdate;
    }
}