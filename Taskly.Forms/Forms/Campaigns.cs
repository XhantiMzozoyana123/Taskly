using System;
using System.Linq;
using System.Windows.Forms;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Forms.Forms
{
    public partial class Campaigns : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly ICampaignService _campaignService;
        public Campaigns(ApplicationDbContext context, ICampaignService campaignService)
        {
            InitializeComponent();
            _context = context;
            _campaignService = campaignService;
        }

        private void Campaigns_Load(object sender, EventArgs e)
        {
            LoadCampaigns();
            dgvCampaigns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCampaigns.MultiSelect = false;
            dgvCampaigns.AutoGenerateColumns = true;
        }

        // -------------------------
        // Load all campaigns
        // -------------------------
        private void LoadCampaigns()
        {
            dgvCampaigns.DataSource = _context.Campaigns.ToList();
        }

        // -------------------------
        // Add new campaign
        // -------------------------
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            var campaign = new Domain.Entities.Campaigns
            {
                Name = txtName.Text,
                Description = rtxtDescription.Text,
                StartDate = dtStartDate.Value,
                EndDate = dtEndDate.Value,
                Status = "Active"
            };

            _context.Campaigns.Add(campaign);
            _context.SaveChanges();
            LoadCampaigns();
            MessageBox.Show("Campaign created successfully!");
        }

        // -------------------------
        // Update selected row in DataGridView
        // -------------------------
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                int id = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;
                var campaign = _context.Campaigns.Find(id);
                if (campaign != null)
                {
                    // Get updated values directly from DataGridView cells
                    campaign.Name = dgvCampaigns.CurrentRow.Cells["Name"].Value?.ToString();
                    campaign.Description = dgvCampaigns.CurrentRow.Cells["Description"].Value?.ToString();

                    if (DateTime.TryParse(dgvCampaigns.CurrentRow.Cells["StartDate"].Value?.ToString(), out var start))
                        campaign.StartDate = start;

                    if (DateTime.TryParse(dgvCampaigns.CurrentRow.Cells["EndDate"].Value?.ToString(), out var end))
                        campaign.EndDate = end;

                    campaign.Status = dgvCampaigns.CurrentRow.Cells["Status"].Value?.ToString();

                    _context.SaveChanges();
                    LoadCampaigns();
                    MessageBox.Show("Campaign updated successfully!");
                }
            }
        }

        // -------------------------
        // Delete selected row in DataGridView
        // -------------------------
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                int id = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;
                var campaign = _context.Campaigns.Find(id);
                if (campaign != null)
                {
                    _context.Campaigns.Remove(campaign);
                    _context.SaveChanges();
                    LoadCampaigns();
                    MessageBox.Show("Campaign deleted successfully!");
                }
            }
        }

        // -------------------------
        // Delete all campaigns
        // -------------------------
        private void btnDeleteALL_Click(object sender, EventArgs e)
        {
            var allCampaigns = _context.Campaigns.ToList();
            if (allCampaigns.Count > 0)
            {
                _context.Campaigns.RemoveRange(allCampaigns);
                _context.SaveChanges();
                LoadCampaigns();
                MessageBox.Show("All campaigns deleted!");
            }
        }

        // -------------------------
        // Optional: sequence button
        // -------------------------
        private void btnSequence_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                int selectedCampaignId = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;
                var campaign = _context.Campaigns.Find(selectedCampaignId);

                if (campaign != null)
                {
                    // Open child form for CampaignContent
                    var contentForm = new Campaign_Sequences(_context, campaign);
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

        private void btnRunCampaign_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                // Get the Id of the selected campaign from the DataGridView
                int selectedId = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;

                // Retrieve the full campaign object from the DbContext
                var selectedCampaign = _context.Campaigns.Find(selectedId);

                if (selectedCampaign != null)
                {
                    // You now have the full campaign object
                    MessageBox.Show($"Running campaign: {selectedCampaign.Name}");

                    // TODO: Add your campaign execution logic here
                    _campaignService.RunCampaignsAsync(selectedCampaign);
                }
                else
                {
                    MessageBox.Show("Selected campaign could not be found.");
                }
            }
            else
            {
                MessageBox.Show("Please select a campaign to run.");
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (dgvCampaigns.CurrentRow != null)
            {
                // Get the Id of the selected campaign
                int selectedId = (int)dgvCampaigns.CurrentRow.Cells["Id"].Value;

                // Retrieve the campaign from the database
                var campaign = _context.Campaigns.Find(selectedId);

                if (campaign != null)
                {
                    // Toggle status between Active and Paused
                    if (campaign.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        await _campaignService.PauseCampaignAsync(campaign);
                        MessageBox.Show($"Campaign '{campaign.Name}' is now Paused.");
                    }
                    else if (campaign.Status.Equals("Paused", StringComparison.OrdinalIgnoreCase))
                    {
                        await _campaignService.ResumeCampaignAsync(campaign);
                        MessageBox.Show($"Campaign '{campaign.Name}' is now Active.");
                    }
                    else
                    {
                        MessageBox.Show($"Campaign '{campaign.Name}' cannot be paused or played (Status: {campaign.Status}).");
                    }
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
