using System;
using System.Linq;
using System.Windows.Forms;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Forms.Forms
{
    public partial class Accounts : Form
    {
        private readonly ApplicationDbContext _context;
        private int? _selectedId = null;

        public Accounts(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Accounts_Load(object sender, EventArgs e)
        {
            LoadPlatform();
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            try
            {
                dgvAccounts.DataSource = _context.SocialLogins
                    .Select(x => new
                    {
                        x.Id,
                        x.Username,
                        x.Platform,
                        x.CreatedAt,
                        x.UpdatedAt
                    })
                    .ToList();

                dgvAccounts.ClearSelection();
                _selectedId = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading accounts:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPlatform()
        {
            cboPlatform.Items.AddRange(new[]
            {
                "All Platforms",
                "Facebook",
                "Instagram",
                "Twitter",
                "Reddit",
                "TikTok"
            });

            cboPlatform.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text) ||
                    cboPlatform.SelectedIndex == 0)
                {
                    MessageBox.Show("Please fill in all fields and select a platform.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var socialLogin = new SocialLogins
                {
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text.Trim(),
                    Platform = cboPlatform.Text,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.SocialLogins.Add(socialLogin);
                _context.SaveChanges();

                MessageBox.Show("Account added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAccounts();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding account:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvAccounts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAccounts.SelectedRows.Count > 0)
            {
                _selectedId = Convert.ToInt32(dgvAccounts.SelectedRows[0].Cells["Id"].Value);
                var selected = _context.SocialLogins.Find(_selectedId);

                if (selected != null)
                {
                    txtUsername.Text = selected.Username;
                    txtPassword.Text = selected.Password;
                    cboPlatform.Text = selected.Platform;
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedId == null)
            {
                MessageBox.Show("Please select an account to update.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var existing = _context.SocialLogins.Find(_selectedId);

                if (existing != null)
                {
                    existing.Username = txtUsername.Text.Trim();
                    existing.Password = txtPassword.Text.Trim();
                    existing.Platform = cboPlatform.Text;
                    existing.UpdatedAt = DateTime.Now;

                    _context.SaveChanges();

                    MessageBox.Show("Account updated successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAccounts();
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating account:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedId == null)
            {
                MessageBox.Show("Please select an account to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this account?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    var account = _context.SocialLogins.Find(_selectedId);

                    if (account != null)
                    {
                        _context.SocialLogins.Remove(account);
                        _context.SaveChanges();

                        MessageBox.Show("Account deleted successfully!",
                            "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAccounts();
                        ClearInputs();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting account:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will delete ALL social accounts. Are you sure?",
                "Confirm Delete All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _context.SocialLogins.RemoveRange(_context.SocialLogins);
                    _context.SaveChanges();

                    MessageBox.Show("All accounts deleted successfully!",
                        "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAccounts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting all accounts:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearInputs()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            cboPlatform.SelectedIndex = 0;
            _selectedId = null;
        }
    }
}
