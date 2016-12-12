using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGenerator
{
    public static class Extensions
    {
        
        public static void SaveToFile(this string jsonStr, string folder)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return;

            Directory.CreateDirectory(folder);
            dynamic ssd = JsonConvert.DeserializeObject(jsonStr);
            var path = folder + ssd.name + ".json";
            //File.Create(path);

            using (TextWriter sw = new StreamWriter(path, true))
            {
                sw.Write(jsonStr);
            }
        }
    }
}
