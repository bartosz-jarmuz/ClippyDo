using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows; // MessageBox
using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Settings;
using MessageBox = System.Windows.MessageBox;

namespace ClippyDo.Adapter.Windows.Registration
{
    internal sealed class HotkeysRegistrationStartupTask : IStartupTask
    {
        private readonly IGlobalHotkeyService _hotkeys;
        private readonly ISettingsStore _settingsStore;
        private readonly ILogger _log;

        public HotkeysRegistrationStartupTask(
            IGlobalHotkeyService hotkeys,
            ISettingsStore settingsStore,
            ILogger log)
        {
            _hotkeys = hotkeys;
            _settingsStore = settingsStore;
            _log = log;
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            var s = await _settingsStore.LoadAsync(ct);

            // Try preferred, fall back if needed; these calls DO NOT mutate 's'
            var pickerChosen = await EnsureHotkeyAsync(
                id: "Picker",
                preferred: s.HotkeyPicker,
                fallbacks: new[] { "Ctrl+Alt+V", "Ctrl+Shift+Insert", "Win+Shift+V", "Ctrl+Win+V" },
                ct);

            var compareChosen = await EnsureHotkeyAsync(
                id: "Compare",
                preferred: s.HotkeyCompare,
                fallbacks: new[] { "Ctrl+Alt+C", "Ctrl+Shift+Y", "Win+Shift+C", "Ctrl+Win+C" },
                ct);

            // De-conflict if both ended up same after fallback
            if (Normalize(compareChosen) == Normalize(pickerChosen))
            {
                _log.Warn($"Hotkey conflict detected between Picker and Compare on '{pickerChosen}'. Adjusting Compare.");
                compareChosen = await EnsureHotkeyAsync(
                    id: "Compare",
                    preferred: NextAfter(compareChosen, new[] { "Ctrl+Alt+C", "Ctrl+Shift+Y", "Win+Shift+C", "Ctrl+Win+C" }),
                    fallbacks: Array.Empty<string>(),
                    ct);
            }

            // Build a NEW Settings instance (init-only properties respected)
            var updated = new Settings
            {
                // Limits
                MaxStoredItems = s.MaxStoredItems,
                SimpleCompareMaxChars = s.SimpleCompareMaxChars,

                // UI
                PlacePickerNearMouse = s.PlacePickerNearMouse,
                PreviewMaxScreenFraction = s.PreviewMaxScreenFraction,

                // Filters (regex)
                NumbersRegex = s.NumbersRegex,
                PathsRegex = s.PathsRegex,
                CustomRegex = s.CustomRegex,

                // Hotkeys (override the chosen ones)
                HotkeyPicker = pickerChosen,
                HotkeyCompare = compareChosen,
                HotkeyPastePlain = s.HotkeyPastePlain,

                // Persistence
                DatabasePath = s.DatabasePath,

                // Columns layout
                ColumnsLayout = s.ColumnsLayout
            };

            await _settingsStore.SaveAsync(updated, ct);
        }

        private async Task<string> EnsureHotkeyAsync(string id, string preferred, string[] fallbacks, CancellationToken ct)
        {
            if (TryRegister(id, preferred, out var _))
            {
                _log.Info($"Registered hotkey '{id}' as '{preferred}'.");
                return preferred;
            }

            foreach (var candidate in fallbacks.Where(f => !string.Equals(Normalize(f), Normalize(preferred), StringComparison.OrdinalIgnoreCase)))
            {
                if (TryRegister(id, candidate, out var _))
                {
                    _log.Warn($"Hotkey '{id}' fallback: '{preferred}' in use. Using '{candidate}' instead.");
                    MessageBox.Show(
                        $"ClippyDo could not register the hotkey for '{id}' ({preferred}) because another app is using it.\n" +
                        $"It will use '{candidate}' instead. You can change this in Settings.",
                        "Hotkey adjusted",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return candidate;
                }
            }

            // None worked; keep preferred in settings so user can see/change it
            MessageBox.Show(
                $"ClippyDo could not register the hotkey for '{id}'.\n" +
                $"Tried: {string.Join(", ", new[] { preferred }.Concat(fallbacks))}\n\n" +
                $"Another application likely owns these combinations. Please pick a different hotkey in Settings.",
                "Hotkey registration failed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            _log.Error($"Hotkey '{id}' registration failed. Preferred: '{preferred}'. Tried fallbacks: [{string.Join(", ", fallbacks)}].");
            return preferred;
        }

        private bool TryRegister(string id, string gesture, out string? errorMessage)
        {
            try
            {
                _hotkeys.Register(id, gesture);
                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _log.Warn($"Hotkey '{id}' -> '{gesture}' failed: {ex.Message}");
                return false;
            }
        }

        private static string Normalize(string gesture)
            => string.Join("+", gesture.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(p => p.ToUpperInvariant()));

        private static string NextAfter(string current, string[] ordered)
        {
            var i = Array.FindIndex(ordered, x => string.Equals(Normalize(x), Normalize(current), StringComparison.OrdinalIgnoreCase));
            return i >= 0 && i + 1 < ordered.Length ? ordered[i + 1] : current;
        }
    }
}
