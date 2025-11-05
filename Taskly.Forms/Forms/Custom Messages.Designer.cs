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
            lstResult = new ListBox();
            rtxtMessage = new RichTextBox();
            btnDeleteALL = new Button();
            btnUpdate = new Button();
            btnDelete = new Button();
            btnRun = new Button();
            rtxtQuery = new RichTextBox();
            SuspendLayout();
            // 
            // lstResult
            // 
            lstResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstResult.FormattingEnabled = true;
            lstResult.Location = new Point(12, 146);
            lstResult.Name = "lstResult";
            lstResult.Size = new Size(1072, 204);
            lstResult.TabIndex = 0;
            lstResult.SelectedIndexChanged += lstResult_SelectedIndexChanged;
            // 
            // rtxtMessage
            // 
            rtxtMessage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtxtMessage.Location = new Point(12, 356);
            rtxtMessage.Name = "rtxtMessage";
            rtxtMessage.Size = new Size(1072, 300);
            rtxtMessage.TabIndex = 1;
            rtxtMessage.Text = "";
            // 
            // btnDeleteALL
            // 
            btnDeleteALL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteALL.Location = new Point(12, 662);
            btnDeleteALL.Name = "btnDeleteALL";
            btnDeleteALL.Size = new Size(128, 43);
            btnDeleteALL.TabIndex = 2;
            btnDeleteALL.Text = "Delete ALL";
            btnDeleteALL.UseVisualStyleBackColor = true;
            btnDeleteALL.Click += btnDeleteALL_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUpdate.Location = new Point(956, 662);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(128, 43);
            btnUpdate.TabIndex = 3;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(146, 662);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(128, 43);
            btnDelete.TabIndex = 5;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnRun
            // 
            btnRun.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRun.Location = new Point(956, 24);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(128, 98);
            btnRun.TabIndex = 7;
            btnRun.Text = "Run Query";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // rtxtQuery
            // 
            rtxtQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtxtQuery.Location = new Point(12, 12);
            rtxtQuery.Name = "rtxtQuery";
            rtxtQuery.Size = new Size(938, 120);
            rtxtQuery.TabIndex = 8;
            rtxtQuery.Text = "";
            // 
            // Custom_Messages
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1096, 717);
            Controls.Add(rtxtQuery);
            Controls.Add(btnRun);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(btnDeleteALL);
            Controls.Add(rtxtMessage);
            Controls.Add(lstResult);
            Name = "Custom_Messages";
            Text = "Custom_Messages";
            Load += Custom_Messages_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListBox lstResult;
        private RichTextBox rtxtMessage;
        private Button btnDeleteALL;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRun;
        private RichTextBox rtxtQuery;
    }
}