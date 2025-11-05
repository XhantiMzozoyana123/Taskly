using System;
using System.Linq;
using System.Windows.Forms;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Forms.Forms
{
    public partial class Campaign_Contents : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly CampaignMessages _parentMessage;

        public Campaign_Contents(ApplicationDbContext context, CampaignMessages message)
        {
            InitializeComponent();
            _context = context;
            _parentMessage = message;
        }

        private void Campaign_Contents_Load(object sender, EventArgs e)
        {
            LoadContents();

            dgvContent.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvContent.MultiSelect = false;
            dgvContent.AutoGenerateColumns = true;
        }

        // -------------------------
        // Load all child contents
        // -------------------------
        private void LoadContents()
        {
            dgvContent.DataSource = _context.CampaignContents
                .Where(c => c.CampaignMessageId == _parentMessage.Id)
                .Select(c => new
                {
                    c.Id,
                    c.MessageText,
                    c.Replied
                })
                .ToList();
        }

        // -------------------------
        // Update selected content
        // -------------------------
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvContent.CurrentRow != null)
            {
                int id = (int)dgvContent.CurrentRow.Cells["Id"].Value;
                var content = _context.CampaignContents.Find(id);

                if (content != null)
                {
                    content.MessageText = rtxtDescription.Text.Trim();

                    _context.SaveChanges();
                    LoadContents();
                    MessageBox.Show("Content updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a content row to update.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // -------------------------
        // Delete selected content
        // -------------------------
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvContent.CurrentRow != null)
            {
                int id = (int)dgvContent.CurrentRow.Cells["Id"].Value;
                var content = _context.CampaignContents.Find(id);

                if (content != null)
                {
                    _context.CampaignContents.Remove(content);
                    _context.SaveChanges();
                    LoadContents();
                    MessageBox.Show("Content deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a content to delete.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // -------------------------
        // Delete all contents for this message
        // -------------------------
        private void btnDeleteALL_Click(object sender, EventArgs e)
        {
            var allContents = _context.CampaignContents.Where(c => c.CampaignMessageId == _parentMessage.Id).ToList();
            if (allContents.Count > 0)
            {
                _context.CampaignContents.RemoveRange(allContents);
                _context.SaveChanges();
                LoadContents();
                MessageBox.Show("All contents deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No content to delete.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // -------------------------
        // Load selected content into editor
        // -------------------------
        private void dgvContents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvContent.CurrentRow != null)
            {
                rtxtDescription.Text = dgvContent.CurrentRow.Cells["MessageText"].Value?.ToString() ?? string.Empty;
            }
        }


        // -------------------------
        // Add new content
        // -------------------------

        private void btnMessages_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(rtxtDescription.Text))
            {
                MessageBox.Show("Please enter message text before adding.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var content = new CampaignContent
            {
                CampaignMessageId = _parentMessage.Id,
                MessageText = rtxtDescription.Text.Trim(),
            };

            _context.CampaignContents.Add(content);
            _context.SaveChanges();

            LoadContents();
            rtxtDescription.Clear();
            MessageBox.Show("Content added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
