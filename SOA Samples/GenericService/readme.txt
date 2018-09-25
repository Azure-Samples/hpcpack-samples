Pre-requisites
--------------

1. Client machine: XP, Vista, 7, 8 or 8.1 with the following software installed:
    - Microsoft HPC Pack 2016 Client Utilities
    - Visual Studio 2015 or later


2. Server machine: Windows HPC Server 2016

Steps
-----

1. Open GenericService\GenericService.sln with Visual Studio

2. Build and deploy the service
   a. Right click the project and select Build
   b. Copy GenericService\GenericService.config to %CCP_HOME%ServiceRegistration/ folder on head node
   c. Copy GenericService\bin\GenericService.dll to C:\Services on each compute node

3. Open GenericServiceClientApp\GenericServiceClientApp.sln with Visual Studio

3. Change the head node in the source code, Program.cs.  Find the following line:
     
            SessionStartInfo startInfo = new SessionStartInfo("[headnode]", "GenericService");

    Replace "[headnode]" with the name of your head node.

4. Build the project and run GenericServiceClientApp.exe
