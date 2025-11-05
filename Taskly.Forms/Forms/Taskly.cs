using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Taskly.Application.Constants;
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
        private readonly IAiService _aiService;
        private readonly ICampaignService _campaignService;
        private readonly IUiLogger _logger;

        private List<string> messageSequence = new List<string>();

        public Taskly(ApplicationDbContext context,
            IExtractService extractService,
            ISenderService senderService,
            IAiService aiService,
            ICampaignService campaignService,
            IUiLogger logger)
        {
            InitializeComponent();

            _context = context;
            _extractService = extractService;
            _senderService = senderService;
            _aiService = aiService;
            _campaignService = campaignService;
            _logger = logger;

            // Bind ListBox to the logger's BindingList
            lstLogs.DataSource = _logger.Logs;
            lstLogs.DisplayMember = "ToString"; // calls LogMessage.ToString()
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Search started. This may take a while.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            try
            {
                var settings = await _context.Settings.FirstOrDefaultAsync();
                var httpMode = settings.ProcessDataRemotely;

                SearchDto searchDto = new SearchDto
                {
                    Keyword = txtKeywords.Text,
                    Query = rtxtQuery.Text,
                    PageNumber = int.Parse(txtPages.Text),
                    MultiPlatform = ckMultiPlatform.Checked,
                    CookiePath = cboCookie.Text,
                    PrivateMode = ckPrivateMode.Checked,
                    HttpMode = httpMode
                };

                if (httpMode)
                {
                    ApiConstant.ExtractorHttpRequest(searchDto, settings.MasterDomainUrl);
                }
                else
                {
                    await _extractService.ExtractAsync(searchDto);
                }
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
            LoadSummary();
            LoadCookies();
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
            }
            if (cboCookie.Items.Count > 0)
            {
                cboCookie.SelectedIndex = 0;
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
                var settings = await _context.Settings.FirstOrDefaultAsync();
                var httpMode = settings.SendMessagesRemotely;
                
                var messengerDto = new MessengerDto
                {
                    Text = rtxtMessage.Text,
                    TextList = messageSequence,
                    MessegeRotation = ckMessageRotate.Checked,
                    AccountRotation = ckAccountRotate.Checked,
                    PrivateMode = ckPrivateMode.Checked,
                    MessageDelay = (int)(settings.MessagingDelayInMinutes * 60 * 1000)
                };

                if (httpMode) { }
                else
                {
                    await _senderService.StartMessages(messengerDto);
                }
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
                cboMessagingGroup.Items.Add(msg);
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
                cboMessagingGroup.Items.Remove(cboMessagingGroup.SelectedItem);
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

        public void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                // Make sure we’re on the UI thread
                Invoke(new Action(() => AppendLog(message)));
                return;
            }

            // Append log to the existing RichTextBox from the designer
            lstLogs.Items.Add(message + Environment.NewLine);
            // Auto-scroll to the latest log
            lstLogs.TopIndex = lstLogs.Items.Count - 1;
        }

        public void ClearLogs()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearLogs));
                return;
            }

            if (lstLogs.DataSource is BindingList<string> logs)
            {
                logs.Clear(); // This clears the UI automatically
            }
        }


        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            ClearLogs();
        }

        private void templatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Templates templates = new Templates(_context);
            templates.Show();
        }

        private void customMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Custom_Messages custom_Messages = new Custom_Messages(_context, _aiService);
            custom_Messages.Show();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadSummary();
        }

        private async void btnBulkSearch_Click(object sender, EventArgs e)
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            bool httpMode = settings.ProcessDataRemotely;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV files (*.csv)|*.csv";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Read CSV file into list of SearchDto
                        List<SearchDto> searchList;
                        using (var reader = new StreamReader(ofd.FileName))
                        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true,
                        }))
                        {
                            searchList = csv.GetRecords<SearchDto>().ToList();
                        }

                        MessageBox.Show($"Loaded {searchList.Count} searches. Starting now...", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        if (httpMode)
                        {
                            ApiConstant.BatchExtractorHttpRequest(searchList, settings.MasterDomainUrl);
                            MessageBox.Show("All searches are in progress via HTTP.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        else
                        {
                            foreach (var item in searchList)
                            {
                                await _extractService.ExtractAsync(item);
                            }
                        }

                        MessageBox.Show("All searches completed successfully.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading CSV or performing searches: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings(_context);
            settings.Show();
        }

        private void googleAIGeminiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Google_AI_Keys google_AI_Keys = new Google_AI_Keys(_context);
            google_AI_Keys.Show();
        }

        private void dataControlsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Data_Controls data_Controls = new Data_Controls(_context);
            data_Controls.Show();
        }

        private void templatesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Templates templates = new Templates(_context);
            templates.Show();
        }

        private void customMessagesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Custom_Messages custom_Messages = new Custom_Messages(_context, _aiService);
            custom_Messages.Show();
        }

        private void cookiesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Cookies cookies = new Cookies(_context);
            cookies.Show();
        }

        private void domainsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Domains domains = new Domains(_context);
            domains.Show();
        }

        private void campaignToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Campaigns campaigns = new Campaigns(_context, _campaignService);
            campaigns.Show();
        }

        private void btnCampaigns_Click(object sender, EventArgs e)
        {
            Campaigns campaigns = new Campaigns(_context, _campaignService);
            campaigns.Show();
        }
    }
}