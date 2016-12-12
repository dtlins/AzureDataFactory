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
        private static string _folder = Environment.CurrentDirectory + "\\files\\";

        static void Main(string[] args)
        {
            var filepath = ConfigurationManager.AppSettings.GetValues("tablesCSVLocation")?.FirstOrDefault() ?? Environment.CurrentDirectory + "\\tables.csv";
            string results = string.Empty;

            var csv = new CSVfile(filepath);

            var delimiter = '|';

            //foreach (var table in csv.Tables)
            //{
            //    results += Templates.GenerateSourceTable(table) + delimiter;
            //    results += Templates.GenerateDestinationTable(table) + delimiter;
            //    results += Templates.AzureSqlDWTableStaging(table) + delimiter;
            //}

            //Directory.Delete(_folder, true);
            //results.Split(delimiter).ToList().ForEach(s => s.SaveToFile(_folder));

            //var onlyUpdatesInLastXDays = TimeSpan.FromDays(1);
            //results += Templates.GenerateCopyPipeline(DateTime.UtcNow, DateTime.UtcNow, csv, LinkedService.LinkedServices["All"], onlyUpdatesInLastXDays);

            results += Templates.GenerateUpsertStoredProcedures(csv);
            //results += Templates.GenerateGetStoredProcedures(csv);

            File.WriteAllText(Environment.CurrentDirectory + "\\results.txt", results);
        }
    }
}
