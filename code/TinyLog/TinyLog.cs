using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace TinyLog
{
    #region edit region 编辑区域
    /// <summary>
    ///  日志配置类，外部修改配置，请在这里扩展
    ///  log config , you can edit here 
    /// </summary>
    internal class TinyLogConfig
    {
        //是否开启日志
        // is on log
        public bool isOn = true;

        //是否开启归档
        public bool isAchiveOn = true;

        //刷新间隔 10秒刷一次
        // internal of refresh , it controls how fast your log down to your file
        public int refreshInternal = 10000;

        //最快落文件间隔
        // fastest internal of memory to file
        public int unitInternal = 2000;

        //日志缓存建议长度 64KB×2
        // memory cache suggested length ,if memory size > it , it should write to file ,but it depends on unitInternal
        public int cacheLogLength = 65536;

        //最大缓存大小，大于这个不在记录
        //max memory cache length,  if memory size is > it ,new log content will be ignore
        public int cacheLogMaxLength = 524288;

        //日志名
        // default log file name
        public string logFileName = "default.log";

        //日志路径
        // default log file path
        public string logPath = "TinyLogs";

        //文件最大大小 最大10M
        // max file length 
        public long fileMaxLength = 10485760;

        //内部错误次数，默认为3，超过该次数，日志自动降级
        // max inner error retry count
        public int errorRetry = 3;

        //日志格式
        // format of log
        public string formatString = "[时间：{0}级别：{1} 日志内容]：{2}";

        #region 归档部分 archive

        //压缩器 your compressor
        public ICompress comperessor = new Gzip();

        //归档文件夹最大大小 默认100M
        // max length of your archive dir ,if bigger than it ,dir will be clear 
        public long archiveDirMaxLength = 104857600;

        #endregion

    }
    #endregion

    /// <summary>
    /// tinyLog：
    /// use case：
    ///     component in any program
    ///     easy to integrate
    ///     some test scene
    ///     
    ///     it can be used as a inpendent dll ,or integrated into your program
    /// 
    /// supports：
    ///     1、日志格式非常简单、所有数据存在一个文件中
    ///     2、日志文件支持分割，如果数据超过10M放到.old文件中，同时将之前的old文件删除
    ///     3、日志支持高频写入，输入暂存到内存，每1分钟刷到文件，如果内存数据量大于64KB，将触发立即灌入文件
    ///     4、如果日志数据过于多，5S中总数据量超过524288（512KB），多出的数据将被丢弃
    ///     5、日志路径和日志名称支持简单动态配置
    ///     6、日志组件内部错误将以innerFatal方式也写入日志中
    ///     7、日志存在开关IsOn，可以动态开关日志
    ///     8、增加内部异常关闭自动关闭机制
    ///     9、日志增加归档archive功能，默认归档目录最大大小为100M
    /// </summary>
    public class TinyLog : IDLog
    {
        private static Thread mainThread = Thread.CurrentThread;

        private static bool? isOn;
        /// <summary>
        /// isStopLog
        /// </summary>
        public static bool IsOn
        {
            get
            {
                if (isOn.HasValue == false)
                {
                    isOn = globalConfig.isOn;
                }
                return isOn.Value;
            }
            set
            {
                isOn = value;
            }
        }

        private static bool? isArchiveOn;
        /// <summary>
        /// IsArchiveOn
        /// </summary>
        public static bool IsArchiveOn
        {
            get
            {
                if (isArchiveOn.HasValue == false)
                {
                    isArchiveOn = globalConfig.isAchiveOn;
                }
                return isArchiveOn.Value;
            }

            set
            {
                isArchiveOn = value;
            }
        }

        private StringBuilder sb = new StringBuilder();

        /// <summary>
        /// get conten which cached in memory
        /// </summary>
        public string LogContent { get { return sb.ToString(); } }

        private object lockObj = new object();

        private Thread thread;

        private DateTime? nextRunTime = null;

        private static TinyLogConfig globalConfig = new TinyLogConfig();

        private TinyLogConfig config = new TinyLogConfig();


        //error count
        private static int errorCount = 0;

        private static int archiveErrorCount = 0;

        private string logID;

        //random factor
        private static int factor = 100;

        /// <summary>
        /// logid
        /// </summary>
        private string LogID
        {
            get
            {
                if (logID == null)
                    logID = DateTime.Now.ToString("yyyyMMddhhmmssfff" + new Random(DateTime.Now.Millisecond + factor++).Next(100, 999));
                return logID;
            }
        }

        public TinyLog()
        {
            //10000
            sb = new StringBuilder(10000);
        }

        public TinyLog(string logName, string logFolderName=""):this() {
            this.Init(logName, logFolderName);
        }

        private static TinyLog _log = new TinyLog();

        /// <summary>
        /// get singlton instance of tinyLog
        /// </summary>
        /// <returns></returns>
        public static TinyLog GetInstance() { return _log; }

        public void Init(string logName, string logFolderName="")
        {
            config.logFileName = logName;
            if (string.IsNullOrEmpty(logFolderName) == false)
                config.logPath = logFolderName;
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

        private void InnerWrite(string head, string info)
        {
            if (!IsOn)
                return;
            lock (lockObj)
            {
                if (sb.Length < globalConfig.cacheLogMaxLength)
                {
                    sb.AppendLine("LogId:[" +LogID + "]" +string.Format(globalConfig.formatString, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), head, info));
                    if (sb.Length > globalConfig.cacheLogLength)
                    {
                        nextRunTime = DateTime.Now;
                    }
                }
                else
                {
                    nextRunTime = DateTime.Now;
                }

                //start when @ start time
                if (thread == null)
                {
                    thread = new Thread(new ThreadStart(writeSb2File));
                    //background thread
                    thread.IsBackground = false;
                    thread.Start();
                }
            }
        }

        private void writeSb2File()
        {
            while (true)
            {
                if (IsOn && sb.Length > 0)
                {
                    try
                    {
                        if (nextRunTime.HasValue == false)
                        {
                            nextRunTime = DateTime.Now.AddMilliseconds(globalConfig.refreshInternal);
                        }
                        if (DateTime.Now > nextRunTime)
                        {
                            FileSave();
                            nextRunTime = DateTime.Now.AddMilliseconds(globalConfig.refreshInternal);
                        }
                    }
                    catch (Exception ex)
                    {
                        InnerWrite("innerFatal", "errors when writing log data to file :" + ex.ToString());
                        innerError();
                    }
                }
                //Console.WriteLine(mainThread.IsAlive);
                if (!mainThread.IsAlive)
                {
                    FileSave();
                    break;
                }
                Thread.Sleep(globalConfig.unitInternal);
            }
        }

        private void FileSave()
        {
            string filePath = null;
            var strSb = "";
            //get path
            if (Path.IsPathRooted(config.logPath) == false)
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.logPath, config.logFileName);
            }
            else
            {
                filePath = Path.Combine(config.logPath, config.logFileName);
            }
            if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            if (File.Exists(filePath))
            {
                //judge file size , if bigger than config's size , save to .old 
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length > globalConfig.fileMaxLength)
                {
                    if (File.Exists(filePath + ".old"))
                    {
                        File.Delete(filePath + ".old");
                    }
                    fi.MoveTo(filePath + ".old");
                    if (IsArchiveOn)
                    {
                        var athread = new Thread(new ThreadStart(achive));
                        //background
                        athread.IsBackground = true;
                        athread.Start();
                    }
                    //write to file
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
        /// method of archive
        /// </summary>
        private void achive()
        {
            if (IsArchiveOn)
            {
                try
                {
                    string filePath = null;
                    // get right path
                    if (Path.IsPathRooted(config.logPath) == false)
                    {
                        filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.logPath, config.logFileName);
                    }
                    else
                    {
                        filePath = Path.Combine(config.logPath, config.logFileName);
                    }
                    var dir = Path.Combine(Path.GetDirectoryName(filePath), "archive");
                    //create archine dir
                    if (Directory.Exists(dir) == false)
                    {
                        Directory.CreateDirectory(dir);
                    }
                    //put .old to archive
                    var srcFile = filePath + ".old";
                    var tgtFile = Path.Combine(dir, Path.GetFileName(filePath) + ".old." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip");
                    config.comperessor.CompressData(srcFile, tgtFile);
                    File.Delete(srcFile);

                    //if size bigger than maxDirLength ,clear archive
                    if (GetDirSize(new DirectoryInfo(dir)) > config.archiveDirMaxLength)
                    {
                        ClearDir(dir);
                    }
                }
                catch (Exception ex)
                {
                    InnerWrite("innerFatal", "archive errors:" + ex.ToString());
                    innerAchiveError();
                }
                
            }
        }

        private void innerError()
        {
            errorCount++;
            if (errorCount > globalConfig.errorRetry)
            {
                InnerWrite("innerFatal", "innerErrorCount:" + errorCount + " and reset already");
                IsOn = false;
                errorCount = 0;
            }
        }

        /// <summary>
        /// error handler of archive error
        /// </summary>
        private void innerAchiveError()
        {
            archiveErrorCount++;
            if (archiveErrorCount > globalConfig.errorRetry)
            {
                InnerWrite("innerFatal", "innerErrorCount:" + archiveErrorCount + " and reset already");
                IsArchiveOn = false;
                archiveErrorCount = 0;
            }
        }

        /// <summary>
        /// get dir size 
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static long GetDirSize(DirectoryInfo d)
        {
            long Size = 0;
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // get of dir size
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += GetDirSize(di);  
            }
            return (Size);
        }

        /// <summary>
        /// clear dir
        /// </summary>
        /// <param name="srcPath"></param>
        public static void ClearDir(string srcPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos(); 
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)           
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);         
                    }
                    else
                    {
                        File.Delete(i.FullName);   
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

    }

    /// <summary>
    /// interface of compress
    /// </summary>
    internal interface ICompress
    {
        byte[] CompressData(byte[] data);
        string CompressData(string data);
        void CompressData(Stream inStream, Stream outStream);
        void CompressData(string srcFile, string tgtFile);
        void DeCompressData(string srcFile, string tgtFile);
        void DeCompressData(Stream inStream, Stream outStream);
        byte[] DeCompressData(byte[] data);
        string DeCompressData(string data);
    }

    internal class Gzip : ICompress
    {
        public byte[] CompressData(byte[] data)
        {
            using (MemoryStream msOut = new MemoryStream())
            {
                GZipStream gzs = new GZipStream(msOut, CompressionMode.Compress, true);
                gzs.Write(data, 0, data.Length);
                gzs.Close();
                return msOut.ToArray();
            }
        }

        public string CompressData(string data)
        {
            byte[] arrData = Encoding.UTF8.GetBytes(data);
            byte[] arrCompressed = CompressData(arrData);
            return Convert.ToBase64String(arrCompressed);
        }

        public void CompressData(Stream inStream, Stream outStream)
        {
            using (GZipStream gzs = new GZipStream(outStream, CompressionMode.Compress, true))
            {
                long leftLength = inStream.Length;
                byte[] buffer = new byte[4096];
                int maxLength = buffer.Length;
                int num = 0;
                int fileStart = 0;
                while (leftLength > 0)
                {
                    inStream.Position = fileStart;
                    if (leftLength < maxLength)
                    {
                        num = inStream.Read(buffer, 0, Convert.ToInt32(leftLength));
                        gzs.Write(buffer, 0, Convert.ToInt32(leftLength));
                    }
                    else
                    {
                        num = inStream.Read(buffer, 0, maxLength);
                        gzs.Write(buffer, 0, maxLength);
                    }
                    if (num == 0)
                    {
                        break;
                    }
                    fileStart += num;
                    leftLength -= num;
                }

            }
        }

        public void CompressData(string srcFile, string tgtFile)
        {
            if (File.Exists(srcFile) == false)
            {
                return;
            }
            if (File.Exists(tgtFile)) File.Delete(tgtFile);

            using (FileStream fs = new FileStream(srcFile, FileMode.Open))
            {
                using (FileStream tfs = new FileStream(tgtFile, FileMode.OpenOrCreate))
                {
                    CompressData(fs, tfs);
                }
            }
        }

        public void DeCompressData(string srcFile, string tgtFile)
        {
            if (File.Exists(srcFile) == false)
            {
                return;
            }
            if (File.Exists(tgtFile)) File.Delete(tgtFile);
            using (FileStream fs = new FileStream(srcFile, FileMode.Open))
            {
                using (FileStream tfs = new FileStream(tgtFile, FileMode.OpenOrCreate))
                {
                    DeCompressData(fs, tfs);
                }
            }
        }

        public void DeCompressData(Stream inStream, Stream outStream)
        {
            Stream msOut = outStream;
            using (GZipStream gzs = new GZipStream(inStream, CompressionMode.Decompress, true))
            {
                var bytes = new byte[4096];
                int count;
                while ((count = gzs.Read(bytes, 0, bytes.Length)) != 0)
                {
                    msOut.Write(bytes, 0, count);
                }
            }

        }

        public byte[] DeCompressData(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (MemoryStream outMs = new MemoryStream(data))
                {
                    DeCompressData(new MemoryStream(data), ms);
                    return ms.ToArray();
                }
            }
        }

        public string DeCompressData(string data)
        {
            byte[] arrData = Convert.FromBase64String(data);
            byte[] arrCompressed = DeCompressData(arrData);
            return Encoding.UTF8.GetString(arrCompressed);
        }
    }
}
