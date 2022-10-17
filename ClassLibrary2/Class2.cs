using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Czx
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FileExceptionAttribute : Attribute { }
}
namespace Czx.Forms
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class BlockMultiOpenAttribute : Attribute { }
}
namespace Czx.Https.Client
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CookieAttribute : Attribute
    {
        public string CookieName { get; set; }
        public string CookieValue { get; set; }
        public string CookiePath { get; set; }
        public string CookieDomain { get; set; }
        /// <summary>
        /// 将 Cookie 初始化，然后只调用一次。
        /// </summary>
        /// <param name="name">Cookie 名称</param>
        /// <param name="value">Cookie 值</param>
        /// <param name="path">Cookie 路径</param>
        /// <param name="domain">Cookie 领域</param>
        public CookieAttribute(string name, string value, string path, string domain)
        {
            CookieName = name;
            CookieValue = value;
            CookiePath = path;
            CookieDomain = domain;
        }
    }
}
namespace Czx.Sql
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class SqlConnectionAttribute : Attribute
    {
        /// <summary>
        /// 数据库连接配置。
        /// </summary>
        /// <param name="positionalString">连接字符串</param>
        public SqlConnectionAttribute(string positionalString)
        {
            SqlConnectionString = positionalString;
        }
        public string SqlConnectionString { get; set; }
    }
}