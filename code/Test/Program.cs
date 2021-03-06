﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestThread();
            //Test();
            TestMulti();
            //TestEasyUse();
        }

        static void TestEasyUse() {
            var tl = new TinyLog.TinyLog("tiny.log");
            tl.Error("abc");
        }

        static void Test()
        {
            var log = new TinyLog.TinyLog();
            log.Init("a.log", "");
            for (int i = 0; i < 100000000; i++)
            {
                log.Debug("【" + i + "】aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                log.Info("【" + i + "】aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                log.Warn("【" + i + "】aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                log.Error("【" + i + "】aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                log.Fatal("【" + i + "】aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                Thread.Sleep(10);
            }
        }

        static TinyLog.TinyLog tl = null;

        static void TestThread()
        {
            tl = new TinyLog.TinyLog();
            tl.Init("b.log", "");
            for (int i = 0; i < 10; i++)
            {
                new Thread(new ParameterizedThreadStart(Run)).Start(i);
            }

        }

        static void Run(object obj)
        {
            for (int i = 0; i < 100000000; i++)
            {
                tl.Debug("线程" + obj + "执行第" + i + "次sdddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
                Thread.Sleep(10);
            }
        }

        static void TestMulti() {
            for (int i = 0; i < 10; i++)
            {
                new Thread(new ParameterizedThreadStart(mutlti)).Start(i);
            }
        }

        static void mutlti(object obj) {
            var logName = "test" + obj + ".log";
            var log = new TinyLog.TinyLog();
            log.Init(logName, "TinyLogs/abc");
            for (int i = 0; i < 1000000; i++)
            {
                log.Debug(logName + "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
                Thread.Sleep(10);
            }

        }
    }
}
