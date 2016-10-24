using ScriptGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGenerator
{
    internal class CSVfile
    {
        public IList<Table> Tables { get; private set; }

        public CSVfile(string filepath)
        {
            Tables = new List<Table>();
            var rows = File.ReadAllLines(filepath).Select(i => i.Split(';'));
            foreach (var row in rows)
            {
                if (row[0] == rows.First()[0]) continue;
                if (string.IsNullOrEmpty(row.First())) continue;

                var sourceLinkedService = row[0];
                var destinationLinkedService = row[1];
                var dependancies = row[2];
                var tablename = row[3];
                var tableColumns = row[4];

                var s1 = tablename.Substring(tablename.IndexOf('[') + 1);
                var schema = s1.Substring(0, s1.IndexOf(']'));

                var s2 = s1.Substring(s1.IndexOf('[') + 1);
                var table = s2.Substring(0, s2.IndexOf(']'));

                Tables.Add(new Table()
                {
                    SourceLinkedService = LinkedService.LinkedServices[sourceLinkedService],
                    DestinationLinkedService = LinkedService.LinkedServices[destinationLinkedService],
                    Dependancies = dependancies?.Split(','),
                    SchemaName = schema,
                    TableName = table,
                    ColumnNames = tableColumns.Split(',')
                });
            }
        }


    }
    public class Table
    {
        public Table() { }
        public Table(string schemaAndTablename)
        {
            var s1 = schemaAndTablename.Substring(schemaAndTablename.IndexOf('[') + 1);
            this.SchemaName= s1.Substring(0, s1.IndexOf(']'));

            var s2 = s1.Substring(s1.IndexOf('[') + 1);
            this.TableName = s2.Substring(0, s2.IndexOf(']'));
        }
        public LinkedService SourceLinkedService { get; set; }
        public LinkedService DestinationLinkedService { get; set; }
        public string TableName { get; set; }
        public IList<string> ColumnNames { get; set; }
        public string SchemaName { get; set; }
        public IList<string> Dependancies { get; set; }
    }
}
