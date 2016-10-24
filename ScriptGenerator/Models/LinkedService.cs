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
