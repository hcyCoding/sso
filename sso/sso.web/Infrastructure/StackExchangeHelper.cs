using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace sso.web.Infrastructure
{
    public class StackExchangeHelper
    {
        #region  私有函数，属性，以及构造函数

        /// <summary>
        /// 连接字符串
        /// </summary>
        private string connectionString;

        /// <summary>
        /// redis键值key的前缀
        /// </summary>
        private string defaultKey;

        /// <summary>
        /// redis连接对象
        /// </summary>
        private IConnectionMultiplexer connectionMultiplexer;

        /// <summary>
        /// redis数据库
        /// </summary>
        private IDatabase redisDB;

        /// <summary>
        /// 锁
        /// </summary>
        private readonly object Locker = new object();

        /// <summary>
        /// 实例化IConnectionMultiplexer
        /// </summary>
        /// <returns></returns>
        private IConnectionMultiplexer GetConnectionMultiplexer()
        {
            if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
            {
                lock (Locker)
                {
                    if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                    {
                        connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
                    }
                }
            }
            return connectionMultiplexer;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public StackExchangeHelper()
        {
            //connectionString = ConfigurationManager.ConnectionStrings["RedisConnectionString"].ConnectionString;
            //defaultKey = ConfigurationManager.AppSettings["RedisDefaultKey"].ToString();
            connectionString = "localhost:6379,password=123456";
            defaultKey = "sso.web";
            GetConnectionMultiplexer();
            redisDB = connectionMultiplexer.GetDatabase(-1);  //-1实际是第一个数据库
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbIndex">数据库索引</param>
        public StackExchangeHelper(int dbIndex)
        {
            connectionString = ConfigurationManager.ConnectionStrings["RedisConnectionString"].ConnectionString;
            defaultKey = ConfigurationManager.AppSettings["RedisDefaultKey"].ToString();
            GetConnectionMultiplexer();
            redisDB = connectionMultiplexer.GetDatabase(dbIndex);
        }

        #endregion

        #region  字符串

        #region  同步

        /// <summary>
        /// 设置key并保存值（如果key已存在，则覆盖）
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value">内容</param>
        /// <param name="expireMinutes">过期时间</param>
        /// <returns>是否保存成功</returns>
        public bool StringSet(string key, string value, int expireMinutes = 30)
        {
            key = AddKeyPrefix(key);
            expireMinutes += AddtionExpireMinutes();
            return redisDB.StringSet(key, value, TimeSpan.FromMinutes(expireMinutes));
        }

        /// <summary>
        /// 设置key并保存值(对象序列化后进行保存)
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">对象实例</param>
        /// <param name="expireMinutes">过期时间</param>
        /// <returns>是否保存成功</returns>
        public bool StringSet<T>(string key, T value, int expireMinutes = 30)
        {
            key = AddKeyPrefix(key);
            expireMinutes += AddtionExpireMinutes();
            var serializeVal = SerializeToBinary(value);
            return redisDB.StringSet(key, serializeVal, TimeSpan.FromMinutes(expireMinutes));
        }

        /// <summary>
        /// 获取key值对应的内容
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>内容</returns>
        public string StringGet(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.StringGet(key);
        }

        /// <summary>
        /// 获取key值对应的内容(返回对象)
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <returns>对象</returns>
        public T StringGet<T>(string key)
        {
            key = AddKeyPrefix(key);
            var value = redisDB.StringGet(key);
            return DeserializeToObject<T>(value);
        }

        #endregion

        #region  异步

        /// <summary>
        /// [异步]设置key并保存值（如果key已存在，则覆盖）
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value">内容</param>
        /// <param name="expireMinutes">过期时间</param>
        /// <returns>是否保存成功</returns>
        public async Task<bool> StringSetAsync(string key, string value, int expireMinutes = 30)
        {
            key = AddKeyPrefix(key);
            expireMinutes += AddtionExpireMinutes();
            return await redisDB.StringSetAsync(key, value, TimeSpan.FromMinutes(expireMinutes));
        }

        /// <summary>
        /// [异步]设置key并保存值(对象序列化后进行保存)
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">对象实例</param>
        /// <param name="expireMinutes">过期时间</param>
        /// <returns>是否保存成功</returns>
        public async Task<bool> StringSetAsync<T>(string key, T value, int expireMinutes = 30)
        {
            key = AddKeyPrefix(key);
            expireMinutes += AddtionExpireMinutes();
            var serializeVal = SerializeToBinary(value);
            return await redisDB.StringSetAsync(key, serializeVal, TimeSpan.FromMinutes(expireMinutes));
        }

        /// <summary>
        /// [异步]获取key值对应的内容
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>内容</returns>
        public async Task<string> StringGetAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.StringGetAsync(key);
        }

        /// <summary>
        /// [异步]获取key值对应的内容(返回对象)
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <returns>对象</returns>
        public async Task<T> StringGetAsync<T>(string key)
        {
            key = AddKeyPrefix(key);
            var value = await redisDB.StringGetAsync(key);
            return DeserializeToObject<T>(value);
        }

        #endregion

        #endregion

        #region  Hash

        #region  同步

        /// <summary>
        /// 判断hash中是否有某个字段
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <returns>是否存在</returns>
        public bool HashExist(string key, string field)
        {
            key = AddKeyPrefix(key);
            return redisDB.HashExists(key, field);
        }

        /// <summary>
        /// 移除hash中指定字段
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <returns>是否移除成功</returns>
        public bool HashDelete(string key, string field)
        {
            key = AddKeyPrefix(key);
            return redisDB.HashDelete(key, field);
        }

        /// <summary>
        /// 移除hash中指定字段
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="fields">字段名集合</param>
        /// <returns>移除字段数量</returns>
        public long HashDelete(string key, IEnumerable<RedisValue> fields)
        {
            key = AddKeyPrefix(key);
            return redisDB.HashDelete(key, fields.ToArray());
        }

        /// <summary>
        /// 设置hash值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public bool HashSet(string key, string field, string value)
        {
            key = AddKeyPrefix(key);
            return redisDB.HashSet(key, field, value);
        }

        /// <summary>
        /// 设置hash值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="hashEntries">数据集合</param>
        public void HashSet(string key, IEnumerable<HashEntry> hashEntries)
        {
            key = AddKeyPrefix(key);
            redisDB.HashSet(key, hashEntries.ToArray());
        }

        /// <summary>
        /// 获取hash值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <returns>内容</returns>
        public RedisValue HashGet(string key, string field)
        {
            key = AddKeyPrefix(key);
            return redisDB.HashGet(key, field);
        }

        /// <summary>
        /// 获取hash值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="redisValues">字段名集合</param>
        /// <returns>内容集合</returns>
        public RedisValue[] HashGet(string key, RedisValue[] redisValues)
        {
            key = AddKeyPrefix(key);
            return redisDB.HashGet(key, redisValues);
        }

        /// <summary>
        /// 获取hash字段名集合
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>字段名集合</returns>
        public IEnumerable<RedisValue> HashKeys(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.HashKeys(key);
        }

        /// <summary>
        /// 获取hash值集合
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>值集合</returns>
        public RedisValue[] HashValues(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.HashValues(key);
        }

        /// <summary>
        /// 设置hash值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <param name="value">对象</param>
        /// <returns>是否成功</returns>
        public bool HashSet<T>(string key, string field, T value)
        {
            key = AddKeyPrefix(key);
            var serializeVal = SerializeToBinary(value);
            return redisDB.HashSet(key, field, serializeVal);
        }

        /// <summary>
        /// 获取hash值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <returns>内容</returns>
        public T HashGet<T>(string key, string field)
        {
            key = AddKeyPrefix(key);
            var value = redisDB.HashGet(key, field);
            return DeserializeToObject<T>(value);
        }

        #endregion

        #region  异步

        /// <summary>
        /// [异步]判断hash中是否有某个字段
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <returns>是否存在</returns>
        public async Task<bool> HashExistAsync(string key, string field)
        {
            key = AddKeyPrefix(key);
            return await redisDB.HashExistsAsync(key, field);
        }

        /// <summary>
        /// [异步]移除hash中指定字段
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <returns>是否移除成功</returns>
        public async Task<bool> HashDeleteAsync(string key, string field)
        {
            key = AddKeyPrefix(key);
            return await redisDB.HashDeleteAsync(key, field);
        }

        /// <summary>
        /// [异步]移除hash中指定字段
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="fields">字段名集合</param>
        /// <returns>移除字段数量</returns>
        public async Task<long> HashDeleteAsync(string key, IEnumerable<RedisValue> fields)
        {
            key = AddKeyPrefix(key);
            return await redisDB.HashDeleteAsync(key, fields.ToArray());
        }

        /// <summary>
        /// [异步]设置hash值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public async Task<bool> HashSetAsync(string key, string field, string value)
        {
            key = AddKeyPrefix(key);
            return await redisDB.HashSetAsync(key, field, value);
        }

        /// <summary>
        /// [异步]设置hash值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="hashEntries">数据集合</param>
        public async Task HashSetAsync(string key, IEnumerable<HashEntry> hashEntries)
        {
            key = AddKeyPrefix(key);
            await redisDB.HashSetAsync(key, hashEntries.ToArray());
        }

        /// <summary>
        /// [异步]获取hash值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <returns>内容</returns>
        public async Task<RedisValue> HashGetAsync(string key, string field)
        {
            key = AddKeyPrefix(key);
            return await redisDB.HashGetAsync(key, field);
        }

        /// <summary>
        /// [异步]获取hash值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="redisValues">字段名集合</param>
        /// <returns>内容集合</returns>
        public async Task<RedisValue[]> HashGetAsync(string key, RedisValue[] redisValues)
        {
            key = AddKeyPrefix(key);
            return await redisDB.HashGetAsync(key, redisValues);
        }

        /// <summary>
        /// [异步]获取hash字段名集合
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>字段名集合</returns>
        public async Task<IEnumerable<RedisValue>> HashKeysAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.HashKeysAsync(key);
        }

        /// <summary>
        /// [异步]获取hash值集合
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>值集合</returns>
        public async Task<RedisValue[]> HashValuesAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.HashValuesAsync(key);
        }

        /// <summary>
        /// [异步]设置hash值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <param name="value">对象</param>
        /// <returns>是否成功</returns>
        public async Task<bool> HashSetAsync<T>(string key, string field, T value)
        {
            key = AddKeyPrefix(key);
            var serializeVal = SerializeToBinary(value);
            return await redisDB.HashSetAsync(key, field, serializeVal);
        }

        /// <summary>
        /// [异步]获取hash值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="field">字段名</param>
        /// <returns>内容</returns>
        public async Task<T> HashGetAsync<T>(string key, string field)
        {
            key = AddKeyPrefix(key);
            var value = await redisDB.HashGetAsync(key, field);
            return DeserializeToObject<T>(value);
        }

        #endregion

        #endregion

        #region  List

        #region  同步

        /// <summary>
        /// 移除并返回列表的第一个元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>第一个元素值</returns>
        public string ListLeftPop(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.ListLeftPop(key);
        }

        /// <summary>
        /// 移除并返回列表的最后一个元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>最后一个元素</returns>
        public string ListRightPop(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.ListRightPop(key);
        }

        /// <summary>
        /// 移除列表指定健上与该值相同的元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public long ListRemove(string key, string value)
        {
            key = AddKeyPrefix(key);
            return redisDB.ListRemove(key, value);
        }

        /// <summary>
        /// 列表头部插入元素，如果不存在则先创建
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public long ListLeftPush(string key, string value)
        {
            key = AddKeyPrefix(key);
            return redisDB.ListLeftPush(key, value);
        }

        /// <summary>
        /// 列表尾部插入元素，如果不存在则先创建
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public long ListRightPush(string key, string value)
        {
            key = AddKeyPrefix(key);
            return redisDB.ListRightPush(key, value);
        }

        /// <summary>
        /// 返回列表上键值长度，没有则返回0
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>长度</returns>
        public long ListLength(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.ListLength(key);
        }

        /// <summary>
        /// 返回列表上键值所对应的元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>所有元素</returns>
        public IEnumerable<RedisValue> ListRange(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.ListRange(key);
        }

        /// <summary>
        /// 移除并返回列表的第一个元素（反序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <returns>第一个元素</returns>
        public T ListLeftPop<T>(string key)
        {
            key = AddKeyPrefix(key);
            return DeserializeToObject<T>(redisDB.ListLeftPop(key));
        }

        /// <summary>
        /// 移除并返回列表的最后一个元素（反序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <returns>第一个元素</returns>
        public T ListRightPop<T>(string key)
        {
            key = AddKeyPrefix(key);
            return DeserializeToObject<T>(redisDB.ListRightPop(key));
        }

        /// <summary>
        /// 列表头部插入元素，如果不存在则先创建（序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public long ListLeftPush<T>(string key, T value)
        {
            key = AddKeyPrefix(key);
            return redisDB.ListLeftPush(key, SerializeToBinary(value));
        }

        /// <summary>
        /// 列表尾部插入元素，如果不存在则先创建（序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public long ListRightPush<T>(string key, T value)
        {
            key = AddKeyPrefix(key);
            return redisDB.ListRightPush(key, SerializeToBinary(value));
        }

        #endregion

        #region  异步

        /// <summary>
        /// [异步]移除并返回列表的第一个元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>第一个元素值</returns>
        public async Task<string> ListLeftPopAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.ListLeftPopAsync(key);
        }

        /// <summary>
        /// [异步]移除并返回列表的最后一个元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>最后一个元素</returns>
        public async Task<string> ListRightPopAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.ListRightPopAsync(key);
        }

        /// <summary>
        /// [异步]移除列表指定健上与该值相同的元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public async Task<long> ListRemoveAsync(string key, string value)
        {
            key = AddKeyPrefix(key);
            return await redisDB.ListRemoveAsync(key, value);
        }

        /// <summary>
        /// [异步]列表头部插入元素，如果不存在则先创建
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public async Task<long> ListLeftPushAsync(string key, string value)
        {
            key = AddKeyPrefix(key);
            return await redisDB.ListLeftPushAsync(key, value);
        }

        /// <summary>
        /// [异步]列表尾部插入元素，如果不存在则先创建
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public async Task<long> ListRightPushAsync(string key, string value)
        {
            key = AddKeyPrefix(key);
            return await redisDB.ListRightPushAsync(key, value);
        }

        /// <summary>
        /// [异步]返回列表上键值长度，没有则返回0
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>长度</returns>
        public async Task<long> ListLengthAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.ListLengthAsync(key);
        }

        /// <summary>
        /// [异步]返回列表上键值所对应的元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>所有元素</returns>
        public async Task<IEnumerable<RedisValue>> ListRangeAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.ListRangeAsync(key);
        }

        /// <summary>
        /// [异步]移除并返回列表的第一个元素（反序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <returns>第一个元素</returns>
        public async Task<T> ListLeftPopAsync<T>(string key)
        {
            key = AddKeyPrefix(key);
            var value = await redisDB.ListLeftPopAsync(key);
            return DeserializeToObject<T>(value);
        }

        /// <summary>
        /// [异步]移除并返回列表的最后一个元素（反序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <returns>第一个元素</returns>
        public async Task<T> ListRightPopAsync<T>(string key)
        {
            key = AddKeyPrefix(key);
            var value = await redisDB.ListRightPopAsync(key);
            return DeserializeToObject<T>(value);
        }

        /// <summary>
        /// [异步]列表头部插入元素，如果不存在则先创建（序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public async Task<long> ListLeftPushAsync<T>(string key, T value)
        {
            key = AddKeyPrefix(key);
            return await redisDB.ListLeftPushAsync(key, SerializeToBinary(value));
        }

        /// <summary>
        /// [异步]列表尾部插入元素，如果不存在则先创建（序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public async Task<long> ListRightPushAsync<T>(string key, T value)
        {
            key = AddKeyPrefix(key);
            return await redisDB.ListRightPushAsync(key, SerializeToBinary(value));
        }

        #endregion

        #endregion

        #region  Set

        #region  同步

        /// <summary>
        /// 集合新增元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public bool SetAdd(string key, string value)
        {
            key = AddKeyPrefix(key);
            return redisDB.SetAdd(key, value);
        }

        /// <summary>
        /// 返回集合中所有元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>元素集合</returns>
        public IEnumerable<RedisValue> SetMembers(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.SetMembers(key);
        }

        /// <summary>
        /// 返回集合元素个数
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>元素个数</returns>
        public long SetLength(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.SetLength(key);
        }

        #endregion

        #region  异步

        /// <summary>
        /// [异步]集合新增元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public async Task<bool> SetAddAsync(string key, string value)
        {
            key = AddKeyPrefix(key);
            return await redisDB.SetAddAsync(key, value);
        }

        /// <summary>
        /// [异步]返回集合中所有元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns>元素集合</returns>
        public async Task<IEnumerable<RedisValue>> SetMembersAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.SetMembersAsync(key);
        }

        #endregion

        #endregion

        #region  SortSet

        #region  同步

        /// <summary>
        /// 有序集合新增元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public bool SortedSetAdd(string key, string memebr, double score)
        {
            key = AddKeyPrefix(key);
            return redisDB.SortedSetAdd(key, memebr, score);
        }

        /// <summary>
        /// 在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<RedisValue> SortedSetRangeByRank(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.SortedSetRangeByRank(key);
        }

        /// <summary>
        /// 返回有序集合的元素个数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SortedSetLength(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.SortedSetLength(key);
        }

        /// <summary>
        /// 移除有序集合元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="memebr"></param>
        /// <returns></returns>
        public bool SortedSetRemove(string key, string memebr)
        {
            key = AddKeyPrefix(key);
            return redisDB.SortedSetRemove(key, memebr);
        }

        /// <summary>
        /// 有序集合新增元素（序列化）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public bool SortedSetAdd<T>(string key, T member, double score)
        {
            key = AddKeyPrefix(key);
            var json = SerializeToBinary(member);

            return redisDB.SortedSetAdd(key, json, score);
        }

        #endregion

        #region  异步

        /// <summary>
        /// [异步]有序集合新增元素
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public async Task<bool> SortedSetAddAsync(string key, string memebr, double score)
        {
            key = AddKeyPrefix(key);
            return await redisDB.SortedSetAddAsync(key, memebr, score);
        }

        /// <summary>
        /// [异步]在有序集合中返回指定范围的元素，默认情况下从低到高。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RedisValue>> SortedSetRangeByRankAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.SortedSetRangeByRankAsync(key);
        }

        /// <summary>
        /// [异步]返回有序集合的元素个数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> SortedSetLengthAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.SortedSetLengthAsync(key);
        }

        /// <summary>
        /// [异步]移除有序集合元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="memebr"></param>
        /// <returns></returns>
        public async Task<bool> SortedSetRemoveAsync(string key, string memebr)
        {
            key = AddKeyPrefix(key);
            return await redisDB.SortedSetRemoveAsync(key, memebr);
        }

        /// <summary>
        /// [异步]有序集合新增元素（序列化）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="member"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public async Task<bool> SortedSetAddAsync<T>(string key, T member, double score)
        {
            key = AddKeyPrefix(key);
            var json = SerializeToBinary(member);

            return await redisDB.SortedSetAddAsync(key, json, score);
        }

        #endregion

        #endregion

        #region  发布订阅

        #region  同步

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="redisChannel"></param>
        /// <param name="action"></param>
        public void Subscribe(RedisChannel redisChannel, Action<RedisChannel, RedisValue> action)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            sub.Subscribe(redisChannel, action);
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="redisChannel"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public long Publish(RedisChannel redisChannel, RedisValue redisValue)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            return sub.Publish(redisChannel, redisValue);
        }

        #endregion

        #region  异步

        /// <summary>
        /// [异步]订阅
        /// </summary>
        /// <param name="redisChannel"></param>
        /// <param name="action"></param>
        public async Task SubscribeAsync(RedisChannel redisChannel, Action<RedisChannel, RedisValue> action)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            await sub.SubscribeAsync(redisChannel, action);
        }

        /// <summary>
        /// [异步]发布
        /// </summary>
        /// <param name="redisChannel"></param>
        /// <param name="redisValue"></param>
        /// <returns></returns>
        public async Task<long> PublishAsync(RedisChannel redisChannel, RedisValue redisValue)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            return await sub.PublishAsync(redisChannel, redisValue);
        }

        #endregion

        #endregion

        #region  公共方法

        /// <summary>
        /// 返回key值（默认key值和设置的key值拼接）
        /// </summary>
        /// <param name="key">设置的key值</param>
        /// <returns></returns>
        private string AddKeyPrefix(string key)
        {
            return $"{defaultKey}:{key}";
        }

        /// <summary>
        /// 生成一个随机数
        /// </summary>
        /// <returns></returns>
        private int AddtionExpireMinutes()
        {
            Random random = new Random();
            return random.Next(1, 5);
        }

        /// <summary>
        /// 对象序列化为二进制数据
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>二进制数据</returns>
        public byte[] SerializeToBinary(object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                byte[] data = ms.ToArray();
                return data;
            }
        }

        /// <summary>
        /// 二进制数据反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="data">二进制数据</param>
        /// <returns>对象</returns>
        public T DeserializeToObject<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                var result = (T)bf.Deserialize(ms);
                return result;
            }
        }

        #region  key值操作

        /// <summary>
        /// 对key值设置过期时间
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="expireMinutes">过期时间</param>
        /// <returns></returns>
        public bool KeyExpire(string key, int expireMinutes)
        {
            key = AddKeyPrefix(key);
            expireMinutes += AddtionExpireMinutes();
            return redisDB.KeyExpire(key, TimeSpan.FromMinutes(expireMinutes));
        }

        /// <summary>
        /// 移除key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyDelete(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.KeyDelete(key);
        }

        /// <summary>
        /// 移除key值
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public long KeyDelete(IEnumerable<string> keys)
        {
            var rediKeys = keys.Select(p => (RedisKey)AddKeyPrefix(p));
            return redisDB.KeyDelete(rediKeys.ToArray());
        }

        /// <summary>
        /// 检查是否存在key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.KeyExists(key);
        }

        /// <summary>
        /// 重命名key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public bool KeyRename(string key, string newKey)
        {
            key = AddKeyPrefix(key);
            newKey = AddKeyPrefix(newKey);
            return redisDB.KeyRename(key, newKey);
        }

        /// <summary>
        /// 获取key值剩余时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TimeSpan? GetRemainTime(string key)
        {
            key = AddKeyPrefix(key);
            return redisDB.KeyTimeToLive(key);
        }

        /// <summary>
        /// [异步]移除key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> KeyDeleteAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.KeyDeleteAsync(key);
        }

        /// <summary>
        /// [异步]移除key值
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public async Task<long> KeyDeleteAsync(IEnumerable<string> keys)
        {
            var rediKeys = keys.Select(p => (RedisKey)AddKeyPrefix(p));
            return await redisDB.KeyDeleteAsync(rediKeys.ToArray());
        }

        /// <summary>
        /// [异步]检查是否存在key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> KeyExistsAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.KeyExistsAsync(key);
        }

        /// <summary>
        /// [异步]重命名key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public async Task<bool> KeyRenameAsync(string key, string newKey)
        {
            key = AddKeyPrefix(key);
            newKey = AddKeyPrefix(newKey);
            var ss = redisDB.KeyTimeToLive(key);
            return await redisDB.KeyRenameAsync(key, newKey);
        }

        /// <summary>
        /// 获取key值剩余时间
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<TimeSpan?> GetRemainTimeAsync(string key)
        {
            key = AddKeyPrefix(key);
            return await redisDB.KeyTimeToLiveAsync(key);
        }

        #endregion

        #endregion
    }
}
