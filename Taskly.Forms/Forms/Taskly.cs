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
        private readonly ISenderService _senderService;

        private List<string> messageSequence = new List<string>();

        public Taskly(ApplicationDbContext context,
            IExtractService extractService,
            ISenderService senderService)
        {
            InitializeComponent();

            _context = context;
            _extractService = extractService;
            _senderService = senderService;
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
                    Platform = cboPlatform.Text,
                    CookiePath = cboCookie.Text
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
            LoadSummary();
            LoadCookies();
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

        private void LoadSummary() 
        {
            var leads = _context.Leads.ToList();

            txtCollectedLeads.Text = leads.Count.ToString();
            txtUniqueLeads.Text = leads.GroupBy(g => g.Name).Count().ToString();
            txtContactedLeads.Text = leads.Where(x => x.Status == "Contacted").Count().ToString();
        }


        private void LoadCookies()
        {
            var cookies = _context.CookieFiles
                .Select(c => c.FileName)
                .ToList();
            cboCookie.Items.Clear();
            foreach (var cookie in cookies)
            {
                cboCookie.Items.Add(cookie);
                cboUsername.Items.Add(cookie);
            }
            if (cboCookie.Items.Count > 0)
            {
                cboCookie.SelectedIndex = 0;
            }
            if (cboUsername.Items.Count > 0)
            {
                cboUsername.SelectedIndex = 0;
            }
        }

        private void cookiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Forms.Cookies cookies = new Forms.Cookies(_context);
            cookies.Show();
        }

        private void dataControlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Forms.Data_Controls data_Controls = new Forms.Data_Controls(_context);
            data_Controls.Show();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Message Sequence started. This may take a while.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            try
            {
                MessengerDto messengerDto = new MessengerDto
                {
                    Text = rtxtMessage.Text,
                    TextList = messageSequence,
                    MessegeRotation = ckMessageRotate.Checked,
                    AccountRotation = ckAccountRotate.Checked,
                    PrivateMode = ckPrivateMode.Checked,
                    CookiePath = cboUsername.Text,
                };

                await _senderService.MessagingSequenceAsync(messengerDto);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                MessageBox.Show("Sequence completed.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnAddGroup_Click(object sender, EventArgs e)
        {
            var msg = rtxtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(msg))
            {
                messageSequence.Add(msg);
            }
            else
            {
                MessageBox.Show("Please enter a message first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRemoveGroup_Click(object sender, EventArgs e)
        {
            if (cboMessagingGroup.SelectedItem != null)
            {
                messageSequence.Remove(cboMessagingGroup.SelectedItem.ToString());
            }
            else
            {
                MessageBox.Show("Please select a message to remove.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboMessagingGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboMessagingGroup.SelectedItem != null)
            {
                // Display the selected message in the textbox
                rtxtMessage.Text = cboMessagingGroup.SelectedItem.ToString();
            }
            else
            {
                rtxtMessage.Clear();
            }
        }

        private void btnDataControls_Click(object sender, EventArgs e)
        {
            Forms.Data_Controls data_Controls = new Forms.Data_Controls(_context);
            data_Controls.Show();
        }
    }
}