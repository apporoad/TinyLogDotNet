# TinyLogDotNet
tinyLog in c#

# 中文见
[参见](https://github.com/apporoad/TinyLogDotNet/blob/master/README_CN.md "detail")

# use case:
	component in any program  
	easy to integrate  
	some test scene  
	it can be used as a inpendent dll ,or integrated into your program  

# design details:
	1. log format is very easy, all data is saved in one file  
	2. log file will be split , when data is more than 10M , old data will be copy to .old file  
	3. you can write log in high frequency , data will be cached in memory , when data is bigger enouph or internal time up , will save to file  
	4. if data very huge , part data will be ignored  
	5. log file path and name ,support dynamic config   
	6. inner error will be logged in your log  with key [innerFatal]
	7. you can stop or start log with IsOn  
	8. when innerErrors keep , log will stop auto 
	9. log will be archive,log will be compressed in Gzip , default archive length is 100M
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
