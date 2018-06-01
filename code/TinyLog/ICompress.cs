using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TinyLog
{
    /// <summary>
    /// 压缩接口，从tickDotNet中集成过来
    /// </summary>
    internal interface ICompress
    {
        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="data">数据字节流</param>
        /// <returns>压缩后的数据</returns>
        byte[] CompressData(byte[] data);

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>压缩后数据</returns>
        string CompressData(string data);

        /// <summary>
        /// 大文件压缩
        /// </summary>
        /// <param name="inStream">入流</param>
        /// <param name="outStream">出流</param>
        /// <returns></returns>
        void CompressData(Stream inStream, Stream outStream);
        /// <summary>
        /// 压缩文件，支持大文件压缩
        /// </summary>
        /// <param name="srcFile">源文件</param>
        /// <param name="tgtFile">目标文件</param>
        /// <returns></returns>
        void CompressData(string srcFile, string tgtFile);

        /// <summary>
        /// 解压文件，支持大文件压缩
        /// </summary>
        /// <param name="srcFile">源文件</param>
        /// <param name="tgtFile">目标文件</param>
        /// <returns></returns>
        void DeCompressData(string srcFile, string tgtFile);

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="inStream">入流</param>
        /// <param name="outStream">出流</param>
        /// <returns>解压后数据</returns>
        void DeCompressData(Stream inStream, Stream outStream);

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="data">数据字节数组</param>
        /// <returns>解压后数据</returns>
        byte[] DeCompressData(byte[] data);



        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="data">数据/param>
        /// <returns>解压后数据</returns>
        string DeCompressData(string data);
    }
}
