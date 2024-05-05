using Microsoft.Extensions.Options;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Library
{
    public class RedisOptions
    {
        public string host { get; set; }
        public int port { get; set; }
        public string password { get; set; }
        public int timeout { get; set; }
    }
    public class RedisHelper
    {
        private static RedisOptions redisOptions;
        private static RedisClient client;
        public RedisHelper(RedisOptions _redisOptions)
        {
            redisOptions = _redisOptions;
            client = new RedisClient(redisOptions.host, redisOptions.port, redisOptions.password);
            client.SendTimeout = redisOptions.timeout;
            client.RetryTimeout = redisOptions.timeout;
            client.ReceiveTimeout = redisOptions.timeout;
            client.ConnectTimeout = redisOptions.timeout;
        }
        public static T Get<T>(string key)
        {
            key = MD5Helper.GetMD5(key);
            return client.Get<T>(key);
        }
        public static bool Set(string key, object value, int seconds)
        {
            key = MD5Helper.GetMD5(key);
            return client.Set(key, value, new TimeSpan(0, 0, seconds));
        }
        public static bool Set(string key, object value, DateTime expiresAt)
        {
            key = MD5Helper.GetMD5(key);
            return client.Set(key, value, expiresAt);
        }
        public static bool Remove(string key)
        {
            key = MD5Helper.GetMD5(key);
            return client.Remove(key);
        }
    }
}
