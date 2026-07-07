using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Csv;

namespace PandasBudgettingTool.Models.Importing.Importers;

public class WfAccount : IImporter
{
  public string HelpFileName { get; } = "WellsFargo.txt";

  public async Task<List<Transaction>> ImportAsync(string filePath, string accountName)
  {
    if (!File.Exists(filePath))
      throw new FileNotFoundException(filePath);
    
    var fileString = File.ReadAllText(filePath);
    var csvlines = CsvReader.ReadFromText(fileString, new CsvOptions()).ToList();
    
    var transactions = new List<Transaction>();
    foreach (var csvline in csvlines)
    {
      //csv is MM/DD/YYYY need YYYY-MM-DD
      var dateArray = csvline[0].Replace("\"","").Split("/");
      var date = $"{dateArray[2]}-{dateArray[0]}-{dateArray[1]}";
      var dateTime = DateTime.Parse(date);
      var name = csvline[1].Replace("\"","");
      var amountString = csvline[2].Replace("\"","");
      var amount = decimal.Parse(amountString);
      var isPosted = csvline[4] == "Posted";
      if (isPosted)
      {
        transactions.Add(new Transaction()
        {
          AccountName = accountName,
          Id = $"{date}|{name}|{amount}",
          Date = dateTime,
          Amount = amount,
          Name =  name,
        });
      }
    }

    return transactions;
  }
  
  public bool ValidateFormat(string filePath)
  {
    if (!File.Exists(filePath))
      return false;

    var fileString = File.ReadAllText(filePath);
    var csvoptions = new CsvOptions()
    {
      HeaderMode = HeaderMode.HeaderAbsent,
    };
    var csvlines = CsvReader.ReadFromText(fileString, csvoptions).ToList();
    var firstRowCheck = csvlines[0].ToString() == "\"DATE\",\"DESCRIPTION\",\"AMOUNT\",\"CHECK #\",\"STATUS\"";
    return firstRowCheck;
  }
}

