using Clickless.MVVM.Models;
using Newtonsoft.Json;
using System;
using System.IO;

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
                var ret = File.Create(path);
                ret.Close();
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
                data = data ?? new T();

                return data ;
            }

            return data;
        }
    }
}
