using System.Collections.Generic;
using System.Threading.Tasks;

namespace PandasBudgettingTool.Models.Importing.Importers;

public class WfAccount : IImporter
{
  public string HelpFileName { get; } = "WellsFargo.txt";
  
  public async Task<List<Transaction>> ImportAsync(string filePath)
  {
    await Task.Delay(5000);
    return new List<Transaction>();
  }
}