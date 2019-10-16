using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace CrazyRecite
{
    /// <summary>x
    /// 对App.Config文件中AppSetting的操作
    /// </summary>
    public static class ConfigurationUtil
    {

        /// <summary>
        /// 获取Configuration对象
        /// </summary>
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        #region 获取配置文件中对应key的值
        /// <summary>
        /// 获取配置文件中对应key的值
        /// </summary>
        /// <param name="key">需要查询的key</param>
        /// <returns>返回Value</returns>
        public static string GetValue(string key)
        {
            string value = "";//初始化值
            if(config.AppSettings.Settings[key]!=null)
                value = config.AppSettings.Settings[key].Value;
            //返回值
            return value;
        }
        #endregion

        #region 修改对应key的Value
        /// <summary>
        /// 设置对应key的Value
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void SetValue(string key, string value)
        {
            //写入<add>元素的Value
            add(key, value);
            config.AppSettings.Settings[key].Value = value;
            saveAndRefash();
        }
        #endregion

        #region 删除对应的节点
        /// <summary>
        /// 删除对应的节点
        /// </summary>
        public static void delete(string key)
        {
            if (config.AppSettings.Settings[key] != null)
            {
                //删除<add>元素
                config.AppSettings.Settings.Remove(key);
                saveAndRefash();
            }
        }
        #endregion

        #region 添加节点
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void add(string key, string value)
        {
            //增加<add>元素
            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value);
                saveAndRefash();
            }
        }
        #endregion

        #region 保存和刷新(private)
        /// <summary>
        /// 保存和刷新(更改完了一定要记得保存和刷新噢)
        /// </summary>
        private static void saveAndRefash()
        {
            config.Save();//保存
            /*
            //一定要记得保存，写不带参数的config.Save()也可以
            config.Save(ConfigurationSaveMode.Modified);
            */
            //刷新，否则程序读取的还是之前的值（可能已装入内存）
            ConfigurationManager.RefreshSection("appSettings");
        }
        #endregion

    }
}
