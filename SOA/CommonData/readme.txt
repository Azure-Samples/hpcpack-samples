Pre-requisites
--------------

1. Client machine: XP, Vista, 7, 8 or 8.1 with the following software installed:
    - Microsoft HPC Pack 2016 Client Utilities
    - Visual Studio 2015 or later


2. Server machine: Windows HPC Server 2016

Steps
-----

1. You need Visual Studio 2015 to open the project file

2. Change the head node in the source code, Program.cs in CommonDataClient project.  Find the following line:
     
        public static string headNode = "[headnode]";

    Replace "[headnode]" with the name of your head node.

3. Build the solution
   a. Launch Visual Studio 2015 and open the CommonDataSample.sln file
   b. Build the solution by selecting Build->Rebuild Solution menu

4. Deploy the custom broker assembly

   a. Goto CommonData\CommonDataService\bin\debug folder
   b. Copy CommonDataService.dll to C:\Services\ on each compute node
   c. Copy CommonDataService.config to C:\program files\Microsoft HPC Pack 2016\ServiceRegistration on head node

5. Share the headnode folder "C:\HPCRuntimeDirectory" and name it "Runtime" in network

6. Run CommonDataClient.exe

