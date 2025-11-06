using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Taskly.Application.Dtos;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Forms.Forms
{
    public partial class Settings : Form
    {
        private readonly ApplicationDbContext _context;
        private Domain.Entities.Settings? _settings;

        public Settings(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
        }


        private async Task LoadSettingsAsync()
        {
            _settings = await _context.Settings.FirstOrDefaultAsync();

            if (_settings == null)
            {
                _settings = new Domain.Entities.Settings();
                _context.Settings.Add(_settings);
                await _context.SaveChangesAsync();
            }

            // Bind UI controls to values
            txtDomain.Text = _settings.MasterDomainUrl;
            ckHttpMode.Checked = _settings.ProcessDataRemotely;
            ckSendMessagesOnline.Checked = _settings.SendMessagesRemotely;
            ckSearchDomainRotate.Checked = _settings.DomainRotateWhenExtractingRemotely;
            ckSearchCookieRotate.Checked = _settings.CookieRotateWhenExtractingRemotely;
            txtMessegingDelay.Text = _settings.MessagingDelayInMinutes.ToString();
            ckMessenginRandomCookieSelect.Checked = _settings.RandomlySelectCookiesForMessaging;
            ckGemini.Checked = _settings.APIKeyRotateWhenUsingGemini;
            ckLMStudio.Checked = _settings.UseLMStudio;
        }

        private async void btnSaveChanges_Click(object sender, EventArgs e)
        {
            try
            {
                _settings = await _context.Settings.FirstOrDefaultAsync();

                if (_settings == null)
                {
                    _settings = new Domain.Entities.Settings();
                    _context.Settings.Add(_settings);
                }

                // Assign values from UI
                _settings.MasterDomainUrl = txtDomain.Text;
                _settings.ProcessDataRemotely = ckHttpMode.Checked;
                _settings.SendMessagesRemotely = ckSendMessagesOnline.Checked;
                _settings.DomainRotateWhenExtractingRemotely = ckSearchDomainRotate.Checked;
                _settings.CookieRotateWhenExtractingRemotely = ckSearchCookieRotate.Checked;
                _settings.MessagingDelayInMinutes = int.Parse(txtMessegingDelay.Text);
                _settings.RandomlySelectCookiesForMessaging = ckMessenginRandomCookieSelect.Checked;
                _settings.APIKeyRotateWhenUsingGemini = ckGemini.Checked;
                _settings.UseLMStudio = ckLMStudio.Checked;

                await _context.SaveChangesAsync();

                MessageBox.Show("✅ Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDownloadBatch_Click(object sender, EventArgs e)
        {
            // Example list of SearchDto objects (replace with actual EF Core query later)
            var searchData = new List<SearchDto>
            {
                new SearchDto { Id = 1, Keyword = "Test1", Query = "Query1", CookiePath = @"C:\cookies\file1.txt", PageNumber = 1, PrivateMode = false, MultiPlatform = false },
                new SearchDto { Id = 2, Keyword = "Test2", Query = "Query2", CookiePath = @"C:\cookies\file2.txt", PageNumber = 2, PrivateMode = true, MultiPlatform = true },
            };

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = "SearchData.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using var writer = new StreamWriter(sfd.FileName);
                        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true
                        });
                        csv.WriteRecords(searchData);

                        MessageBox.Show("CSV file created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating CSV file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void Settings_Load_1(object sender, EventArgs e)
        {
            await LoadSettingsAsync();
        }

        private void ckSearchCookieRotate_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
