using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PandasBudgettingTool.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    /// <summary>Persist the current page's visible data to the database.</summary>
    public virtual Task SaveAsync() => Task.CompletedTask;

    /// <summary>Reload the current page's data from the database.</summary>
    public virtual Task RefreshAsync() => Task.CompletedTask;
}