using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Forms.Forms
{
    public partial class Custom_Messages : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        public Custom_Messages(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
            InitializeComponent();
        }

        private async void Custom_Messages_Load(object sender, EventArgs e)
        {
            await LoadCustomMessagesAsync();
        }

        // ------------------------------
        // LOAD ALL CUSTOM MESSAGES
        // ------------------------------
        private async Task LoadCustomMessagesAsync()
        {
            var messages = _context.CustomMessages
                                   .Join(_context.Leads,
                                         msg => msg.LeadId,
                                         lead => lead.Id,
                                         (msg, lead) => new
                                         {
                                             msg.Id,
                                             LeadName = lead.Name,
                                             msg.Text
                                         })
                                   .ToList();

            if (messages.Count == 0)
            {
                lstResult.DataSource = null;
                lstResult.Items.Clear();
                lstResult.Items.Add("📭 No custom messages yet — click 'Run' to generate messages!");
                lstResult.Enabled = false; // disable selection since no items
                rtxtMessage.Clear();
                return;
            }

            lstResult.Enabled = true;
            lstResult.DataSource = messages;
            lstResult.DisplayMember = "LeadName";
            lstResult.ValueMember = "Id";
            lstResult.SelectedIndex = -1;

            rtxtMessage.Clear();
        }


        // ------------------------------
        // GENERATE MESSAGES USING AI
        // ------------------------------
        private async void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                lstResult.Items.Clear();
                var leads = _context.Leads.ToList();

                foreach (var lead in leads)
                {
                    var aiDto = new Application.Dtos.AiDto
                    {
                        Prompt = rtxtQuery.Text,
                        Leads = lead
                    };

                    // Generate message using AI service
                    var generatedMessage = await _aiService.GenerateDirectMessageAsync(aiDto);

                    var customMessage = new CustomMessages
                    {
                        LeadId = lead.Id,
                        Text = generatedMessage
                    };

                    _context.CustomMessages.Add(customMessage);
                    await _context.SaveChangesAsync();

                    lstResult.Items.Add($"✅ Generated message for {lead.Name}");
                }

                MessageBox.Show("All messages generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCustomMessagesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating messages: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------
        // DISPLAY SELECTED MESSAGE CONTENT
        // ------------------------------
        private async void lstResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstResult.SelectedIndex == -1) return;

            var selectedItem = lstResult.SelectedItem;
            if (selectedItem == null) return;

            int messageId = (int)lstResult.SelectedValue;
            var message = await _context.CustomMessages.FindAsync(messageId);

            if (message != null)
            {
                rtxtMessage.Text = message.Text;
            }
        }

        // ------------------------------
        // UPDATE SELECTED MESSAGE
        // ------------------------------
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (lstResult.SelectedIndex == -1)
            {
                MessageBox.Show("Select a message to update.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int messageId = (int)lstResult.SelectedValue;
            var message = await _context.CustomMessages.FindAsync(messageId);

            if (message == null)
            {
                MessageBox.Show("Message not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            message.Text = rtxtMessage.Text;
            _context.CustomMessages.Update(message);
            await _context.SaveChangesAsync();

            MessageBox.Show("Message updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadCustomMessagesAsync();
        }

        // ------------------------------
        // DELETE SELECTED MESSAGE
        // ------------------------------
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstResult.SelectedIndex == -1)
            {
                MessageBox.Show("Select a message to delete.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int messageId = (int)lstResult.SelectedValue;
            var message = await _context.CustomMessages.FindAsync(messageId);

            if (message != null)
            {
                _context.CustomMessages.Remove(message);
                await _context.SaveChangesAsync();

                MessageBox.Show("Message deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCustomMessagesAsync();
            }
        }

        // ------------------------------
        // DELETE ALL MESSAGES
        // ------------------------------
        private async void btnDeleteALL_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Are you sure you want to delete ALL custom messages?",
                                          "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                _context.CustomMessages.RemoveRange(_context.CustomMessages);
                await _context.SaveChangesAsync();

                MessageBox.Show("All custom messages deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCustomMessagesAsync();
            }
        }
    }
}
