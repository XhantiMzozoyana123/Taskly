using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Forms.Forms
{
    public partial class Data_Controls : Form
    {
        private readonly ApplicationDbContext _context;

        public Data_Controls(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Data_Controls_Load(object sender, EventArgs e)
        {
            LoadLeads();
        }

        private void LoadLeads()
        {
            dgvData.DataSource = _context.Leads.ToList();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.Title = "Import Leads from CSV";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var reader = new StreamReader(openFileDialog.FileName))
                        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HeaderValidated = null,
                            MissingFieldFound = null
                        }))
                        {
                            var importedLeads = csv.GetRecords<Leads>().ToList();

                            // ✅ Ignore Id values to let SQL Server auto-generate them
                            foreach (var lead in importedLeads)
                            {
                                lead.Id = 0; // reset identity
                                _context.Leads.Add(lead);
                            }

                            _context.SaveChanges();
                        }

                        LoadLeads();
                        MessageBox.Show("Leads imported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error importing leads: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                saveFileDialog.Title = "Export Leads to CSV";
                saveFileDialog.FileName = $"leads_export_{DateTime.Now:yyyyMMddHHmmss}.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var leads = _context.Leads.ToList();

                        using (var writer = new StreamWriter(saveFileDialog.FileName))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(leads);
                        }

                        MessageBox.Show("Leads exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting leads: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvData.CurrentRow == null)
            {
                MessageBox.Show("Select a lead to update first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = (int)dgvData.CurrentRow.Cells["Id"].Value;
            var lead = _context.Leads.FirstOrDefault(l => l.Id == id);

            if (lead == null)
            {
                MessageBox.Show("Selected lead not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Example: toggle status between "New" and "Contacted"
            lead.Status = lead.Status == "New" ? "Contacted" : "New";
            _context.SaveChanges();
            LoadLeads();

            MessageBox.Show("Lead updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvData.CurrentRow == null)
            {
                MessageBox.Show("Select a lead to delete first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = (int)dgvData.CurrentRow.Cells["Id"].Value;
            var lead = _context.Leads.FirstOrDefault(l => l.Id == id);

            if (lead != null)
            {
                var confirm = MessageBox.Show($"Delete lead '{lead.Name}'?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    _context.Leads.Remove(lead);
                    _context.SaveChanges();
                    LoadLeads();

                    MessageBox.Show("Lead deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnDeleteALL_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Are you sure you want to delete ALL leads?",
                "Confirm Delete All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                _context.Leads.RemoveRange(_context.Leads);
                _context.SaveChanges();
                LoadLeads();

                MessageBox.Show("All leads deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
