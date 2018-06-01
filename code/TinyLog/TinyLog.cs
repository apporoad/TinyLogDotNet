using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TinyLog
{
    /// <summary>
    /// 微型日志：
    /// 适用场景：
    ///     底层组件
    ///     简单小项目
    ///     不想用大的日志框架的测试场景
    ///     
    ///     可以编译成独立组件，也可以直接集成到项目中
    /// 支持以下特性：
    ///     1、日志格式非常简单、所有数据存在一个文件中
    ///     2、日志文件支持分割，如果数据超过10M放到.old文件中，同时将之前的old文件删除
    ///     3、日志支持高频写入，输入暂存到内存，每1分钟刷到文件，如果内存数据量大于64KB，将触发立即灌入文件
    ///     4、如果日志数据过于多，5S中总数据量超过524288（512KB），多出的数据将被丢弃
    ///     5、日志路径和日志名称支持简单动态配置
    ///     6、日志组件内部错误将以innerFatal方式也写入日志中
    ///     7、日志存在开关IsOn，可以动态开关日志
    ///     8、增加内部异常关闭自动关闭机制
    /// </summary>
    public class TinyLog
    {
        public static bool IsOn { get; set; }

        private static string formatString = "[时间：{0}级别：{1} 日志内容]：{2}";

        private static StringBuilder sb = new StringBuilder();

        /// <summary>
        /// 查看缓存中的日志内容
        /// </summary>
        public static string LogContent { get { return sb.ToString(); } }

        private static object lockObj = new object();

        private static Thread thread;

        private static DateTime? nextRunTime = null;

        //刷新间隔
        private static int refreshInternal = 60000;

        //最快落文件间隔
        private static int unitInternal = 5000;

        //日志缓存建议长度 64KB×2
        private static int cacheLogLength = 65536;

        //最大缓存大小，大于这个不在记录
        private static int cacheLogMaxLength = 524288;

        //日志名
        private static string logFileName = "default.log";

        //日志路径
        private static string logPath = "logs";

        //文件最大大小 最大10M
        private static long fileMaxLength = 10485760;

        //内部错误次数，默认为3，超过该次数，日志自动降级
        private static int errorRetry = 3;

        //错误次数
        private static int errorCount = 0;

        static TinyLog()
        {
            IsOn = true;
            //变更区域 初始化10000
            formatString = "[时间：{0}级别：{1} 日志内容]：{2}";
            sb = new StringBuilder(10000);
            refreshInternal = 60000;
            unitInternal = 5000;
            cacheLogLength = 65536;
            cacheLogMaxLength = 524288;
            logFileName = "default.log";
            logPath = "logs";
        }

        public void Init(string logName, string logFolderName)
        {
            logFileName = logName;
            if (string.IsNullOrEmpty(logFolderName) == false)
                logPath = logFolderName;
        }

        public void Error(string strInfo)
        {
            InnerWrite("ERROR", strInfo ?? "");
        }

        public void Debug(string strInfo)
        {
            InnerWrite("DEBUG", strInfo ?? "");
        }

        public void Fatal(string strInfo)
        {
            InnerWrite("FATAL", strInfo ?? "");
        }

        public void Info(string strInfo)
        {
            InnerWrite("INFO", strInfo ?? "");
        }

        public void Warn(string strInfo)
        {
            InnerWrite("WARN", strInfo ?? "");
        }

        private static void InnerWrite(string head, string info)
        {
            //日志关闭时，不写入日志
            if (!IsOn)
                return;
            lock (lockObj)
            {
                if (sb.Length < cacheLogMaxLength)
                {
                    sb.AppendLine(string.Format(formatString, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), head, info));
                    if (sb.Length > cacheLogLength)
                    {
                        nextRunTime = DateTime.Now;
                    }
                }
                else
                {
                    nextRunTime = DateTime.Now;
                }

                //首次启动
                if (thread == null)
                {
                    thread = new Thread(new ThreadStart(writeSb2File));
                    //设置为后台线程
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
        }

        private static void writeSb2File()
        {
            while (true)
            {
                if (IsOn || sb.Length > 0)
                {
                    try
                    {
                        if (nextRunTime.HasValue == false)
                        {
                            nextRunTime = DateTime.Now.AddMilliseconds(refreshInternal);
                        }
                        if (DateTime.Now > nextRunTime)
                        {
                            FileSave();
                            nextRunTime = DateTime.Now.AddMilliseconds(refreshInternal);
                        }
                    }
                    catch (Exception ex)
                    {
                        InnerWrite("innerFatal", "日志数据写入到文件错误:" + ex.ToString());
                        innerError();
                    }
                }
                Thread.Sleep(unitInternal);
            }
        }

        private static void FileSave()
        {
            string filePath = null;
            var strSb = "";
            //获取文件路径
            if (Path.IsPathRooted(logPath) == false)
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logPath, logFileName);
            }
            else
            {
                filePath = Path.Combine(logPath, logFileName);
            }
            if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            //文件是否存在
            if (File.Exists(filePath))
            {
                //判断旧文件大小，如果超过就存放到.old中
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length > fileMaxLength)
                {
                    if (File.Exists(filePath + ".old"))
                    {
                        File.Delete(filePath + ".old");
                    }
                    fi.MoveTo(filePath + ".old");
                    //重新写入到文件
                    lock (lockObj)
                    {
                        strSb = sb.ToString();
                        sb.Clear();
                    }
                    File.WriteAllText(filePath, strSb, Encoding.UTF8);

                }
                else
                {
                    lock (lockObj)
                    {
                        strSb = sb.ToString();
                        sb.Clear();
                    }
                    File.AppendAllText(filePath, strSb, Encoding.UTF8);
                }
            }
            else
            {
                lock (lockObj)
                {
                    strSb = sb.ToString();
                    sb.Clear();
                }
                File.WriteAllText(filePath, strSb, Encoding.UTF8);
            }

        }

        /// <summary>
        /// 内部错误
        /// </summary>
        private static void innerError()
        {
            errorCount++;
            if (errorCount > errorRetry)
            {
                InnerWrite("innerFatal", "内部出错次数为" +errorCount + " 并已经重置");
                IsOn = false;
                errorCount = 0;
            }
        }

    }
}
