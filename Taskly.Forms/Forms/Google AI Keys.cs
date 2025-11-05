using System;
using System.Linq;
using System.Windows.Forms;
using Taskly.Domain;
using Taskly.Domain.Entities;
using Taskly.Infrastructure; // Your ApplicationDbContext namespace

namespace Taskly.Forms.Forms
{
    public partial class Google_AI_Keys : Form
    {
        private readonly ApplicationDbContext _context;

        public Google_AI_Keys(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Google_AI_Keys_Load(object sender, EventArgs e)
        {
            LoadKeys();
        }

        private void LoadKeys()
        {
            var keys = _context.GoogleAIs.ToList();
            dgvKeys.DataSource = keys;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string apiKey = txtKey.Text.Trim();

            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Enter a valid API key.");
                return;
            }

            var googleAI = new GoogleAI
            {
                ApiKey = apiKey
            };

            _context.GoogleAIs.Add(googleAI);
            _context.SaveChanges();

            LoadKeys();
            txtKey.Clear();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvKeys.CurrentRow == null)
            {
                MessageBox.Show("Select a key to update.");
                return;
            }

            int id = (int)dgvKeys.CurrentRow.Cells["Id"].Value;
            var key = _context.GoogleAIs.Find(id);

            if (key != null)
            {
                key.ApiKey = txtKey.Text.Trim();
                _context.SaveChanges();
                LoadKeys();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvKeys.CurrentRow == null)
            {
                MessageBox.Show("Select a key to delete.");
                return;
            }

            int id = (int)dgvKeys.CurrentRow.Cells["Id"].Value;
            var key = _context.GoogleAIs.Find(id);

            if (key != null)
            {
                _context.GoogleAIs.Remove(key);
                _context.SaveChanges();
                LoadKeys();
            }
        }

        private void btnDeleteALL_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all API keys?",
                                "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _context.GoogleAIs.RemoveRange(_context.GoogleAIs);
                _context.SaveChanges();
                LoadKeys();
            }
        }

        private void dgvGoogleAI_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvKeys.CurrentRow != null)
            {
                txtKey.Text = dgvKeys.CurrentRow.Cells["ApiKey"].Value.ToString();
            }
        }
    }
}
