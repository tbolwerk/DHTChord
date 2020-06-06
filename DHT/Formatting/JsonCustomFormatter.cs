using System;
using System.IO;
using Newtonsoft.Json;

namespace DHT.Formatting
{
    public  class JsonCustomFormatter
    {
        public  string SerializeObject(object obj, int maxDepth)
        {
            using (var strWriter = new StringWriter())
            {
                using (var jsonWriter = new CustomJsonTextWriter(strWriter))
                {
                    bool Include() => jsonWriter.CurrentDepth <= maxDepth;
                    var resolver = new CustomContractResolver(Include);
                    
                    var serializer = new JsonSerializer {ContractResolver = resolver,ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
                    serializer.Serialize(jsonWriter, obj);
                }
                return strWriter.ToString();
            }
        }
    }
}