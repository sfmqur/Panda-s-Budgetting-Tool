using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PandasBudgettingTool.ViewModels;

public partial class HelpViewModel : ViewModelBase
{
  [ObservableProperty]
  private string _helpText = string.Empty;

  public HelpViewModel()
  {
    HelpText = loadHelpText("help.txt");
  }
  
  private static string loadHelpText(string helpFileName)
  {
    var path = helpFileName;
    return File.Exists(path)
      ? File.ReadAllText(path)
      : $"No help file found for '{helpFileName}'.";
  }
}