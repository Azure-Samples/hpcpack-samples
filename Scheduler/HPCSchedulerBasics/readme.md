## Introduction
This sample code provides basic usage of HPC Scheduler. You can call
```powershell
.\HPCSchedulerBasics.exe [-u <user name>] [-c <cluster name>] [-d]
```
-u Required. It provides the username to connect to the cluster.  
-c Optional. It provides the HPC cluster name. If you don't provide it, the default value will be %CCP_SCHEDULER%.  
-d Optional. Runs in debug mode, and it will print more useful information for debugging.

## How to build
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
