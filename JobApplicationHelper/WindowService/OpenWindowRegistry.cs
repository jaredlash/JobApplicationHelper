using System.Windows;
using JobApplicationHelper.ViewModels;

namespace JobApplicationHelper.WindowService;

public sealed class OpenWindowRegistry : IOpenWindowRegistry
{
    private sealed record WindowEntry(ViewModelBase ViewModel, Window Window);

    private readonly Dictionary<Type, WindowEntry> singleWindows = [];

    private readonly Dictionary<ViewModelBase, Window> instanceWindows = new(ReferenceEqualityComparer.Instance);

    public bool TryGetSingle(Type viewModelType, out ViewModelBase? viewModel, out Window? window)
    {
        if (singleWindows.TryGetValue(viewModelType, out var entry))
        {
            viewModel = entry.ViewModel;
            window = entry.Window;
            return true;
        }

        viewModel = null;
        window = null;
        return false;
    }

    public bool TryGetWindow(ViewModelBase viewModel, out Window? window)
    {
        return instanceWindows.TryGetValue(viewModel, out window);
    }

    public void RegisterSingle(Type viewModelType, ViewModelBase viewModel, Window window)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(window);

        if (singleWindows.ContainsKey(viewModelType))
        {
            throw new InvalidOperationException($"A window for {viewModelType.FullName} is already registered.");
        }

        singleWindows.Add(viewModelType, new WindowEntry(viewModel, window));
    }

    public void RegisterInstance(ViewModelBase viewModel, Window window)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(window);

        if (!instanceWindows.TryAdd(viewModel, window))
        {
            throw new InvalidOperationException("This ViewModel instance already has a registered window.");
        }
    }

    public void RemoveSingle(Type viewModelType, ViewModelBase viewModel, Window window)
    {
        if (singleWindows.TryGetValue(viewModelType, out var entry)
            && ReferenceEquals(entry.ViewModel, viewModel)
            && ReferenceEquals(entry.Window, window))
        {
            singleWindows.Remove(viewModelType);
        }
    }

    public void RemoveInstance(ViewModelBase viewModel, Window window)
    {
        if (instanceWindows.TryGetValue(viewModel, out var existingWindow)
            && ReferenceEquals(existingWindow, window))
        {
            instanceWindows.Remove(viewModel);
        }
    }
}