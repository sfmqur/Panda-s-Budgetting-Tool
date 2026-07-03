using System.Collections.Generic;
using System.Threading.Tasks;

namespace PandasBudgettingTool.Models.Importing;

public interface IImporter
{
  /// <summary>
  /// Relative Path of the importer help file under Help, hardcoded
  /// </summary>
  string HelpFileName { get; }
  
  /// <summary>
  /// Yield a list of Transactions
  /// </summary>
  /// <param name="filePath"></param>
  /// <returns></returns>
  Task<List<Transaction>> ImportAsync(string filePath);
}