using System;
using System.Linq;
using System.Windows.Forms;
using Taskly.Domain;
using Taskly.Domain.Entities;
using Taskly.Infrastructure;

namespace Taskly.Forms.Forms
{
    public partial class Domains : Form
    {
        private readonly ApplicationDbContext _context;

        public Domains(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Domains_Load(object sender, EventArgs e)
        {
            LoadDomains();
        }

        private void LoadDomains()
        {
            var domains = _context.Domains.ToList();
            dgvDomains.DataSource = domains;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var domainUrl = txtDomain.Text.Trim();

            if (string.IsNullOrEmpty(domainUrl))
            {
                MessageBox.Show("Enter a valid domain URL.");
                return;
            }

            var domain = new Domain.Entities.Domains
            {
                Url = domainUrl
            };

            _context.Domains.Add(domain);
            _context.SaveChanges();

            LoadDomains();
            txtDomain.Clear();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvDomains.CurrentRow == null)
            {
                MessageBox.Show("Select a domain to update.");
                return;
            }

            int id = (int)dgvDomains.CurrentRow.Cells["Id"].Value;
            var domain = _context.Domains.Find(id);

            if (domain != null)
            {
                domain.Url = dgvDomains.Text.Trim();
                _context.SaveChanges();
                LoadDomains();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDomains.CurrentRow == null)
            {
                MessageBox.Show("Select a domain to delete.");
                return;
            }

            int id = (int)dgvDomains.CurrentRow.Cells["Id"].Value;
            var domain = _context.Domains.Find(id);

            if (domain != null)
            {
                _context.Domains.Remove(domain);
                _context.SaveChanges();
                LoadDomains();
            }
        }

        private void btnDeleteALL_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all domains?",
                                "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _context.Domains.RemoveRange(_context.Domains);
                _context.SaveChanges();
                LoadDomains();
            }
        }
    }
}
