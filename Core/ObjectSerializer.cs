using Clickless.MVVM.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.Core
{
    public class ObjectSerializer
    {
        static readonly string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Clickless");


        public static void SaveData(object data, string filename)
        {
            var path = Path.Combine(appDataPath, filename);
            if (!Directory.Exists(appDataPath)) {
                Directory.CreateDirectory(appDataPath);
            }
            if (!File.Exists(path))
            {
                File.Create(path);
            }

            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, jsonData);
        }

        public static void SaveData<T>(T data) where T : IFileNameProvider
        {
            var path = Path.Combine(appDataPath, data.GetFileName());
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            if (!File.Exists(path))
            {
                File.Create(path);
            }

            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, jsonData);
        }

        public static T LoadDataOrDefault<T>() where T: IFileNameProvider, new()
        {
            var data = new T();

            var filePath = Path.Combine(appDataPath, data.GetFileName());

            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                data = JsonConvert.DeserializeObject<T>(jsonData);
                return data;
            }

            return default(T);
        }

        public static T LoadDataOrDefault<T>(string filePath)
        {
            filePath = Path.Combine(appDataPath, filePath);

            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }

            return default(T);
        }
    }
}
