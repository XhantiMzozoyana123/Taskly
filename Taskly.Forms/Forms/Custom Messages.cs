using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Taskly.Application.Dtos;
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
            InitializeComponent();
            _context = context;
            _aiService = aiService;
        }

        private async void Custom_Messages_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        // -----------------------
        // Load data into DataGridView
        // -----------------------
        private async Task LoadDataAsync()
        {
            var messages = await _context.CustomMessages.AsNoTracking().ToListAsync();
            dgvData.DataSource = messages;
            lstLogs.Items.Add($"Loaded {messages.Count} messages.");
        }

        // -----------------------
        // Double-click to populate query
        // -----------------------
        private void dgvData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                rtxtQuery.Text = dgvData.Rows[e.RowIndex].Cells["Text"].Value.ToString();
            }
        }

        // -----------------------
        // Update selected message
        // -----------------------
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvData.CurrentRow == null) return;

            int id = (int)dgvData.CurrentRow.Cells["Id"].Value;
            var message = await _context.CustomMessages.FindAsync(id);

            if (message != null)
            {
                message.Text = rtxtQuery.Text;
                message.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await LoadDataAsync();
                lstLogs.Items.Add($"Updated message ID {id}");
            }
        }

        // -----------------------
        // Delete selected message
        // -----------------------
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvData.CurrentRow == null) return;

            int id = (int)dgvData.CurrentRow.Cells["Id"].Value;
            var message = await _context.CustomMessages.FindAsync(id);

            if (message != null)
            {
                _context.CustomMessages.Remove(message);
                await _context.SaveChangesAsync();
                await LoadDataAsync();
                lstLogs.Items.Add($"Deleted message ID {id}");
            }
        }

        // -----------------------
        // Delete all messages
        // -----------------------
        private async void btnDeleteALL_Click(object sender, EventArgs e)
        {
            var allMessages = await _context.CustomMessages.ToListAsync();
            _context.CustomMessages.RemoveRange(allMessages);
            await _context.SaveChangesAsync();
            await LoadDataAsync();
            lstLogs.Items.Add("Deleted all messages.");
        }

        // -----------------------
        // CsvHelper ClassMap
        // -----------------------
        public sealed class CustomMessagesMap : ClassMap<CustomMessages>
        {
            public CustomMessagesMap()
            {
                Map(m => m.LeadId).Name("LeadId");
                Map(m => m.Text).Name("Text");
                Map(m => m.Id).Ignore();
                Map(m => m.CreatedAt).Ignore();
                Map(m => m.UpdatedAt).Ignore();
            }
        }

        // -----------------------
        // Import CSV
        // -----------------------
        private async void btnImportCSV_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.Title = "Import Custom Messages from CSV";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    using (var reader = new StreamReader(openFileDialog.FileName))
                    using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HeaderValidated = null,
                        MissingFieldFound = null
                    }))
                    {
                        csv.Context.RegisterClassMap<CustomMessagesMap>();
                        var imported = csv.GetRecords<CustomMessages>().ToList();

                        // ✅ Reset identity column values so EF lets SQL Server generate new IDs
                        foreach (var msg in imported)
                        {
                            msg.Id = 0;
                            msg.CreatedAt = DateTime.UtcNow;
                            msg.UpdatedAt = DateTime.UtcNow;
                        }

                        // ✅ Use AddRangeAsync for async bulk insert
                        await _context.CustomMessages.AddRangeAsync(imported);
                        await _context.SaveChangesAsync();

                        prgLoad.Maximum = imported.Count;
                        prgLoad.Value = imported.Count;

                        lstLogs.Items.Add($"✅ Imported {imported.Count} custom messages from CSV.");
                    }

                    await LoadDataAsync();
                    MessageBox.Show("Messages imported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing messages: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // -----------------------
        // Export CSV
        // -----------------------
        private async void btnExportCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                saveFileDialog.Title = "Export Custom Messages to CSV";
                saveFileDialog.FileName = $"custom_messages_export_{DateTime.Now:yyyyMMddHHmmss}.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var messages = await _context.CustomMessages.AsNoTracking().ToListAsync();

                        using (var writer = new StreamWriter(saveFileDialog.FileName))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.Context.RegisterClassMap<CustomMessagesMap>();
                            csv.WriteRecords(messages);
                        }

                        lstLogs.Items.Add($"Exported {messages.Count} messages to CSV.");
                        MessageBox.Show("Messages exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting messages: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // -----------------------
        // Clear logs
        // -----------------------
        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            lstLogs.Items.Clear();
        }

        // -----------------------
        // AI-generated messages
        // -----------------------
        private async void btnAIGenMessages_Click(object sender, EventArgs e)
        {
            var leads = await _context.Leads.AsNoTracking().ToListAsync();
            if (!leads.Any()) return;

            prgLoad.Maximum = leads.Count;
            prgLoad.Value = 0;

            int processed = 0;

            foreach (var lead in leads)
            {
                var aiDto = new AiDto
                {
                    Prompt = rtxtQuery.Text,
                    Lead = lead
                };

                // Generate message asynchronously
                var response = await _aiService.GenerateDirectMessageAsync(aiDto);

                // Create CustomMessages entity
                var customMessage = new CustomMessages()
                {
                    LeadId = lead.Id,
                    Text = response
                };

                // Add to DB (batch SaveChanges every N items)
                _context.CustomMessages.Add(customMessage);
                processed++;

                // Save in batches of 20 to avoid too many DB calls
                if (processed % 20 == 0 || processed == leads.Count)
                {
                    await _context.SaveChangesAsync();
                }

                // Update progress bar and log
                prgLoad.Value = processed;
                lstLogs.Items.Add($"Generated message for Lead ID {lead.Id}");
                lstLogs.TopIndex = lstLogs.Items.Count - 1; // Scroll to bottom
            }

            lstLogs.Items.Add($"✅ AI messages generated for {leads.Count} leads.");
            prgLoad.Value = prgLoad.Maximum;
        }
    }
}
