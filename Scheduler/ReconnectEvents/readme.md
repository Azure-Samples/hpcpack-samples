 # Introduction
 This sample will submit jobs every 2 seconds to the cluster. It will stop doing so when the scheduler is disconnected. Disconnect by ending process HpcScheduler.exe (In Task Manageer -> More Details -> Details tab -> find HpcScheduler.exe -> End Task). HpcScheduler.exe will restart automatically after a while. Then, it will submit jobs again.
 
 # How to build
Target on both .Net 8 and .Net Framework 4.7.2
```powershell
dotnet build
```

Target on .Net 8
```powershell
dotnet build -f net8.0
```

Target on .Net Framework 4.7.2
```powershell
dotnet build -f net472
```
