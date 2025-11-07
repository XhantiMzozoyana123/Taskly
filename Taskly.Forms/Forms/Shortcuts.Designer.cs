namespace Taskly.Forms.Forms
{
    partial class Shortcuts
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
            dgvShortcuts = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvShortcuts).BeginInit();
            SuspendLayout();
            // 
            // dgvShortcuts
            // 
            dgvShortcuts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvShortcuts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvShortcuts.Location = new Point(12, 12);
            dgvShortcuts.Name = "dgvShortcuts";
            dgvShortcuts.RowHeadersWidth = 51;
            dgvShortcuts.Size = new Size(1001, 407);
            dgvShortcuts.TabIndex = 0;
            // 
            // Shortcuts
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1025, 431);
            Controls.Add(dgvShortcuts);
            Name = "Shortcuts";
            Text = "Shortcuts";
            Load += Shortcuts_Load;
            ((System.ComponentModel.ISupportInitialize)dgvShortcuts).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvShortcuts;
    }
}