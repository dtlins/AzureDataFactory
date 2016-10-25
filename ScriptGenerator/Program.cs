using ScriptGenerator.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filepath = ConfigurationManager.AppSettings.GetValues("tablesCSVLocation").FirstOrDefault() ?? Environment.CurrentDirectory + "\\tables.csv";
            string results = string.Empty;

            var csv = new CSVfile(filepath);

            //foreach (var table in csv.Tables)
            //{
            //    //results += Templates.GenerateSourceTable(table) + Environment.NewLine;
            //    results += Templates.GenerateDestinationTable(table) + Environment.NewLine;
            //    //results += Templates.AzureSqlDWTableStaging(table) + Environment.NewLine;
            //}

            var onlyUpdatesInLastXDays = TimeSpan.FromDays(1);
            results += Templates.GenerateCopyPipeline(DateTime.UtcNow, DateTime.UtcNow, csv, LinkedService.LinkedServices["All"], onlyUpdatesInLastXDays);

            //results += Templates.GenerateUpsertStoredProcedures(csv);
            //results += Templates.GenerateGetStoredProcedures(csv);

            File.WriteAllText(Environment.CurrentDirectory + "\\results.txt", results);
        }
    }
}
