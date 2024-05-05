using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace UEditor.Utitlies
{
    /// <summary>
    /// Config 的摘要说明
    /// </summary>
    public static class Config
    {
        private static bool noCache = true;
        public static string configPath = string.Empty;
        private static JObject BuildItems()
        {
            var RootPath = AppContext.BaseDirectory;
            //#if DEBUG
            //            DirectoryInfo DIR = Directory.GetParent(RootPath);
            //            RootPath = DIR.Parent.Parent.Parent.ToString();
            //#endif      
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = "/ueditor/config.json";
            }
            if (configPath[0] != '/')
            {
                configPath = "/" + configPath;
            }
            var json = File.ReadAllText(RootPath + "/wwwroot" + configPath);
            return JObject.Parse(json);
        }

        public static JObject Items
        {
            get
            {
                if (noCache || _Items == null)
                {
                    _Items = BuildItems();
                }
                return _Items;
            }
        }
        private static JObject _Items;


        public static T GetValue<T>(string key)
        {
            return Items[key].Value<T>();
        }

        public static String[] GetStringList(string key)
        {
            return Items[key].Select(x => x.Value<String>()).ToArray();
        }

        public static String GetString(string key)
        {
            return GetValue<String>(key);
        }

        public static int GetInt(string key)
        {
            return GetValue<int>(key);
        }
    }
}