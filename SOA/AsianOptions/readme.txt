Pre-requisites
--------------

1. Client machine: XP, Vista, 7 , 8 or 8.1 with the following software installed:
    - Microsoft HPC Pack 2016 R2 Client Utilities
    - Office 2016
    - Visual Studio 2015 or later

2. Server machine: Windows HPC Server 2016

Steps
-----

1. You need Visual Studio 2015 to open the project solution file
2. Change the head node in the source code, Config.cs in AsianOptions project.  Find the following line:

     
        public static string headNode = "[headnode]";

    Replace "[headnode]" with the name of your head node.


3. Build the solution
   a. Launch Visual Studio 2015 and open the AsianOptions.sln file
   b. Build the solution by selecting Build->Rebuild Solution menu

4. Deploy the service

   a. Goto AsianOptions\AsianOptionsService\bin
   b. Copy AsianOptionsService.dll to C:\Services\ on each compute node
   c. Copy AsianOptionsService.config to c:\program files\Microsoft HPC Pack 2016\ServiceRegistration on head node

5. Set AsianOptions as startup project. From Visual Studio, select Debug -> Start without Debugging, an Excel Spreadsheet pops up,
   Click "Run" in sheet1 to run the Asian Options Pricing model

