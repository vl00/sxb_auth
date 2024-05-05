using Enyim.Caching;
using Enyim.Caching.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iSchool.Authorization.Lib
{
    public class MemcachedHelper
    {
        private static string _DEPEND_ = "mcache_default_depend";
        private static string _DICT_CACHE_ = "default_core_depend_dictiaonry";
        private static int _EXP_ = 10 * 60; //默认缓存10分钟 
        private static int HH = 3600; //1小时=3600秒

        static readonly object mlock = new object();
        private static readonly ILoggerFactory _loggerFacotry = new LoggerFactory();
        /// <summary>  
        /// 定义一个静态MemcachedClient客户端,它随类一起加载，所有对象共用  
        /// </summary>  
        private static MemcachedClient mclient;
        /// <summary>
        /// 构造函数，连接memcachedcore并为KEYS字典开辟储存空间
        /// </summary>
        static MemcachedHelper()
        {
            //mclient = MemCached.getInstance();
            if (mclient == null)
            {
                lock (mlock)
                {
                    if (mclient == null)
                    {
                        MemcachedClientOptions options = new MemcachedClientOptions();
                        UtilConf.Configuration.GetSection("enyimMemcached").Bind(options);
                        options.Protocol = Enyim.Caching.Memcached.MemcachedProtocol.Text;
                        mclient = new MemcachedClient(_loggerFacotry, new MemcachedClientConfiguration(_loggerFacotry, options));
                    }
                }
            }
            //在缓存中开辟一个专门用来存储Kyes的字典对象       
            MDictionary_SaveDict(new Dictionary<string, List<string>>());
        }

        #region ** 获取缓存 **
        /// <summary>
        /// 获取缓存 
        /// </summary>
        public static object Get(string key)
        {
            key = StrToMD5(key.ToLower());
            return mclient.Get(key);
        }
        public static T Get<T>(string key)
        {
            key = StrToMD5(key.ToLower());
            return mclient.Get<T>(key);
        }
        #endregion

        #region ** 添加缓存 **
        /// <summary> 
        /// 添加单个依赖项的缓存 (最小时间单位为秒)
        /// </summary>         
        public static void Set(string depend, string key, object obj, int exp)
        {
            depend = depend.ToLower();
            key = StrToMD5(key.ToLower());
            try
            {
                //HttpContext.Current.Application.Lock();

                //将数据加入缓存 
                mclient.Add(key, obj, exp);

                //HttpContext.Current.Application.UnLock();

                ////将Keys加入字典 
                //MDictionary_AddKeys(depend, key);
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region ++ Set的多种实现方式
        /// <summary>
        /// 默认时间
        /// </summary>
        public static void Set(string depend, string key, object obj)
        {
            MemcachedHelper.Set(depend, key, obj, _EXP_);
        }
        /// <summary>
        /// 默认Depend和时间
        /// </summary>
        public static void Set(string key, object obj)
        {
            MemcachedHelper.Set(_DEPEND_, key, obj, _EXP_);
        }
        /// <summary>
        /// 默认Depend
        /// </summary>
        public static void Set(string key, object obj, int exp)
        {
            MemcachedHelper.Set(_DEPEND_, key, obj, exp);
        }
        /// <summary>
        /// 长时间缓存
        /// </summary>
        public static void SetLong(string depend, string key, object obj)
        {
            int t = 31536000; //1年 = 10 * 365 * 24 * 60 * 60; 
            MemcachedHelper.Set(depend, key, obj, t);
        }
        /// <summary>
        /// 长时间默认depend
        /// </summary>
        public static void SetLong(string key, object obj)
        {
            int t = 31536000; //365 * 24 * 60 * 60; //1年
            MemcachedHelper.Set(_DEPEND_, key, obj, t);
        }
        public static void SetAllLong(string key, object obj)
        {
            int t = 315360000; //365 * 24 * 60; //10年
            MemcachedHelper.Set(_DEPEND_, key, obj, t);
        }
        #endregion

        #endregion

        #region ** 删除缓存 **
        /// <summary>
        /// 删除有依赖项的Keys的缓存
        /// </summary>
        public static void RemoveKeys(string depend, string key)
        {
            depend = depend.ToLower();
            key = StrToMD5(key.ToLower());
            try
            {
                //HttpContext.Current.Application.Lock();

                //删除缓存                
                mclient.Remove(key);

                //删除key
                //MDictionary_RemoveKeys(depend, key);

                //HttpContext.Current.Application.UnLock();

            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除默认depend的缓存
        /// </summary>
        public static void RemoveKeys(string key)
        {
            RemoveKeys(_DEPEND_, key);
        }

        /// <summary>
        /// 删除整个依赖项
        /// </summary>
        public static void RemoveDepend(string depend)
        {
            depend = depend.ToLower();

            try
            {
                //获取keys列表
                List<string> keysList = MDictionary_GetKeys(depend);

                //循环删除数据
                for (int i = 0; i < keysList.Count; i++)
                {
                    string k = keysList[i];
                    //清空缓存Key
                    mclient.Remove(k);
                    ////清空字典下的Key 
                    //MDictionary.RemoveKeys(depend, k);
                }
                ////清空字典 
                //MDictionary_RemoveDepend(depend);

            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region --字典管理--

        #region ** 字典存取 ** 
        /// <summary>
        /// 取出字典
        /// </summary>        
        public static Dictionary<string, List<string>> MDictionary_GetDict()
        {
            Dictionary<string, List<string>> dict = mclient.Get(_DICT_CACHE_) as Dictionary<string, List<string>>;
            if (dict == null)
            {
                Dictionary<string, List<string>> newDict = new Dictionary<string, List<string>>();
                MDictionary_SaveDict(newDict);
                return newDict;
            }
            else
            {
                return dict;
            }
        }

        /// <summary>
        /// 存入字典
        /// </summary>        
        public static void MDictionary_SaveDict(Dictionary<string, List<string>> dict)
        {
            //默认保存360天
            mclient.Add(_DICT_CACHE_, dict, HH * 24 * 360);
        }

        /// <summary>
        /// 添加并存入
        /// </summary>
        public static void MDictionary_AddToDictAndSave(string depend, List<string> listKeys)
        {
            //取出字典
            Dictionary<string, List<string>> dict = MDictionary_GetDict();

            //修改或新增字典
            if (dict.ContainsKey(depend))
            {
                dict[depend] = listKeys; //覆盖
            }
            else
            {
                dict.Add(depend, listKeys); //新添加
            }

            //存回
            MDictionary_SaveDict(dict);
        }
        #endregion

        #region ** keys操作  ** 
        /// <summary>
        /// 根据depend获取Keys列表
        /// </summary>
        public static List<string> MDictionary_GetKeys(string depend)
        {
            depend = depend.ToLower();

            Dictionary<string, List<string>> dict = MDictionary_GetDict();
            if (dict.ContainsKey(depend))
            {
                return dict[depend];
            }
            return new List<string>();
        }

        /// <summary>
        /// 添加keys到字典
        /// </summary>
        public static void MDictionary_AddKeys(string depend, string key)
        {
            depend = depend.ToLower();
            key = key.ToLower();

            //添加keys列表
            List<string> listKeys = MDictionary_GetKeys(depend);
            if (!listKeys.Contains(key))
            {
                listKeys.Add(key);
                //添加并存回字典
                MDictionary_AddToDictAndSave(depend, listKeys);
            }

        }

        /// <summary>
        /// 从字典中删除一个Key
        /// </summary>
        public static void MDictionary_RemoveKeys(string depend, string key)
        {
            depend = depend.ToLower();
            key = key.ToLower();

            List<string> listKeys = MDictionary_GetKeys(depend);
            if (listKeys.Contains(key))
            {
                listKeys.Remove(key);
                //添加并存回字典
                MDictionary_AddToDictAndSave(depend, listKeys);
            }
        }

        /// <summary>
        /// 删除depend下所有的keys列表
        /// </summary>
        public static void MDictionary_RemoveDepend(string depend)
        {
            depend = depend.ToLower();

            Dictionary<string, List<string>> dict = MDictionary_GetDict();
            if (dict.ContainsKey(depend))
            {
                dict.Remove(depend);
                //存回
                MDictionary_SaveDict(dict);
            }
        }
        #endregion

        /// <summary>
        /// 转换成md5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StrToMD5(string str)
        {
            byte[] data = Encoding.GetEncoding("UTF-8").GetBytes(str + ",;]Sd&@Hhib!$f#^vdv^82%%7(Q&*)#E");
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] OutBytes = md5.ComputeHash(data);

            string OutString = "";
            for (int i = 0; i < OutBytes.Length; i++)
            {
                OutString += OutBytes[i].ToString("x2");
            }
            return OutString.ToUpper();
        }
        #endregion
    }
    public class UtilConf
    {
        private static IConfiguration config;
        public static IConfiguration Configuration//加载配置文件
        {
            get
            {
                if (config != null) return config;
                config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
                return config;
            }
            set => config = value;
        }
    }
}
