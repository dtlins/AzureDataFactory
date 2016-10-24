using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGenerator.Models
{
    public class LinkedService
    {
        public LinkedServiceType Type { get; set; }
        public string Name { get; set; }
        //public static Dictionary<string, LinkedService> LinkedServices => new Dictionary<string, LinkedService>()
        //{
        //    ["nike-uat75-sql1_Payment"] = new LinkedService() { Name = "nike-uat75-sql1_Payment", Type = LinkedServiceType.SqlServerTable },
        //    ["nike-uat75-sql1_ESW"] = new LinkedService() { Name = "nike-uat75-sql1_ESW", Type = LinkedServiceType.SqlServerTable },
        //    ["nike-uat75-sql1_esw_Reports"] = new LinkedService() { Name = "nike-uat75-sql1_esw_Reports", Type = LinkedServiceType.SqlServerTable },
        //    ["nike-uat75-sql1_eswArchive"] = new LinkedService() { Name = "nike-uat75-sql1_eswArchive", Type = LinkedServiceType.SqlServerTable },

        //    ["AzureSqlDWLinkedService"] = new LinkedService() { Name = "AzureSqlDWLinkedService", Type = LinkedServiceType.AzureSqlDWTable },
        //    ["AzureSqlESWDW"] = new LinkedService() { Name = "AzureSqlESWDW", Type = LinkedServiceType.AzureSqlDWTable },

        //    ["All"] = new LinkedService() { Name = "All", Type = LinkedServiceType.All },
        //};

        public static Dictionary<string, LinkedService> LinkedServices
        {
            get
            {
                var settings = ConfigurationManager.GetSection("customAppSettingsGroup/LinkedServices") as System.Collections.Specialized.NameValueCollection;
                var dict = new Dictionary<string, LinkedService>() { };
                foreach (string item in settings)
                {
                    dict.Add(item, new LinkedService() { Name = item, Type = (LinkedServiceType)Enum.Parse(typeof(LinkedServiceType), settings[item]) });
                }
                return dict;
            }
        }

        public enum LinkedServiceType
        {
            AzureSqlDWTable,
            SqlServerTable,
            All
        }
    }
}
