namespace Taskly.Forms.Forms
{
    partial class Templates
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
            rtxtMessage = new RichTextBox();
            lblName = new Label();
            cboName = new ComboBox();
            btnUpdate = new Button();
            btnDeleteAll = new Button();
            btnDelete = new Button();
            btnAdd = new Button();
            SuspendLayout();
            // 
            // rtxtMessage
            // 
            rtxtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtxtMessage.Location = new Point(12, 61);
            rtxtMessage.Name = "rtxtMessage";
            rtxtMessage.Size = new Size(853, 551);
            rtxtMessage.TabIndex = 0;
            rtxtMessage.Text = "";
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(12, 22);
            lblName.Name = "lblName";
            lblName.Size = new Size(49, 20);
            lblName.TabIndex = 1;
            lblName.Text = "Name";
            // 
            // cboName
            // 
            cboName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboName.FormattingEnabled = true;
            cboName.Location = new Point(65, 19);
            cboName.Name = "cboName";
            cboName.Size = new Size(671, 28);
            cboName.TabIndex = 2;
            cboName.SelectedIndexChanged += cboName_SelectedIndexChanged;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUpdate.Location = new Point(740, 629);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(123, 53);
            btnUpdate.TabIndex = 3;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDeleteAll
            // 
            btnDeleteAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteAll.Location = new Point(12, 629);
            btnDeleteAll.Name = "btnDeleteAll";
            btnDeleteAll.Size = new Size(123, 53);
            btnDeleteAll.TabIndex = 4;
            btnDeleteAll.Text = "Delete ALL";
            btnDeleteAll.UseVisualStyleBackColor = true;
            btnDeleteAll.Click += btnDeleteAll_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(141, 629);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(123, 53);
            btnDelete.TabIndex = 5;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(742, 15);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(123, 35);
            btnAdd.TabIndex = 6;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // Templates
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(877, 694);
            Controls.Add(btnAdd);
            Controls.Add(btnDelete);
            Controls.Add(btnDeleteAll);
            Controls.Add(btnUpdate);
            Controls.Add(cboName);
            Controls.Add(lblName);
            Controls.Add(rtxtMessage);
            Name = "Templates";
            Text = "Templates";
            Load += Templates_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox rtxtMessage;
        private Label lblName;
        private ComboBox cboName;
        private Button btnUpdate;
        private Button btnDeleteAll;
        private Button btnDelete;
        private Button btnAdd;
    }
}