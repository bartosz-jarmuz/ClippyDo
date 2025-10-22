using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClippyDo.Core.Abstractions;
using ClippyDo.Core.Features.Clipboard;

namespace ClippyDo.App.Wpf.Features.Picker;

public sealed partial class PickerViewModel : ObservableObject
{
    private readonly ISearchIndex _searchIndex;
    private CancellationTokenSource? _cts;
    public ObservableCollection<Clip> Items { get; } = new();

    [ObservableProperty]
    private string _query = string.Empty;

    public PickerViewModel(ISearchIndex searchIndex)
    {
        _searchIndex = searchIndex;
    }

    partial void OnQueryChanged(string value)
    {
        _ = RefreshAsync(value);
    }

    private async Task RefreshAsync(string query)
    {
        // cancel prior in-flight search
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            // optional tiny debounce to avoid hammering on fast typing
            await Task.Delay(75, ct);

            // gather results off the UI thread
            var buffer = new List<Clip>();
            await foreach (var clip in _searchIndex.SearchAsync(query, ct))
            {
                buffer.Add(clip);
                if (buffer.Count >= 200) break; // basic safety cap; refine later via virtualization
            }

            // update UI collection
            Items.Clear();
            foreach (var c in buffer) Items.Add(c);
        }
        catch (OperationCanceledException)
        {
            // expected when typing quickly
        }
    }

    [RelayCommand]
    private void ClearQuery() => Query = string.Empty;
}