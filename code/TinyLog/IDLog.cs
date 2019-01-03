namespace TinyLog
{
    /// <summary>
    /// dynamic log interface 
    /// </summary>
    public interface IDLog
    {
        /// <summary>
        /// init log
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="logFolderName"></param>
        void Init(string logName, string logFolderName);

        void Error(string strInfo);

        void Debug(string strInfo);

        void Fatal(string strInfo);

        void Info(string strInfo);

        void Warn(string strInfo);
    }
}
