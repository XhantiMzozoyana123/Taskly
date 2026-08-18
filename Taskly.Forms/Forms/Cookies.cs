using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Forms.Forms
{
    public partial class Cookies : Form
    {
        private readonly ApplicationDbContext _context;

        public Cookies(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Cookies_Load(object sender, EventArgs e)
        {
            LoadCookies();
        }

        private void LoadCookies()
        {
            var cookies = _context.CookieFiles
                .Select(c => new
                {
                    c.Id,
                    c.FileName
                })
                .ToList();

            dgvCookies.DataSource = cookies;
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a Cookies File";
                openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        string fileContent = File.ReadAllText(filePath);

                        // Cookies are stored and used locally only — no remote uploads.
                        CookieFiles cookieEntity = new CookieFiles
                        {
                            FileName = filePath,
                            Content = fileContent,
                            Remote = false
                        };

                        _context.CookieFiles.Add(cookieEntity);
                        _context.SaveChanges();

                        LoadCookies();
                        MessageBox.Show("Cookies file uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error uploading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvCookies.CurrentRow == null)
            {
                MessageBox.Show("Select a file to update first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int selectedId = (int)dgvCookies.CurrentRow.Cells["Id"].Value;

            var cookie = _context.CookieFiles.FirstOrDefault(c => c.Id == selectedId);
            if (cookie == null)
            {
                MessageBox.Show("Could not find the selected file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a New Cookies File";
                openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        string fileContent = File.ReadAllText(filePath);

                        cookie.FileName = Path.GetFileName(filePath);
                        cookie.Content = fileContent;

                        _context.SaveChanges();
                        LoadCookies();

                        MessageBox.Show("Cookies file updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCookies.CurrentRow == null)
            {
                MessageBox.Show("Select a file to delete first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int selectedId = (int)dgvCookies.CurrentRow.Cells["Id"].Value;
            var cookie = _context.CookieFiles.FirstOrDefault(c => c.Id == selectedId);

            if (cookie != null)
            {
                var confirm = MessageBox.Show($"Are you sure you want to delete '{cookie.FileName}'?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    _context.CookieFiles.Remove(cookie);
                    _context.SaveChanges();
                    LoadCookies();

                    MessageBox.Show("File deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnDeleteALL_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Are you sure you want to delete ALL cookie files?",
                "Confirm Delete All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                _context.CookieFiles.RemoveRange(_context.CookieFiles);
                _context.SaveChanges();
                LoadCookies();

                MessageBox.Show("All cookie files deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
