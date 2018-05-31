using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyLog
{
    /// <summary>
    /// 动态日志接口
    /// </summary>
    public interface IDLog
    {
        /// <summary>
        /// 初始化日志
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="logFolderName"></param>
        void Init(string logName, string logFolderName);
        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="strInfo">日志对应信息</param>
        void Error(string strInfo);

        /// <summary>
        /// 写用来查问题时看的日志（调试日志）
        /// </summary>
        /// <param name="strInfo">日志对应信息</param>
        void Debug(string strInfo);

        /// <summary>
        /// 写致命或重大错误日志
        /// </summary>
        /// <param name="strInfo">日志对应信息</param>
        void Fatal(string strInfo);

        /// <summary>
        /// 写描述日志
        /// </summary>
        /// <param name="strInfo">日志对应信息</param>
        void Info(string strInfo);

        /// <summary>
        /// 写警告日志
        /// </summary>
        /// <param name="strInfo">日志对应信息</param>
        void Warn(string strInfo);
    }
}
