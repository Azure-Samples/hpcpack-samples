# Introduction
HPC Scheduler REST API Samples.
This set of sample sources provide an introduction to the programming model available to the latest REST API for the HPC Scheduler.

# Description
Examples include:
- Authentication between client and REST server using Basic Authentication or Default netowrk credentials or the AAD Authentication.
- Create a job
- Create a task
- Submit a task
- Check the state of the job

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

# Usage
The username and password should be wrapped with double quotation marks.
CSharpClient2016 -c &lt;cluster_name&gt; -u "&lt;domain_name&gt;\\&lt;user_name&gt;" -p "&lt;password&gt;"

# Node
This is the old version of REST api whose url starts with `WindowsHpc`.
View [CSharpClient2016](https://github.com/Azure-Samples/hpcpack-samples/tree/master/Scheduler/REST/CSharpClient2016) for the latest version of REST API whose url starts with `hpc`.
