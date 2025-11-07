using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Taskly.Application.Interfaces;

namespace Taskly.Forms.Forms
{
    public partial class Shortcuts : Form
    {
        private readonly IShortcutService _shortcutService;

        public Shortcuts(IShortcutService shortcutService)
        {
            InitializeComponent();
            _shortcutService = shortcutService;
        }

        private void Shortcuts_Load(object sender, EventArgs e)
        {
            LoadShortcuts();
        }

        private void LoadShortcuts()
        {
            var registeredShortcuts = _shortcutService.GetRegisteredShortcuts();

            var shortcutList = registeredShortcuts
                .Select(kvp => new ShortcutDisplay
                {
                    KeyCombination = kvp.Key,
                    Action = kvp.Value.ToString()
                })
                .ToList();

            dgvShortcuts.DataSource = new BindingList<ShortcutDisplay>(shortcutList);

            dgvShortcuts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvShortcuts.ReadOnly = true;
        }

        private class ShortcutDisplay
        {
            public string KeyCombination { get; set; }
            public string Action { get; set; }
        }
    }
}
