using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Management;
using System.Security.Cryptography;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Timers;
using System.Linq;
using Timer = System.Timers.Timer;

namespace Czx
{
    public static class Json
    {
        /// <summary>
        /// Json写入。
        /// </summary>
        /// <param name="obj">类型</param>
        /// <returns>返回缩进后的 JSON 数据。</returns>
        public static string WriteJson<T>(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream mstream = new MemoryStream();
            serializer.WriteObject(mstream, obj);
            byte[] Bytes = new byte[mstream.Length];
            mstream.Position = 0;
            mstream.Read(Bytes, 0, (int)mstream.Length);
            mstream.Close();
            mstream.Dispose();
            return Text.Indent(Encoding.UTF8.GetString(Bytes));
        }
        /// <summary>  
        /// Json读取。
        /// </summary>  
        /// <param name="data">Json数据</param>  
        /// <returns>返回 JSON 反序列化后的数据。</returns>  
        public static T ReadJson<T>(string data)
        {
            MemoryStream mstream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(mstream);
        }
    }


    public class File
    {
        static File() { }
        /// <summary>
        /// 创建文件夹。
        /// </summary>
        /// <param name="path">路径</param>
        public static void CreatFolder(string path)
        {
            Directory.CreateDirectory(path);
        }
        /// <summary>
        /// 创建文件。
        /// </summary>
        /// <param name="path">路径，路径后面是文件名称，支持后缀</param>
        public static string CreatFile(string path,string name)
        {
            name = name.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace(@"""", "").Replace("<", "").Replace(">", "").Replace("|", "");
            System.IO.File.Create(path + "\\" + name).Close();
            return path + "\\" + name;
        }
        /// <summary>
        /// 复制任意文件或文件夹到新的任意位置。可用 FileException 特性调出异常。
        /// </summary>
        /// <param name="source">源目标。</param>
        /// <param name="target">目标。</param>
        /// <returns>在不使用 FileException 特性的情况下方便测试，如果抛出异常，则 false，否则 true</returns>
        public static bool Copy(string source, string target)
        {
            return CopyOrMove(source, target, false);
        }
        /// <summary>
        /// 移动任意文件或文件夹到新的任意位置。可用 FileException 特性调出异常。
        /// </summary>
        /// <param name="source">源目标。</param>
        /// <param name="target">目标。</param>
        /// <returns>在不使用 FileException 特性的情况下方便测试，如果抛出异常，则 false，否则 true</returns>
        public static bool Move(string source, string target)
        {
            return CopyOrMove(source, target, true);
        }
        private static bool CopyOrMove(string source, string target, bool CM)
        {
            try
            {
                if (target[target.Length - 1] == Path.DirectorySeparatorChar)
                {
                    if (!Directory.Exists(target))
                    {
                        Directory.CreateDirectory(target);
                    }
                    foreach (var item in Directory.GetFileSystemEntries(source))
                    {
                        Copy(source, target + Path.GetFileName(item));
                        if (CM)
                        {
                            if (Exists(source))
                            {
                                Delete(source);
                            }
                            else if (Directory.Exists(source))
                            {
                                Directory.Delete(source);
                            }
                        }
                    }
                }
                else
                {
                    if (Exists(target))
                    {
                        new FileInfo(target).Delete();
                    }
                    new FileInfo(source).CopyTo(target);
                    if (CM)
                    {
                        if (Exists(source))
                        {
                            Delete(source);
                        }
                        else if (Directory.Exists(source))
                        {
                            Directory.Delete(source);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace();
                foreach (var item in trace.GetFrame(1).GetMethod().GetCustomAttributes(true))
                {
                    if (item != null)
                    {
                        if (item.GetType() == typeof(FileExceptionAttribute))
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
                return false;
            }
            return true;
        }
        /// <summary>
        /// 确定指定的文件是否存在。
        /// </summary>
        /// <param name="path">路径。</param>
        /// <returns>如果目录存在，则 true，否则 false。</returns>
        public static bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }
        /// <summary>
        /// 读取文件。
        /// </summary>
        /// <param name="path">路径，注意后缀</param>
        public static string Read(string path)
        {
            string result;
            try
            {
                StreamReader streamReader = new StreamReader(path);
                result = streamReader.ReadToEnd();
                streamReader.Close();
                streamReader.Dispose();
            }
            catch
            {
                return "";
            }
            return result;
        }
        /// <summary>
        /// 写入文件。
        /// </summary>
        /// <param name="path">路径，注意后缀</param>
        /// <param name="text">写入内容</param>
        public static void Write(string path, string text)
        {
            StreamWriter streamWriter = new StreamWriter(path);
            streamWriter.Write(text);
            streamWriter.Close();
            streamWriter.Dispose();
        }
        /// <summary>
        /// 删除文件夹或文件。
        /// </summary>
        /// <param name="path">路径，注意后缀</param>
        public static void Delete(string path)
        {
            FileInfo fi = new FileInfo(path);
            fi.Delete();
        }
        /// <summary>
        /// 当前用户的下载路径
        /// </summary>
        public static string DownloadPath
        {
            get
            {
                SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0, IntPtr.Zero, out string downloads);
                return downloads + "\\";
            }
        }
        /// <summary>
        /// Windows没有为“Downloads”文件夹定义CSIDL，所以无法通过Environment.SpecialFolder枚举来获取。
        /// 请注意，这是一个Vista和更高版本的API，不要试图在XP / 2003或更低版本上调用它。
        /// 导入shell32.dll，获取Downloads文件夹起源。
        /// </summary>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);
    }
    public class Text
    {
        /// <summary>
        /// 将某些代码缩进对齐。
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>返回不同的缩进后的 JSON 数据。</returns>
        public static string Indent(string data)
        {
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(data);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    Indentation = 2,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            return data;
        }
        /// <summary>
        /// 将文本全部转换为大写。
        /// </summary>
        /// <param name="text">数据</param>
        public static void ToUpper(ref string text)
        {
            text = text.ToUpper();
        }
        /// <summary>
        /// 将文本全部转换为小写。
        /// </summary>
        /// <param name="text">数据</param>
        public static void ToLower(ref string text)
        {
            text = text.ToLower();
        }
    }
    public class Maths
    {
        /// <summary>
        /// PI
        /// </summary>
        public static double PI
        {
            get { return Math.PI; }
        }
        /// <summary>
        /// 限制最大值。
        /// </summary>
        /// <param name="value">目标</param>
        /// <param name="max">最大值</param>
        public static void Max(ref double value, double max)
        {
            value = (value > max) ? max : value;
        }
        /// <summary>
        /// 限制最小值。
        /// </summary>
        /// <param name="value">目标</param>
        /// <param name="min">最小值</param>
        public static void Min(ref double value, double min)
        {
            value = (value < min) ? min : value;
        }
        /// <summary>
        /// 限制最小值和最大值。
        /// </summary>
        /// <param name="value">目标</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        public static void Clamp(ref double value, double min, double max)
        {
            value = (value < min) ? min : (value > max) ? max : value;
        }
        /// <summary>
        /// 绝对值。
        /// </summary>
        /// <param name="value">目标</param>
        public static void Abs(ref double value)
        {
            value = (0 < value) ? value : value - value - value;
        }
        /// <summary>
        /// 自动追加。
        /// </summary>
        /// <param name="value">目标</param>
        /// <param name="maxValue">追踪</param>
        /// <param name="speed">追加速度</param>
        public static void AutoAppend(ref double value,double maxValue,double speed)
        {
            if (maxValue < value)
            {
                value += speed;
            }
        }
        /// <summary>
        /// 自动追减。
        /// </summary>
        /// <param name="value">目标</param>
        /// <param name="maxValue">追踪</param>
        /// <param name="speed">追减速度</param>
        public static void AutoReduction(ref double value,double maxValue,double speed)
        {
            if (maxValue > value)
            {
                value -= speed;
            }
        }
        /// <summary>
        /// 自动追加减。
        /// </summary>
        /// <param name="value">目标</param>
        /// <param name="minValue">追踪</param>
        /// <param name="maxValue">追踪</param>
        /// <param name="speed">追减速度</param>
        public static void AutoAR(ref double value,double minValue,double maxValue,double speed)
        {
            if (maxValue < value)
            {
                value -= speed;
            }
            else if (minValue > value)
            {
                value += speed;
            }
        }
    }
}
namespace Czx.Info
{
    public static class Info
    {
        /// <summary>
        /// 版本。
        /// </summary>
        public static string Version
        {
            get { return "1.0.30"; }
        }
        /// <summary>
        /// 作者。
        /// </summary>
        public static string Writer
        {
            get { return "CZX"; }
        }
        /// <summary>
        /// 获取操作系统版本。
        /// </summary>
        public static string OSVersion
        {
            get { return Environment.OSVersion.VersionString; }
        }
        /// <summary>
        /// 获取IP地址。
        /// </summary>
        /// <returns>若没有IP地址则会返回“Unknown”，否则正确显示。</returns>
        public static string GetIP()
        {
            try
            {
                string str = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc.Cast<ManagementObject>())
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        Array ar = (Array)(mo.Properties["IpAddress"].Value);
                        str = ar.GetValue(0).ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return str;
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
namespace Czx.Security
{
    public static class DESCryp
    {
        private static readonly byte[] iv = { 2, 2, 4, 4, 6, 6, 8, 8 };
        private static readonly byte[] key = { 2, 2, 4, 4, 6, 6, 8, 8 };
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="text">将要加密的文本</param>
        /// <returns></returns>
        public static string StringToDESC(string text)
        {
            byte[] bytIn = Encoding.Default.GetBytes(text);
            try
            {
                DESCryptoServiceProvider mobjCryptoService = new DESCryptoServiceProvider
                {
                    Key = key,
                    IV = iv
                };
                ICryptoTransform encrypto = mobjCryptoService.CreateEncryptor();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
                cs.Write(bytIn, 0, bytIn.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="text">将要加密的文本</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns></returns>
        public static string StringToDESC(string text, byte[] key, byte[] iv)
        {
            byte[] bytIn = Encoding.Default.GetBytes(text);
            try
            {
                DESCryptoServiceProvider mobjCryptoService = new DESCryptoServiceProvider
                {
                    Key = key,
                    IV = iv
                };
                ICryptoTransform encrypto = mobjCryptoService.CreateEncryptor();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
                cs.Write(bytIn, 0, bytIn.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="text">将要解密的文本</param>
        /// <returns></returns>
        public static string DESCToString(string text)
        {
            byte[] bytIn = Convert.FromBase64String(text);
            try
            {
                DESCryptoServiceProvider mobjCryptoService = new DESCryptoServiceProvider
                {
                    Key = iv,
                    IV = key
                };
                MemoryStream ms = new MemoryStream(bytIn);
                ICryptoTransform encrypto = mobjCryptoService.CreateDecryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
                StreamReader strd = new StreamReader(cs, Encoding.Default);
                string result = strd.ReadToEnd();
                strd.Close();
                strd.Dispose();
                return strd.ReadToEnd();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="text">将要解密的文本</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns></returns>
        public static string DESCToString(string text, byte[] key, byte[] iv)
        {
            byte[] bytIn = Convert.FromBase64String(text);
            try
            {
                DESCryptoServiceProvider mobjCryptoService = new DESCryptoServiceProvider
                {
                    Key = iv,
                    IV = key
                };
                MemoryStream ms = new MemoryStream(bytIn);
                ICryptoTransform encrypto = mobjCryptoService.CreateDecryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
                StreamReader strd = new StreamReader(cs, Encoding.Default);
                string result = strd.ReadToEnd();
                strd.Close();
                strd.Dispose();
                return strd.ReadToEnd();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
namespace Czx.Graph
{
    public class Draw
    {
        /// <summary>
        /// 绘制 Microsoft 徽标
        /// </summary>
        /// <returns></returns>
        public static Bitmap Microsoft()
        {
            Bitmap bitmap = new Bitmap(300, 100);
            Graphics graphics = Graphics.FromImage(bitmap);
            int loc = (int)(bitmap.Height / 3.2);
            int reg = 25;
            int join = 27;
            Rectangle[] rectangles = new Rectangle[4];
            rectangles[0] = new Rectangle(loc, loc, reg, reg);
            rectangles[1] = new Rectangle(loc + join, loc, reg, reg);
            rectangles[2] = new Rectangle(loc, loc + join, reg, reg);
            rectangles[3] = new Rectangle(loc + join, loc + join, reg, reg);
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(243, 83, 37)), rectangles[0]);
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(129, 188, 6)), rectangles[1]);
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(5, 166, 240)), rectangles[2]);
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 186, 8)), rectangles[3]);
            graphics.DrawString("Microsoft", new Font("雅黑", 32, FontStyle.Bold), new SolidBrush(Color.White), bitmap.Width / 3.5F, bitmap.Height / 3);
            return bitmap;
        }
        /// <summary>
        /// 画圆
        /// </summary>
        /// <param name="color">颜色。</param>
        /// <param name="width">宽度。</param>
        /// <param name="height">高度。</param>
        /// <param name="fillStyle">填充样式。</param>
        /// <param name="penSize">填充样式。</param>
        /// <returns></returns>
        public static Bitmap Circle(Color color, int width, int height, FillStyle fillStyle, int penSize)
        {
            if (fillStyle == FillStyle.Solid)
            {
                int loc = 0;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.FillEllipse(new SolidBrush(color), new Rectangle(loc, loc, width, height));
                return bitmap;
            }
            else
            {
                int loc = penSize;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawEllipse(new Pen(color) { Width = penSize }, new Rectangle(loc, loc, width - penSize * 2, height - penSize * 2));
                return bitmap;
            }
        }
        /// <summary>
        /// 获取图像字节
        /// </summary>
        /// <param name="image">图像。</param>
        /// <param name="imageFormat">图像样式。</param>
        /// <returns></returns>
        public byte[] GetImageByte(Image image, ImageFormat imageFormat)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                image.Save(ms, imageFormat);
                byte[] vs = ms.GetBuffer();
                return vs;
            }
            catch (Exception)
            {
                Bitmap bitmap = new Bitmap(1, 1);
                bitmap.Save(ms, ImageFormat.Png);
                byte[] vs = ms.GetBuffer();
                return vs;
            }
            finally
            {
                ms.Close();
            }
        }
    }
    public enum FillStyle
    {
        Solid, Hollow
    }
}
namespace Czx.Forms
{
    public class Form : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Form mainForm;
        private static int runIndex = 0;
        public Form()
        {
            mainForm = this;
            mainForm.BackColor = Color.Black;
            mainForm.ForeColor = Color.Black;
            mainForm.Text = (runIndex > 0) ? $"New Title ({runIndex})" : "New Title";
            runIndex++;
            mainForm.Disposed += (object sender, EventArgs e) =>
            {
                runIndex--;
            };
        }

        /// <summary>
        /// 显示窗体
        /// 调用此方法体可能阻塞主线程。
        /// </summary>
        public void Run()
        {
            Application.Run(mainForm);
        }
        /// <summary>
        /// 显示窗体
        /// 调用此方法不会阻塞主线程。
        /// </summary>
        public new void Show()
        {
            mainForm.Show();
        }
    }
    public static class BlockMultiOpen
    {
        /// <summary>
        /// 阻止应用多开。由特性进行执行。
        /// </summary>
        public static void Block()
        {
            StackTrace trace = new StackTrace();
            foreach (var item in trace.GetFrame(1).GetMethod().GetCustomAttributes(true))
            {
                if (item != null)
                {
                    if (item.GetType() == typeof(BlockMultiOpenAttribute))
                    {
                        BlockBycustom(Process.GetCurrentProcess().ProcessName);
                        return;
                    }
                }
            }
            foreach (var item in trace.GetFrame(1).GetMethod().DeclaringType.GetCustomAttributes(true))
            {
                if (item != null)
                {
                    if (item.GetType() == typeof(BlockMultiOpenAttribute))
                    {
                        BlockBycustom(Process.GetCurrentProcess().ProcessName);
                    }
                }
            }
        }
        /// <summary>
        /// 阻止应用多开。由自定义进行执行。
        /// </summary>
        /// <param name="processName"></param>
        public static void Block(string processName)
        {
            BlockBycustom(processName);
        }
        private static void BlockBycustom(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 1)
            {
                Environment.Exit(1);
            }
        }
    }
}
namespace Czx.Forms.Administration
{
    public class Administration
    {
        /// <summary>
        /// 获取当前用户是否为管理员。
        /// </summary>
        public static bool IsAdministrator
        {
            get
            {
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }
        /// <summary>
        /// 启动管理员身份。
        /// </summary>
        /// <param name="workingDirectory">运行根目录。</param>
        /// <param name="fileName">运行目录。</param>
        /// <param name="isExit">如果不是管理员身份，则退出。</param>
        public static void RunAdministrator(string workingDirectory, string fileName, bool isExit)
        {
            if (IsAdministrator)
            {
                return;
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = fileName,
                Verb = "runas"
            };
            try
            {
                Process.Start(startInfo);
                Environment.Exit(1);
            }
            catch
            {
                if (isExit)
                {
                    Environment.Exit(1);
                }
            }
        }
        /// <summary>
        /// 启动管理员身份。
        /// </summary>
        /// <param name="isExit">如果不是管理员身份，则退出。</param>
        public static void RunAdministrator(bool isExit)
        {
            RunAdministrator(Environment.CurrentDirectory, Application.ExecutablePath, isExit);
        }
        /// <summary>
        /// 如果不是管理员身份，则退出。
        /// </summary>
        public static void RunAdministrator()
        {
            RunAdministrator(true);
        }
    }
}
namespace Czx.Https.Client
{
    public class WebClient
    {
        private CookieContainer cookieContainer;
        private WebHeaderCollection headers;

        public CookieContainer CookieContainer { get => cookieContainer; set => cookieContainer = value; }
        public WebHeaderCollection Headers { get => headers; set => headers = value; }

        /// <summary>
        /// 初始化 Client。如需要特性，可添加特性来初始化。
        /// </summary>
        public WebClient()
        {
            cookieContainer = new CookieContainer();
            headers = new WebHeaderCollection();
            StackTrace trace = new StackTrace();
            CookieAttribute cookieAttribute = null;
            foreach (var item in trace.GetFrame(1).GetMethod().GetCustomAttributes(true))
            {
                if (item != null)
                {
                    if (item.GetType() == typeof(CookieAttribute))
                    {
                        cookieAttribute = (CookieAttribute)item;
                        CookieContainer.Add(new Cookie(((CookieAttribute)item).CookieName, ((CookieAttribute)item).CookieValue, ((CookieAttribute)item).CookiePath, ((CookieAttribute)item).CookieDomain));
                    }
                }
            }
            if (cookieAttribute == null)
            {
                foreach (var item in trace.GetFrame(1).GetMethod().DeclaringType.GetCustomAttributes(false))
                {
                    if (item != null)
                    {
                        if (item.GetType() == typeof(CookieAttribute))
                        {
                            CookieContainer.Add(new Cookie(((CookieAttribute)item).CookieName, ((CookieAttribute)item).CookieValue, ((CookieAttribute)item).CookiePath, ((CookieAttribute)item).CookieDomain));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 下载文件，并收集文件流信息。
        /// </summary>
        /// <param name="uri">网址。</param>
        /// <param name="path">保存路径。</param>
        /// <returns></returns>
        public WebFileInfo DownloadFile(string uri,string path)
        {
            WebFileInfo fileInfo = new WebFileInfo
            {
                CookieContainer = CookieContainer
            };
            fileInfo.DownloadFile(uri, path);
            return fileInfo;
        }
        /// <summary>
        /// 添加 Cookie
        /// </summary>
        /// <param name="name">Cookie 名称</param>
        /// <param name="value">Cookie 值</param>
        /// <param name="path">Cookie 路径</param>
        /// <param name="domain">Cookie 领域</param>
        public void AddCookie(string name, string value, string path, string domain)
        {
            CookieContainer.Add(new Cookie(name, value, path, domain));
        }
    }
    public class WebFileInfo
    {
        public CookieContainer CookieContainer;
        public WebHeaderCollection Headers;
        private int lastLength = 0;
        private int nowLength = 0;
        private Thread thread;
        private Timer timer = new Timer();
        /// <summary>
        /// 下载文件，并收集文件流信息。
        /// </summary>
        /// <param name="uri">网址。</param>
        /// <param name="path">保存路径。</param>
        public void DownloadFile(string uri, string path)
        {
            if (string.IsNullOrEmpty(uri))
            {
                isCompleted = true;
                DownloadCompleted?.Invoke();
                return;
            }
            thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    HttpWebRequest webRequest = WebRequest.CreateHttp(uri);
                    webRequest.CookieContainer = CookieContainer;
                    webRequest.Headers = Headers;
                    webRequest.KeepAlive = true;
                    webRequest.Method = "GET";
                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.4985.0 Safari/537.36 Edg/102.0.1235.0";
                    HttpWebResponse httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
                    long totalBytes = httpWebResponse.ContentLength;
                    contentLength = (int)totalBytes;
                    string newFileName = "";
                    foreach (var item in httpWebResponse.Headers.AllKeys)
                    {
                        foreach (var item2 in httpWebResponse.Headers.GetValues(item))
                        {
                            if (item2.Contains(".exe"))
                            {
                                newFileName = item2.Substring(item2.LastIndexOf('='));
                                break;
                            }
                        }
                    }
                    Stream stream = httpWebResponse.GetResponseStream();
                    FileStream fileStream = null;
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(Path.GetDirectoryName(path));
                    if (!string.IsNullOrEmpty(newFileName))
                    {
                        stringBuilder.Append(newFileName);
                    }
                    else
                    {
                        foreach (var item in Path.GetFileName(path))
                        {
                            bool b = true;
                            foreach (var item2 in Path.GetInvalidFileNameChars())
                            {
                                if (item == item2)
                                {
                                    b = false;
                                    continue;
                                }
                            }
                            if (b)
                            {
                                stringBuilder.Append(item);
                            }
                        }
                    }
                    FileName = File.DownloadPath + stringBuilder.ToString().Substring(stringBuilder.ToString().LastIndexOf('=') + 1);
                    try
                    {
                        fileStream = new FileStream(FileName, FileMode.CreateNew);
                    }
                    catch
                    {
                        for (int i = 1; ; i++)
                        {
                            try
                            {
                                fileStream = new FileStream(Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + $" ({i}).{Path.GetExtension(FileName)}", FileMode.CreateNew);
                                break;
                            }
                            catch { }
                        }
                    }
                    long totalDownloadedByte = 0;
                    byte[] by = new byte[1024 * 1024];
                    int osize = stream.Read(by, 0, by.Length);
                    timer.Interval = 1000;
                    timer.Elapsed += (s, e) =>
                    {
                        SpeedText = $"{(nowLength - lastLength) / 1024} KB/S";
                        lastLength = nowLength;
                    };
                    timer.Start();
                    while (osize > 0)
                    {
                        try
                        {
                            if (CanDownload)
                            {
                                totalDownloadedByte += osize;
                                nowLength = (int)totalDownloadedByte;
                                fileStream.Write(by, 0, osize);
                                osize = stream.Read(by, 0, by.Length);
                                value = (int)((double)totalDownloadedByte / (double)totalBytes * 100);
                                Refesh();
                                Application.DoEvents();
                            }
                        }
                        catch
                        {
                            fileStream.Close();
                            stream.Close();
                            fileStream.Dispose();
                            stream.Dispose();
                            isCompleted = true;
                            if (File.Exists(FileName))
                            {
                                File.Delete(FileName);
                            }
                            break;
                        }
                    }
                    webRequest.Abort();
                    fileStream.Close();
                    stream.Close();
                    fileStream.Dispose();
                    stream.Dispose();
                }
                catch { }
                isCompleted = true;
                DownloadCompleted();
            }));
            thread.Start();
        }
        /// <summary>
        /// 刷新下载信息。
        /// </summary>
        public void Refesh()
        {
            Downloading?.Invoke();
        }
        /// <summary>
        /// 终止下载。
        /// </summary>
        public void ExitDownload()
        {
            if (thread != null)
            {
                thread.Abort();
            }
        }
        /// <summary>
        /// 停止下载。
        /// </summary>
        public void StopDownload()
        {
            timer.Stop();
            CanDownload = false;
        }
        /// <summary>
        /// 开始下载。
        /// </summary>
        public void StartDownload()
        {
            timer.Start();
            CanDownload = true;
        }
        private bool CanDownload = true;
        /// <summary>
        /// 获取文件是否下载完成。
        /// </summary>
        public bool IsCompleted
        {
            get => isCompleted;
        }
        private bool isCompleted = false;
        /// <summary>
        /// 获取文件总大小。
        /// </summary>
        public int ContentLength
        {
            get => contentLength;
        }
        private int contentLength = 0;
        /// <summary>
        /// 获取下载进度，0-100。
        /// </summary>
        public int Value
        {
            get => value;
        }
        private int value = 0;
        public string FileName = "";
        public string Text = "";
        public string SpeedText = "";
        public delegate void DownloadingHandle();
        public event DownloadingHandle Downloading;
        public delegate void DownloadCompletedHandle();
        public event DownloadCompletedHandle DownloadCompleted;
    }
}
namespace Czx.Sql
{
    public class SqlClient : IDisposable
    {
        public SqlConnection Connection { get; set; }
        public SqlCommand Command { get; set; }
        public SqlDataReader DataReaderForSql
        {
            get { return reader; }
        }
        public XmlReader DataReaderForXml
        {
            get { return xmlReader; }
        }
        private SqlDataReader reader;
        private XmlReader xmlReader;
        private bool disposedValue;

        /// <summary>
        /// 初始化 SQL 客户端
        /// </summary>
        public SqlClient()
        {
            StackTrace trace = new StackTrace();
            SqlConnectionAttribute sqlConnectionAttribute = null;
            foreach (var item in trace.GetFrame(1).GetMethod().GetCustomAttributes(true))
            {
                if (item != null)
                {
                    if (item.GetType() == typeof(SqlConnectionAttribute))
                    {
                        sqlConnectionAttribute = (SqlConnectionAttribute)item;
                        Connection = new SqlConnection(((SqlConnectionAttribute)item).SqlConnectionString);
                    }
                }
            }
            if (sqlConnectionAttribute == null)
            {
                foreach (var item in trace.GetFrame(1).GetMethod().DeclaringType.GetCustomAttributes(false))
                {
                    if (item != null)
                    {
                        if (item.GetType() == typeof(SqlConnectionAttribute))
                        {
                            Connection = new SqlConnection(((SqlConnectionAttribute)item).SqlConnectionString);
                        }
                    }
                }
            }
            Connection.Open();
            Command = new SqlCommand();
        }
        /// <summary>
        /// 初始化 SQL 客户端
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public SqlClient(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
            Connection.Open();
            Command = new SqlCommand();
        }
        /// <summary>
        /// 执行语句，并生成错误信息。
        /// </summary>
        /// <param name="commandString">语句。</param>
        /// <returns>错误信息。</returns>
        public string Exucute(string commandString)
        {
            try
            {
                Command.Connection = Connection;
                Command.CommandText = commandString;
                Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }
        /// <summary>
        /// 执行语句，并生成 SqlDataReader 。
        /// </summary>
        /// <param name="commandString">语句。</param>
        /// <returns>SqlDataReader 数据。</returns>
        public SqlDataReader ExecuteReader(string commandString)
        {
            try
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
                Command.Connection = Connection;
                Command.CommandText = commandString;
                reader = Command.ExecuteReader();
                return reader;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 执行语句，并生成结果中的第一行的第一列。
        /// </summary>
        /// <param name="commandString">语句</param>
        /// <returns></returns>
        public object ExecuteScalar(string commandString)
        {
            try
            {
                Command.Connection = Connection;
                Command.CommandText = commandString;
                return Command.ExecuteScalar();
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 执行语句，并生成 XmlReader 。
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns>XmlReader 数据。</returns>
        public object ExecuteXmlReader(string commandString)
        {
            try
            {
                Command.Connection = Connection;
                Command.CommandText = commandString;
                xmlReader = Command.ExecuteXmlReader();
                return xmlReader;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 确定指定的数据库是否存在。
        /// </summary>
        /// <param name="databaseName">数据库名。</param>
        /// <returns></returns>
        public bool CheckDatabaseExists(string databaseName)
        {
            {
                string sqlCreateDBQuery;
                bool result;
                try
                {
                    sqlCreateDBQuery = $"SELECT database_id from sys.databases WHERE Name  = '{databaseName}';";
                    SqlCommand sqlCmd = new SqlCommand(sqlCreateDBQuery, Connection);
                    object resultObj = sqlCmd.ExecuteScalar();
                    int databaseID = 0;
                    if (resultObj != null)
                    {
                        int.TryParse(resultObj.ToString(), out databaseID);
                    }
                    result = (databaseID > 0);
                }
                catch (Exception)
                {
                    result = false;
                }
                return result;
            }
        }
        /// <summary>
        /// 确定指定的表是否存在，并确保已选择的数据库。
        /// </summary>
        /// <param name="databaseName">数据库名。</param>
        /// <param name="tableName">表名。</param>
        /// <returns></returns>
        public bool CheckTableExists(string tableName)
        {
            bool hr = false;
            try
            {
                Command.CommandText = $"select * from sys.tables where name ='{tableName}';";
                SqlDataReader reader = Command.ExecuteReader();
                hr = reader.HasRows;
                reader.Close();
            }
            catch { }
            return hr;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Connection.Close();
                    Connection.Dispose();
                    Command.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
namespace Czx.Html
{
    public class ElementControl
    {
        /// <summary>
        /// 获取元素中的值
        /// </summary>
        /// <param name="oriText">原文</param>
        /// <param name="outText">要寻找的元素</param>
        /// <returns>返回找到的第一个值</returns>
        public static string GetElementString(string oriText, string findText)
        {
            if (oriText.Contains(findText))
            {
                Regex regex = new Regex(findText + "=\"\\S+\"");
                string renew = regex.Match(oriText).Value.Replace(findText + "=", "").Remove(0, 1);
                renew = renew.Remove(renew.Length - 1, 1);
                return renew;
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 获取多个元素中的值
        /// </summary>
        /// <param name="oriText">原文</param>
        /// <param name="findText">要寻找的元素</param>
        /// <returns>返回找到的多个值</returns>
        public static string[] GetElementStrings(string oriText, string findText)
        {
            List<string> list = new List<string>();
            Regex regex = new Regex(findText + "=\"\\S+\"");
            IEnumerator matchEnum = regex.Matches(oriText).GetEnumerator();
            while (matchEnum.MoveNext() && matchEnum.Current != null)
            {
                string renew = ((Match)matchEnum.Current).Value.Replace(findText + "=", "").Remove(0, 1);
                renew = renew.Remove(renew.Length - 1, 1);
                list.Add(renew);
            }
            return list.ToArray();
        }
    }
}
namespace Czx.Controller
{
    public class Win32Api
    {
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        [DllImport("Kernel32")]
        public static extern void SetConsoleCtrlHandler(HandlerRoutine HandlerRoutine, bool Add);
        public delegate bool HandlerRoutine(uint dwControlType);
        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);
        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int vKey);
    }
    public class MouseListener
    {
        private Point point;
        public Point Point
        {
            get { return point; }
            set
            {
                if (point != value)
                {
                    point = value;
                    if (MouseMoveEvent != null)
                    {
                        var e = new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0);
                        MouseMoveEvent(this, e);
                    }
                }
            }
        }
        private static bool canStart = true;
        private int hHook;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;

        public const int WH_MOUSE_LL = 14;
        public Win32Api.HookProc hProc;
        public MouseListener()
        {
            this.Point = new Point();
        }
        public void Start()
        {
            if (!canStart)
            {
                throw new Exception("已经有一个在监听了。");
            }
            else
            {
                hProc = new Win32Api.HookProc(MouseHookProc);
                hHook = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
                if (hHook == 0)
                {
                    Stop();
                    throw new Exception("安装鼠标钩子失败。");
                }
                canStart = false;
            }
        }
        public void Stop()
        {
            bool retMouse = true;
            if (hHook != 0)
            {
                retMouse = Win32Api.UnhookWindowsHookEx(hHook);
                hHook = 0;
            }
            if (!retMouse) throw new Exception("卸载鼠标钩子失败。");
            canStart = true;
        }
        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            if (nCode < 0)
            {
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {
                MouseButtons button = MouseButtons.None;
                int clickCount = 0;
                switch ((Int32)wParam)
                {
                    case WM_LBUTTONDOWN:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        MouseDownEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_RBUTTONDOWN:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        MouseDownEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_MBUTTONDOWN:
                        button = MouseButtons.Middle;
                        clickCount = 1;
                        MouseDownEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_LBUTTONUP:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        MouseUpEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_RBUTTONUP:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        MouseUpEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_MBUTTONUP:
                        button = MouseButtons.Middle;
                        clickCount = 1;
                        MouseUpEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_LBUTTONDBLCLK:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        MouseClickEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_MBUTTONDBLCLK:
                        button = MouseButtons.Middle;
                        clickCount = 1;
                        MouseClickEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_RBUTTONDBLCLK:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        MouseClickEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_MOUSEMOVE:
                        button = MouseButtons.None;
                        clickCount = 1;
                        MouseMoveEvent.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                }
                this.Point = new Point(MyMouseHookStruct.pt.X, MyMouseHookStruct.pt.Y);
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }
        public delegate void MouseMoveHandler(object sender, MouseEventArgs e);
        public event MouseMoveHandler MouseMoveEvent;
        public delegate void MouseClickHandler(object sender, MouseEventArgs e);
        public event MouseClickHandler MouseClickEvent;
        public delegate void MouseDownHandler(object sender, MouseEventArgs e);
        public event MouseDownHandler MouseDownEvent;
        public delegate void MouseUpHandler(object sender, MouseEventArgs e);
        public event MouseUpHandler MouseUpEvent;
        ~MouseListener()
        {
            Stop();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public class MouseHookStruct
    {
        public Point pt;
        public int hwnd;
        public int wHitTestCode;
        public int dwExtraInfo;
    }
    public static class MouseHandle
    {
        [DllImport("user32")]
        public static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        public const int MOUSEEVENTF_MOVE = 0x0001;
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const int MOUSEEVENTF_WHEEL = 0x0800;
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
        public static int LClick()
        {
            return mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        public static int MClick()
        {
            return mouse_event(MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
        }
        public static int RClick()
        {
            return mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
    }
    public class KeyboardListener
    {
        public event KeyEventHandler KeyDownEvent;
        public event KeyPressEventHandler KeyPressEvent;
        public event KeyEventHandler KeyUpEvent;
        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        private int hKeyboardHook = 0;
        public const int WH_KEYBOARD_LL = 13;
        private static bool canStart = true;
        Win32Api.HookProc KeyboardHookProcedure;
        public void Start()
        {
            if (!canStart) throw new Exception("已经有一个在监听了。");
            else if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new Win32Api.HookProc(KeyboardHookProc);
                hKeyboardHook = Win32Api.SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, Win32Api.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("安装键盘钩子失败。");
                }
                canStart = false;
            }
        }
        public void Stop()
        {
            bool retKeyboard = true;
            if (hKeyboardHook != 0)
            {
                retKeyboard = Win32Api.UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }
            if (!retKeyboard) throw new Exception("卸载键盘钩子失败！");
            canStart = true;
        }

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        private int KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (KeyDownEvent != null || KeyUpEvent != null || KeyPressEvent != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                if (KeyDownEvent != null && ((Int32)wParam == WM_KEYDOWN || (Int32)wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyDownEvent(this, e);
                }
                if (KeyPressEvent != null && (Int32)wParam == WM_KEYDOWN)
                {
                    byte[] keyState = new byte[256];
                    Win32Api.GetKeyboardState(keyState);

                    byte[] inBuffer = new byte[2];
                    if (Win32Api.ToAscii(MyKeyboardHookStruct.vkCode, MyKeyboardHookStruct.scanCode, keyState, inBuffer, MyKeyboardHookStruct.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        KeyPressEvent(this, e);
                    }
                }
                if (KeyUpEvent != null && ((Int32)wParam == WM_KEYUP || (Int32)wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyUpEvent(this, e);
                }

            }
            return Win32Api.CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }
        ~KeyboardListener()
        {
            Stop();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public class KeyboardHookStruct
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }
}