# TinyLogDotNet
微型日志的C#实现 

# 适用场景：
	底层组件，方便集成到独立dll  
	简单小项目
	不想用大的日志框架的测试场景

# 使用说明  
	可以编译成独立组件，也可以直接集成到项目中
# 详细说明：
	1. 日志格式非常简单. 所有数据存在一个文件中
	2. 日志文件支持分割，如果数据超过10M放到. old文件中，同时将之前的old文件删除
	3. 日志支持高频写入，输入暂存到内存，每1分钟刷到文件，如果内存数据量大于64KB，将触发立即灌入文件
	4. 如果日志数据过于多，5S中总数据量超过524288（512KB），多出的数据将被丢弃
	5. 日志路径和日志名称支持简单动态配置
	6. 日志组件内部错误将以innerFatal方式也写入日志中
	7. 日志存在开关IsOn，可以动态开关日志
	8. 增加内部异常关闭自动关闭机制
	9. 日志增加归档archive功能，默认归档目录最大大小为100M
# how to use

```c#

var log = new TinyLog.TinyLog("tiny.log");
log.Debug("debug");
log.Info("info");
log.Warn("warn");
log.Error("error");
log.Fatal("fatal");
// your can find log in TinyLogs/tiny.log

```
