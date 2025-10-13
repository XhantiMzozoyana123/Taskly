using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace Taskly.Forms.Forms
{
    public partial class Taskly : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly IExtractService _extractService;

        public Taskly(ApplicationDbContext context,
            IExtractService extractService)
        {
            _context = context;
            _extractService = extractService;

            InitializeComponent();
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Search started. This may take a while.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            try
            {
                SearchDto searchDto = new SearchDto
                {
                    Keyword = txtKeywords.Text,
                    Query = rtxtQuery.Text,
                    PageNumber = int.Parse(txtPages.Text),
                    Platform = cboPlatform.Text
                };

                await _extractService.ExtractAsync(searchDto);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MessageBox.Show("Search completed.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Taskly_Load(object sender, EventArgs e)
        {
            LoadPlatform();
        }

        private void LoadPlatform()
        {
            cboPlatform.Items.Add("All Platforms");
            cboPlatform.Items.Add("Facebook");
            cboPlatform.Items.Add("Instagram");
            cboPlatform.Items.Add("Twitter");
            cboPlatform.Items.Add("Reddit");
            cboPlatform.Items.Add("TikTok");

            cboPlatform.SelectedIndex = 0;
        }

        private void accountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accounts = new Forms.Accounts(_context);
            accounts.Show();
        }
    }
}