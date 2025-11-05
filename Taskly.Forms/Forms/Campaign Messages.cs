using System;
using System.Linq;
using System.Windows.Forms;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Forms.Forms
{
    public partial class Campaign_Messages : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly CampaignSequences _campaignSequence;

        public Campaign_Messages(ApplicationDbContext context, CampaignSequences campaignSequence)
        {
            InitializeComponent();
            _context = context;
            _campaignSequence = campaignSequence;
        }

        private void Campaign_Messages_Load(object sender, EventArgs e)
        {
            LoadMessages();
            dgvCampaigns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCampaigns.MultiSelect = false;
            dgvCampaigns.AutoGenerateColumns = true;
        }

        // -------------------------
        // Load all messages
        // -------------------------
        private void LoadMessages()
        {
            dgvCampaigns.DataSource = _context.CampaignMessages.ToList();
        }

        // -------------------------
        // Add new message
        // -------------------------
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            var message = new CampaignMessages
            {
                CampaignSequenceId = _campaignSequence.Id,
                WaitTimeInMinutes = int.Parse(txtDelay.Text),
                MessageRotation = ckMessageRotation.Checked
            };

            _context.CampaignMessages.Add(message);
            _context.SaveChanges();
            LoadMessages();
            MessageBox.Show("Message added successfully!");
        }

        // -------------------------
        // Update selected message
        // -------------------------
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                int id = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;
                var message = _context.CampaignMessages.Find(id);

                if (message != null)
                {
                    message.CampaignSequenceId = Convert.ToInt32(dgvCampaigns.CurrentRow.Cells["CampaignSequenceId"].Value);
                    message.WaitTimeInMinutes = Convert.ToInt32(dgvCampaigns.CurrentRow.Cells["WaitTimeInMinutes"].Value);
                    message.MessageRotation = Convert.ToBoolean(dgvCampaigns.CurrentRow.Cells["MessageRotation"].Value);

                    _context.SaveChanges();
                    LoadMessages();
                    MessageBox.Show("Message updated successfully!");
                }
            }
            else
            {
                MessageBox.Show("Please select a message to update.");
            }
        }

        // -------------------------
        // Delete selected message
        // -------------------------
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                int id = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;
                var message = _context.CampaignMessages.Find(id);

                if (message != null)
                {
                    _context.CampaignMessages.Remove(message);
                    _context.SaveChanges();
                    LoadMessages();
                    MessageBox.Show("Message deleted successfully!");
                }
            }
            else
            {
                MessageBox.Show("Please select a message to delete.");
            }
        }

        // -------------------------
        // Delete all messages
        // -------------------------
        private void btnDeleteALL_Click(object sender, EventArgs e)
        {
            var allMessages = _context.CampaignMessages.ToList();
            if (allMessages.Count > 0)
            {
                _context.CampaignMessages.RemoveRange(allMessages);
                _context.SaveChanges();
                LoadMessages();
                MessageBox.Show("All messages deleted!");
            }
            else
            {
                MessageBox.Show("No messages to delete.");
            }
        }

        // -------------------------
        // Manage child content
        // -------------------------
        private void btnManageContent_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                int selectedMessageId = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;
                var message = _context.CampaignMessages.Find(selectedMessageId);

                if (message != null)
                {
                    // Open child form for CampaignContent
                    var contentForm = new Campaign_Contents(_context, message);
                    contentForm.ShowDialog(); // modal
                }
                else
                {
                    MessageBox.Show("Selected message could not be found.");
                }
            }
            else
            {
                MessageBox.Show("Please select a message first.");
            }
        }
    }
}
