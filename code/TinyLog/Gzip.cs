using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace TinyLog
{
    internal class Gzip : ICompress
    {
        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="data">数据字节流</param>
        /// <returns>压缩后的数据</returns>
        public byte[] CompressData(byte[] data)
        {
            using (MemoryStream msOut = new MemoryStream())
            {
                GZipStream gzs = new GZipStream(msOut, CompressionMode.Compress, true);
                gzs.Write(data, 0, data.Length);//压缩并写入内存
                gzs.Close();
                return msOut.ToArray();
            }
        }

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>压缩后数据</returns>
        public string CompressData(string data)
        {
            byte[] arrData = Encoding.UTF8.GetBytes(data);
            byte[] arrCompressed = CompressData(arrData);
            return Convert.ToBase64String(arrCompressed);
        }

        /// <summary>
        /// 大文件压缩
        /// </summary>
        /// <param name="inStream">入流</param>
        /// <param name="outStream">出流</param>
        /// <returns></returns>
        public void CompressData(Stream inStream, Stream outStream)
        {
            using (GZipStream gzs = new GZipStream(outStream, CompressionMode.Compress, true))
            {
                //还没有读取的文件内容长度
                long leftLength = inStream.Length;
                //创建接收文件内容的字节数组
                byte[] buffer = new byte[4096];
                //每次读取的最大字节数
                int maxLength = buffer.Length;
                //每次实际返回的字节数长度
                int num = 0;
                //文件开始读取的位置
                int fileStart = 0;
                while (leftLength > 0)
                {
                    //设置文件流的读取位置
                    inStream.Position = fileStart;
                    if (leftLength < maxLength)
                    {
                        num = inStream.Read(buffer, 0, Convert.ToInt32(leftLength));
                        //写入
                        gzs.Write(buffer, 0, Convert.ToInt32(leftLength));
                    }
                    else
                    {
                        num = inStream.Read(buffer, 0, maxLength);
                        //写入
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

        /// <summary>
        /// 压缩文件，支持大文件压缩
        /// </summary>
        /// <param name="srcFile">源文件</param>
        /// <param name="tgtFile">目标文件</param>
        /// <returns></returns>
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

        /// <summary>
        /// 解压文件，支持大文件压缩
        /// </summary>
        /// <param name="srcFile">源文件</param>
        /// <param name="tgtFile">目标文件</param>
        /// <returns></returns>
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

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="inStream">入流</param>
        /// <param name="outStream">出流</param>
        /// <returns>解压后数据</returns>
        public void DeCompressData(Stream inStream, Stream outStream)
        {
            Stream msOut = outStream;
            using (GZipStream gzs = new GZipStream(inStream, CompressionMode.Decompress, true))
            {
                //gzs.Write(data, 0, data.Length);//压缩并写入内存

                var bytes = new byte[4096];
                int count;
                while ((count = gzs.Read(bytes, 0, bytes.Length)) != 0)
                {
                    msOut.Write(bytes, 0, count);
                }
            }

        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="data">数据字节数组</param>
        /// <returns>解压后数据</returns>
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
            //using (MemoryStream msOut = new MemoryStream())
            //{
            //    GZipStream gzs = new GZipStream(new MemoryStream(data), CompressionMode.Decompress, true);
            //    //gzs.Write(data, 0, data.Length);//压缩并写入内存

            //    var bytes = new byte[4096];
            //    int count;
            //    while ((count = gzs.Read(bytes, 0, bytes.Length)) != 0)
            //    {
            //        msOut.Write(bytes, 0, count);
            //    }
            //    gzs.Close();
            //    return msOut.ToArray();
            //}
        }



        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="data">数据/param>
        /// <returns>解压后数据</returns>
        public string DeCompressData(string data)
        {
            byte[] arrData = Convert.FromBase64String(data);
            byte[] arrCompressed = DeCompressData(arrData);
            return Encoding.UTF8.GetString(arrCompressed);
        }
    }
}
