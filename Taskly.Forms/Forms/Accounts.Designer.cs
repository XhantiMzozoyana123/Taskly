namespace Taskly.Forms.Forms
{
    partial class Accounts
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
            lblUsername = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            lblPassword = new Label();
            grpAccount = new GroupBox();
            btnAdd = new Button();
            cboPlatform = new ComboBox();
            lblPlatform = new Label();
            dgvAccounts = new DataGridView();
            btnDeleteAll = new Button();
            btnUpdate = new Button();
            btnDelete = new Button();
            grpAccount.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAccounts).BeginInit();
            SuspendLayout();
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(17, 36);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(75, 20);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Username";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(17, 59);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(298, 27);
            txtUsername.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(17, 112);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(298, 27);
            txtPassword.TabIndex = 4;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(17, 89);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(70, 20);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "Password";
            // 
            // grpAccount
            // 
            grpAccount.Controls.Add(btnAdd);
            grpAccount.Controls.Add(cboPlatform);
            grpAccount.Controls.Add(lblPlatform);
            grpAccount.Controls.Add(lblUsername);
            grpAccount.Controls.Add(txtPassword);
            grpAccount.Controls.Add(txtUsername);
            grpAccount.Controls.Add(lblPassword);
            grpAccount.Location = new Point(12, 12);
            grpAccount.Name = "grpAccount";
            grpAccount.Size = new Size(330, 344);
            grpAccount.TabIndex = 5;
            grpAccount.TabStop = false;
            grpAccount.Text = "Add Account";
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(17, 281);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(298, 47);
            btnAdd.TabIndex = 7;
            btnAdd.Text = "Add Acccount";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // cboPlatform
            // 
            cboPlatform.FormattingEnabled = true;
            cboPlatform.Location = new Point(17, 241);
            cboPlatform.Name = "cboPlatform";
            cboPlatform.Size = new Size(298, 28);
            cboPlatform.TabIndex = 7;
            // 
            // lblPlatform
            // 
            lblPlatform.AutoSize = true;
            lblPlatform.Location = new Point(17, 218);
            lblPlatform.Name = "lblPlatform";
            lblPlatform.Size = new Size(66, 20);
            lblPlatform.TabIndex = 5;
            lblPlatform.Text = "Platform";
            // 
            // dgvAccounts
            // 
            dgvAccounts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAccounts.Location = new Point(348, 22);
            dgvAccounts.Name = "dgvAccounts";
            dgvAccounts.RowHeadersWidth = 51;
            dgvAccounts.Size = new Size(748, 334);
            dgvAccounts.TabIndex = 6;
            // 
            // btnDeleteAll
            // 
            btnDeleteAll.Location = new Point(12, 362);
            btnDeleteAll.Name = "btnDeleteAll";
            btnDeleteAll.Size = new Size(163, 47);
            btnDeleteAll.TabIndex = 8;
            btnDeleteAll.Text = "Delete All";
            btnDeleteAll.UseVisualStyleBackColor = true;
            btnDeleteAll.Click += btnDeleteAll_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(933, 362);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(163, 47);
            btnUpdate.TabIndex = 9;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(181, 363);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(163, 47);
            btnDelete.TabIndex = 10;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // Accounts
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1108, 422);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(btnDeleteAll);
            Controls.Add(dgvAccounts);
            Controls.Add(grpAccount);
            Name = "Accounts";
            Text = "Accounts";
            Load += Accounts_Load;
            grpAccount.ResumeLayout(false);
            grpAccount.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAccounts).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label lblUsername;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Label lblPassword;
        private GroupBox grpAccount;
        private DataGridView dgvAccounts;
        private Label lblPlatform;
        private ComboBox cboPlatform;
        private Button btnAdd;
        private Button btnDeleteAll;
        private Button btnUpdate;
        private Button btnDelete;
    }
}