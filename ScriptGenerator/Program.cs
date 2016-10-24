using ScriptGenerator.Models;
using System;
using System.Collections.Generic;
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
            var filepath = Environment.CurrentDirectory + "\\tables.csv";
            string results = string.Empty;

            var csv = new CSVfile(filepath);

            //foreach (var table in csv.Tables)
            //{
            //    //results += Templates.GenerateSourceTable(table) + Environment.NewLine;
            //    results += Templates.GenerateDestinationTable(table) + Environment.NewLine;
            //    results += Templates.AzureSqlDWTableStaging(table) + Environment.NewLine;
            //}

            results += Templates.GenerateCopyPipeline(DateTime.UtcNow, DateTime.UtcNow, csv, LinkedService.LinkedServices["All"]);

            //results += Templates.GenerateUpsertStoredProcedures(csv);
            //results += Templates.GenerateGetStoredProcedures(csv);

            File.WriteAllText(Environment.CurrentDirectory + "\\results.txt", results);
        }
    }
    }
