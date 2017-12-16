using System.IO;
using System.Runtime.Serialization.Json;

namespace D365CustomDataProvider
{
    public static class Utility
    {
        public static T DeserializeObject<T>(string json)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(json);
                writer.Flush();
                stream.Position = 0;
                T responseObject = (T)serializer.ReadObject(stream);
                return responseObject;
            }
        }

        public static string SerializeObject<T>(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, obj);
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream);
                string requestBody = reader.ReadToEnd();
                return requestBody;
            }
        }
    }
}