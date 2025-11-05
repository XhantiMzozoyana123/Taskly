using System;
using System.Linq;
using System.Windows.Forms;
using Taskly.Domain;

namespace Taskly.Forms.Forms
{
    public partial class Templates : Form
    {
        private readonly ApplicationDbContext _context;

        public Templates(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Templates_Load(object sender, EventArgs e)
        {
            LoadTemplates();
        }

        // Load all templates into the ComboBox
        private void LoadTemplates()
        {
            cboName.Items.Clear();
            var templates = _context.Templates.ToList();

            if (templates.Count == 0)
            {
                // Show a friendly message in the dropdown,
                // but don't disable it so users can still type a new name.
                cboName.Items.Add("📭 No templates yet — add one to get started!");
                // ❌ Removed: cboName.Enabled = false;
                rtxtMessage.Clear();
                return;
            }

            // ✅ Always keep it enabled
            // cboName.Enabled = true;

            foreach (var item in templates)
            {
                cboName.Items.Add(item.Name);
            }

            cboName.SelectedIndex = -1;
            rtxtMessage.Clear();
        }


        // Display selected template content
        private void cboName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboName.SelectedIndex == -1 || !_context.Templates.Any()) return;

            var selectedTemplate = _context.Templates.FirstOrDefault(x => x.Name == cboName.SelectedItem.ToString());
            if (selectedTemplate != null)
            {
                rtxtMessage.Text = selectedTemplate.Content;
            }
        }

        // Add new template
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboName.Text) || string.IsNullOrWhiteSpace(rtxtMessage.Text))
            {
                MessageBox.Show("Provide both Name and Content.");
                return;
            }

            // Check for duplicates
            if (_context.Templates.Any(t => t.Name == cboName.Text))
            {
                MessageBox.Show("A template with that name already exists.");
                return;
            }

            var newTemplate = new Domain.Entities.Templates
            {
                Name = cboName.Text,
                Content = rtxtMessage.Text
            };

            _context.Templates.Add(newTemplate);
            _context.SaveChanges();

            MessageBox.Show("Template added!");
            LoadTemplates();
            cboName.Text = string.Empty;
            rtxtMessage.Clear();
        }

        // Update selected template
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (cboName.SelectedIndex == -1)
            {
                MessageBox.Show("Select a template first.");
                return;
            }

            var selectedTemplate = _context.Templates.FirstOrDefault(x => x.Name == cboName.SelectedItem.ToString());
            if (selectedTemplate == null)
            {
                MessageBox.Show("Template not found.");
                return;
            }

            selectedTemplate.Content = rtxtMessage.Text;
            _context.Templates.Update(selectedTemplate);
            _context.SaveChanges();

            MessageBox.Show("Template updated successfully!");
            LoadTemplates();
        }

        // Delete selected template
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (cboName.SelectedIndex == -1)
            {
                MessageBox.Show("Select a template to delete.");
                return;
            }

            var selectedTemplate = _context.Templates.FirstOrDefault(x => x.Name == cboName.SelectedItem.ToString());
            if (selectedTemplate == null)
            {
                MessageBox.Show("Template not found.");
                return;
            }

            _context.Templates.Remove(selectedTemplate);
            _context.SaveChanges();

            MessageBox.Show("Template deleted!");
            LoadTemplates();
        }

        // Delete all templates
        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            if (_context.Templates.Any())
            {
                if (MessageBox.Show("Are you sure you want to delete all templates?",
                                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _context.Templates.RemoveRange(_context.Templates);
                    _context.SaveChanges();

                    MessageBox.Show("All templates deleted!");
                    LoadTemplates();
                }
            }
            else
            {
                MessageBox.Show("No templates to delete.");
            }
        }
    }
}
