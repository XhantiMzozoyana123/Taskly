namespace Taskly.Forms.Forms
{
    partial class Google_AI_Keys
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Google_AI_Keys));
            dgvKeys = new DataGridView();
            btnDeleteALL = new Button();
            btnDelete = new Button();
            btnUpdate = new Button();
            btnAdd = new Button();
            txtKey = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvKeys).BeginInit();
            SuspendLayout();
            // 
            // dgvKeys
            // 
            dgvKeys.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvKeys.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKeys.Location = new Point(12, 54);
            dgvKeys.Name = "dgvKeys";
            dgvKeys.RowHeadersWidth = 51;
            dgvKeys.Size = new Size(776, 338);
            dgvKeys.TabIndex = 0;
            // 
            // btnDeleteALL
            // 
            btnDeleteALL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteALL.Location = new Point(12, 398);
            btnDeleteALL.Name = "btnDeleteALL";
            btnDeleteALL.Size = new Size(116, 40);
            btnDeleteALL.TabIndex = 1;
            btnDeleteALL.Text = "Delete ALL";
            btnDeleteALL.UseVisualStyleBackColor = true;
            btnDeleteALL.Click += btnDeleteALL_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(134, 398);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(116, 40);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUpdate.Location = new Point(672, 398);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(116, 40);
            btnUpdate.TabIndex = 3;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(672, 12);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(116, 36);
            btnAdd.TabIndex = 4;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // txtKey
            // 
            txtKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtKey.Location = new Point(12, 17);
            txtKey.Name = "txtKey";
            txtKey.Size = new Size(654, 27);
            txtKey.TabIndex = 5;
            // 
            // Google_AI_Keys
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(txtKey);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(btnDeleteALL);
            Controls.Add(dgvKeys);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Google_AI_Keys";
            Text = "Google_AI_Keys";
            Load += Google_AI_Keys_Load;
            ((System.ComponentModel.ISupportInitialize)dgvKeys).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvKeys;
        private Button btnDeleteALL;
        private Button btnDelete;
        private Button btnUpdate;
        private Button btnAdd;
        private TextBox txtKey;
    }
}