using System.Collections.Generic;

namespace PandasBudgettingTool.Models.Importing.Importers;

public class WfCredCard : IImporter
{
  public string HelpFileName { get; } = "WellsFargo.txt";
  
  public List<Transaction> Import(string filePath)
  {
    throw new System.NotImplementedException();
  }
}