using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Interfaces
{
    public enum ShortcutAction
    {
        // Hybrid + Lead Search Actions
        LaunchHybridSearchBrowser,
        AddToLeadList,
        NavigatePreviousLead,
        NavigateNextLead,

        // Messaging Actions
        RotateTemplates,
        RotateIcebreakers,
        RotateCustomMessages,

        // Cookie Actions
        RotateCookies,

        // AI Actions
        GenerateIcebreaker,
        GenerateCustomMessage
    }

    public interface IShortcutService
    {
        /// <summary>
        /// Returns all registered shortcut actions.
        /// </summary>
        List<ShortcutAction> GetAllActions();

        /// <summary>
        /// Maps a keyboard key (or key combination) to a shortcut action.
        /// </summary>
        void RegisterShortcut(string keyCombination, ShortcutAction action);

        /// <summary>
        /// Executes the shortcut action associated with the given key combination.
        /// </summary>
        Task ExecuteShortcutAsync(string keyCombination);

        // Add this:
        Dictionary<string, ShortcutAction> GetRegisteredShortcuts();
    }
}
