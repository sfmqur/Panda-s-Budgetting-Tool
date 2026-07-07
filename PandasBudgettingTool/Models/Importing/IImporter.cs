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
  /// <param name="accountName"></param>
  /// <returns></returns>
  Task<List<Transaction>> ImportAsync(string filePath, string accountName);
  
  /// <summary>
  /// Should validate file formatting, and yield a true or false. This ensures users don't select a bad file
  /// or detects if the format changes. 
  /// </summary>
  /// <param name="filePath"></param>
  /// <returns></returns>
  bool ValidateFormat(string filePath);
}