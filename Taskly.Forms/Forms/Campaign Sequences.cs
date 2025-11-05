using System;
using System.Linq;
using System.Windows.Forms;
using Taskly.Domain;
using Taskly.Domain.Entities;
using Taskly.Infrastructure.Services;

namespace Taskly.Forms.Forms
{
    public partial class Campaign_Sequences : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly Domain.Entities.Campaigns _campaigns;

        public Campaign_Sequences(ApplicationDbContext context, Domain.Entities.Campaigns campaigns)
        {
            InitializeComponent();
            _context = context;
            _campaigns = campaigns;
        }

        private void Campaign_Sequences_Load(object sender, EventArgs e)
        {
            LoadSequences();
            dgvCampaigns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCampaigns.MultiSelect = false;
            dgvCampaigns.AutoGenerateColumns = true;
        }

        // -------------------------
        // Load all sequences
        // -------------------------
        private void LoadSequences()
        {
            dgvCampaigns.DataSource = _context.CampaignSequences.ToList();
        }

        // -------------------------
        // Add new sequence
        // -------------------------
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            var sequence = new CampaignSequences
            {
                CampaignId = _campaigns.Id, // Assuming you have a NumericUpDown for CampaignId
                SequenceName = txtName.Text,
                SequenceDescription = rtxtDescription.Text,
                WaitTimeInHours = int.Parse(txtDelay.Text), // NumericUpDown for wait time
                AccountRotation = ckCookieRotation.Checked,
                Completed = false
            };

            _context.CampaignSequences.Add(sequence);
            _context.SaveChanges();
            LoadSequences();
            MessageBox.Show("Sequence added successfully!");
        }

        // -------------------------
        // Update selected sequence
        // -------------------------
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                int id = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;
                var sequence = _context.CampaignSequences.Find(id);

                if (sequence != null)
                {
                    // Update fields directly from DataGridView
                    sequence.CampaignId = _campaigns.Id;
                    sequence.SequenceName = dgvCampaigns.CurrentRow.Cells["SequenceName"].Value?.ToString();
                    sequence.SequenceDescription = dgvCampaigns.CurrentRow.Cells["SequenceDescription"].Value?.ToString();
                    sequence.WaitTimeInHours = Convert.ToInt32(dgvCampaigns.CurrentRow.Cells["WaitTimeInHours"].Value);
                    sequence.AccountRotation = Convert.ToBoolean(dgvCampaigns.CurrentRow.Cells["AccountRotation"].Value);
                    sequence.Completed = Convert.ToBoolean(dgvCampaigns.CurrentRow.Cells["Completed"].Value);

                    _context.SaveChanges();
                    LoadSequences();
                    MessageBox.Show("Sequence updated successfully!");
                }
            }
            else
            {
                MessageBox.Show("Please select a sequence to update.");
            }
        }

        // -------------------------
        // Delete selected sequence
        // -------------------------
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                int id = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;
                var sequence = _context.CampaignSequences.Find(id);

                if (sequence != null)
                {
                    _context.CampaignSequences.Remove(sequence);
                    _context.SaveChanges();
                    LoadSequences();
                    MessageBox.Show("Sequence deleted successfully!");
                }
            }
            else
            {
                MessageBox.Show("Please select a sequence to delete.");
            }
        }

        // -------------------------
        // Delete all sequences
        // -------------------------
        private void btnDeleteALL_Click(object sender, EventArgs e)
        {
            var allSequences = _context.CampaignSequences.ToList();
            if (allSequences.Count > 0)
            {
                _context.CampaignSequences.RemoveRange(allSequences);
                _context.SaveChanges();
                LoadSequences();
                MessageBox.Show("All sequences deleted!");
            }
            else
            {
                MessageBox.Show("No sequences to delete.");
            }
        }

        // -------------------------
        // Optional: manage messages
        // -------------------------
        private void btnMessages_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                int selectedSequenceId = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;
                var sequence = _context.CampaignSequences.Find(selectedSequenceId);

                if (sequence != null)
                {
                    // Open child form for CampaignContent
                    var contentForm = new Campaign_Messages(_context, sequence);
                    contentForm.ShowDialog(); // modal
                }
                else
                {
                    MessageBox.Show("Selected campaign could not be found.");
                }
            }
            else
            {
                MessageBox.Show("Please select a campaign first.");
            }
        }
    }
}
