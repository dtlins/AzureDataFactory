using ScriptGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ScriptGenerator.Models.LinkedService;

namespace ScriptGenerator
{
    internal static class Templates
    {
        private static string DateTimeFormatStr = "yyyy-MM-ddTHH:mm:00Z";


        internal static string GenerateDestinationTable(Table table)
        {
            return table.DestinationLinkedService.Type == LinkedServiceType.SqlServerTable ? SqlServerTable(table, isSourceTable: false) : AzureSqlDWTable(table, isSourceTable: false);
        }

        internal static string GenerateSourceTable(Table table)
        {
            return table.SourceLinkedService.Type == LinkedServiceType.SqlServerTable ? SqlServerTable(table, isSourceTable: true) : AzureSqlDWTable(table, isSourceTable: true);
        }

        internal static string GenerateCopyPipeline(DateTime startTime, DateTime endTime, CSVfile file, LinkedService linkedServiceSource, TimeSpan? onlyUpdatesSinceXDaysAgo = null)
        {
            var tables = linkedServiceSource.Name == LinkedServices["All"].Name ? file.Tables : file.Tables.Where(t => t.SourceLinkedService.Name == linkedServiceSource.Name).ToList();
            return $@"{{
                        ""name"": """",
                        ""properties"": {{
                            ""description"": """",
                            ""activities"": [{string.Join(",", GenerateCopyActivities(tables, onlyUpdatesSinceXDaysAgo))}],
                            ""start"": ""{startTime.ToString(DateTimeFormatStr)}"",
                            ""end"": ""{endTime.ToString(DateTimeFormatStr)}"",
                            ""isPaused"": false,
                            ""hubName"": ""esw-dw-df1_hub"",
                            ""pipelineMode"": ""Scheduled""
                        }}
                    }}";

        }

        internal static string GenerateGetStoredProcedures(CSVfile csv)
        {
            var sb = new StringBuilder();

            foreach (var t in csv.Tables)
            {
                if (t.SourceLinkedService.Type == LinkedServiceType.AzureSqlDWTable)
                {
                    var fullTableName = $"[{t.SchemaName}].[{t.TableName}]";
                    var stagingTableName = $"[{t.SchemaName}].[{t.TableName}_staging]";

                    sb.Append($@" CREATE PROC [dbo].[get_{t.SchemaName}_{t.TableName}] AS
                              BEGIN
                                    SET NOCOUNT ON
                              
                              END
                            GO
                          ");
                }
            }
            return sb.ToString();
        }

        public static string GenerateUpsertStoredProcedures(CSVfile csv)
        {
            var sb = new StringBuilder();

            foreach (var t in csv.Tables)
            {
                var primaryKey = t.ColumnNames.First();
                var fullTableName = $"[{t.SchemaName}].[{t.TableName}]";
                var stagingTableName = $"[{t.SchemaName}].[{t.TableName}_staging]";

                sb.Append($@" CREATE PROC [dbo].[merge_{t.SchemaName}_{t.TableName}] AS
                              BEGIN
                                    SET NOCOUNT ON
                              UPDATE {fullTableName}
                              SET
                                {string.Join(",", t.ColumnNames.Select(n => $"{fullTableName}.{n} = source.{n}"))}
                              FROM
                              {stagingTableName} source WHERE source.{primaryKey} = {fullTableName}.{primaryKey}

                              INSERT INTO {fullTableName}
                              ({string.Join(",", t.ColumnNames)})
                              SELECT
                              {string.Join(",", t.ColumnNames.Select(n => $"source.{n}"))}
                              FROM {stagingTableName} source
                              WHERE NOT EXISTS (SELECT * FROM {fullTableName} target WHERE source.{primaryKey} = target.{primaryKey})
                              
                              DELETE FROM {stagingTableName}

                              END
                            GO
                          ");
            }
            return sb.ToString();
        }

        private static string SqlServerTable(Table table, bool isSourceTable)
        {
            var linkedServiceName = isSourceTable ? table.SourceLinkedService.Name : table.DestinationLinkedService.Name;
            return $@"{{
                        ""name"": ""{GetSqlServerName(table)}"",
                        ""properties"": {{
                            ""published"": false,
                            ""type"": ""{LinkedServices[linkedServiceName].Type}"",
                            ""linkedServiceName"": ""{linkedServiceName}"",
                            ""typeProperties"": {{
                                ""tableName"": ""[{table.SchemaName}].[{table.TableName}]""
                            }},
                            ""availability"": {{
                                ""frequency"": ""Minute"",
                                ""interval"": 15
                            }},
                            ""external"": true,
                            ""policy"": {{}}
                        }}
                    }}";
        }

        private static string AzureSqlDWTable(Table table, bool isSourceTable)
        {
            var linkedServiceName = isSourceTable ? table.SourceLinkedService.Name : table.DestinationLinkedService.Name;
            return $@"{{
                        ""name"": ""{GetDWName(table)}"",
                        ""properties"": {{
                            ""published"": false,
                            ""type"": ""{LinkedServices[linkedServiceName].Type}"",
                            ""linkedServiceName"": ""{linkedServiceName}"",
                            ""typeProperties"": {{
                                ""tableName"": ""[{table.SchemaName}].[{table.TableName}]""
                            }},
                            ""availability"": {{
                                ""frequency"": ""Minute"",
                                ""interval"": 15
                            }}
                        }}
                    }}
                    ";
        }

        public static string AzureSqlDWTableStaging(Table table)
        {
            return $@"{{
                        ""name"": ""{GetDWName(table)}_staging"",
                        ""properties"": {{
                            ""published"": false,
                            ""type"": ""AzureSqlDWTable"",
                            ""linkedServiceName"": ""{table.DestinationLinkedService.Name}"",
                            ""typeProperties"": {{
                                ""tableName"": ""[{table.SchemaName}].[{table.TableName}_staging]""
                            }},
                            ""availability"": {{
                                ""frequency"": ""Minute"",
                                ""interval"": 15
                            }}
                        }}
                    }}
                    ";
        }


        private static IList<string> GenerateCopyActivities(IList<Table> tables, TimeSpan? onlyUpdatesSinceXDaysAgo = null)
        {
            IList<string> activities = new List<string>();

            foreach (var table in tables)
            {

                activities.Add($@"{{
                                    ""type"": ""Copy"",
                                    ""typeProperties"": {{{GetActivityTypeProperties(table, onlyUpdatesSinceXDaysAgo)}}},
                                    ""inputs"": [ {GetActivityInputs(table)} ],
                                    ""outputs"": [
                                        {{
                                            ""name"": ""{GetDWName(table)}_staging""
                                        }}
                                    ],
                                    ""policy"": {{
                                        ""timeout"": ""1.00:00:00"",
                                        ""concurrency"": 1,
                                        ""retry"": 3
                                    }},
                                    ""scheduler"": {{
                                        ""frequency"": ""Minute"",
                                        ""interval"": 15
                                    }},
                                    ""name"": ""{table.SchemaName}_{table.TableName}_CopyToStaging""
                                }}");

                activities.Add($@"{{
                                    ""type"": ""SqlServerStoredProcedure"",
                                    ""typeProperties"": {{
                                        ""storedProcedureName"":""merge_{table.SchemaName}_{table.TableName}"",
                                        ""storedProcedureParameters"":{{}}
                                    }},
                                    ""inputs"": [
                                        {{
                                            ""name"": ""{GetDWName(table)}_staging""
                                        }}
                                    ],
                                    ""outputs"": [
                                        {{
                                            ""name"": ""{GetDWName(table)}""
                                        }}
                                    ],
                                    ""policy"": {{
                                        ""timeout"": ""1.00:00:00"",
                                        ""concurrency"": 1,
                                        ""retry"": 3
                                    }},
                                    ""scheduler"": {{
                                        ""frequency"": ""Minute"",
                                        ""interval"": 15
                                    }},
                                    ""name"": ""{table.SchemaName}_{table.TableName}_CopyFromStaging""
                                }}");
            }
            return activities;
        }

        private static string GetActivityInputs(Table table)
        {
            var str = string.Empty;

            if (table.SourceLinkedService.Type == LinkedServiceType.SqlServerTable)
            {
                str += $@"{{""name"": ""{GetSqlServerName(table)}""}}";
            }
            else if (table.SourceLinkedService.Type == LinkedServiceType.AzureSqlDWTable)
            {
                str += $"{ string.Join(",", table.Dependancies.Select(d => $@"{{ ""name"" : ""{GetDWName(new Table(d))}"" }}"))}";
            }
            return str;
        }

        private static string GetActivityTypeProperties(Table table, TimeSpan? onlyUpdatesSinceXDaysAgo)
        {
            var str = string.Empty;
            var copySource = table.SourceLinkedService.Type == LinkedServiceType.SqlServerTable ? "SqlSource" : "SqlDWSource";
            var sinkType = table.SourceLinkedService.Type == LinkedServiceType.SqlServerTable ? "SqlSink" : "SqlDWSink";

            if (table.SourceLinkedService.Type == LinkedServiceType.SqlServerTable)
            {
                var sqlTableSelect = $"select {string.Join(",", table.ColumnNames)} from [{table.SchemaName}].[{table.TableName}]";

                var sqlReaderQuery = onlyUpdatesSinceXDaysAgo.HasValue ?
                      $"$$Text.Format('{sqlTableSelect} WHERE LastUpdateTime >= \\\\'{{0:yyyy-MM-dd HH:mm}}\\\\'', Date.AddDays(WindowStart,-{onlyUpdatesSinceXDaysAgo.Value.Days}))"
                      : sqlTableSelect;

                str += $@"""source"": {{
                                            ""type"": ""SqlSource"",
                                            ""sqlReaderQuery"": ""{sqlReaderQuery}""
                                        }},
                                        ""sink"": {{
                                            ""type"": ""SqlSink"",
                                            ""writeBatchSize"": 0,
                                            ""writeBatchTimeout"": ""00:00:00""
                                        }}";
            }
            else if (table.SourceLinkedService.Type == LinkedServiceType.AzureSqlDWTable)
            {
                str += $@"""source"": {{
                                            ""type"": ""SqlDWSource"",
                                            ""sqlReaderStoredProcedureName"": ""get_{table.SchemaName}_{table.TableName}""
                                        }},
                                        ""sink"": {{
                                            ""type"": ""SqlDWSink"",
                                            ""writeBatchSize"": 0,
                                            ""writeBatchTimeout"": ""00:00:00""
                                        }}";
            }
            return str;
        }

        private static string GetDWName(Table table)
        {
            return $"DW_{table.SchemaName}_{table.TableName}";
        }
        private static string GetSqlServerName(Table table)
        {
            var service = table.SourceLinkedService.Name.Substring(table.SourceLinkedService.Name.IndexOf('_') + 1);

            return $"{service}_{table.SchemaName}_{table.TableName}";
        }
    }
}
