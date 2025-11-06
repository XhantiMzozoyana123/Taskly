namespace Taskly.Forms.Forms
{
    partial class Domains
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
            lblDomain = new Label();
            txtDomain = new TextBox();
            btnAdd = new Button();
            dgvDomains = new DataGridView();
            btnDelete = new Button();
            btnUpdate = new Button();
            btnDeleteALL = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvDomains).BeginInit();
            SuspendLayout();
            // 
            // lblDomain
            // 
            lblDomain.AutoSize = true;
            lblDomain.Location = new Point(12, 25);
            lblDomain.Name = "lblDomain";
            lblDomain.Size = new Size(102, 20);
            lblDomain.TabIndex = 0;
            lblDomain.Text = "Domain (URL)";
            // 
            // txtDomain
            // 
            txtDomain.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtDomain.Location = new Point(141, 22);
            txtDomain.Name = "txtDomain";
            txtDomain.Size = new Size(761, 27);
            txtDomain.TabIndex = 1;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(928, 16);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(142, 39);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "Add Domain";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // dgvDomains
            // 
            dgvDomains.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDomains.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDomains.Location = new Point(12, 65);
            dgvDomains.Name = "dgvDomains";
            dgvDomains.RowHeadersWidth = 51;
            dgvDomains.Size = new Size(1058, 460);
            dgvDomains.TabIndex = 3;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(12, 539);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(142, 39);
            btnDelete.TabIndex = 4;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUpdate.Location = new Point(928, 539);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(142, 39);
            btnUpdate.TabIndex = 5;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDeleteALL
            // 
            btnDeleteALL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteALL.Location = new Point(160, 539);
            btnDeleteALL.Name = "btnDeleteALL";
            btnDeleteALL.Size = new Size(142, 39);
            btnDeleteALL.TabIndex = 6;
            btnDeleteALL.Text = "Delete ALL";
            btnDeleteALL.UseVisualStyleBackColor = true;
            btnDeleteALL.Click += btnDeleteALL_Click;
            // 
            // Domains
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1082, 590);
            Controls.Add(btnDeleteALL);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(dgvDomains);
            Controls.Add(btnAdd);
            Controls.Add(txtDomain);
            Controls.Add(lblDomain);
            Name = "Domains";
            Text = "Domains";
            Load += Domains_Load;
            ((System.ComponentModel.ISupportInitialize)dgvDomains).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblDomain;
        private TextBox txtDomain;
        private Button btnAdd;
        private DataGridView dgvDomains;
        private Button btnDelete;
        private Button btnUpdate;
        private Button btnDeleteALL;
    }
}