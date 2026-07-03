using System;
using PandasBudgettingTool.Models.Importing.Importers;

namespace PandasBudgettingTool.Models.Importing;

public static class ImporterFactory
{
  public static IImporter GetImporter(ImporterType type)
  {
    return type switch
    {
      ImporterType.WellsFargoAcct => new WfAccount(),
      ImporterType.WellsFargoCc => new WfCredCard(),
      _ => throw new ArgumentException($"Unknown importer type: {type}")
    };
  }
}