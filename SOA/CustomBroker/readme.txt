Pre-requisites
--------------

1. Client machine: XP, Vista, 7, 8 or 8.1 with the following software installed:
    - Microsoft HPC Pack 2016 Client Utilities
    - Visual Studio 2015 or later


2. Server machine: Windows HPC Server 2016

Steps
-----

1. You need Visual Studio 2015 to open the project file

2. Build the solution
   a. In Program.cs of CcpEchoClient project, replace "[headnode]" with your head node name.
   b. Launch Visual Studio 2015 and open the CustomBroker.sln file
   c. Build the solution by selecting Build->Rebuild Solution menu

3. Deploy the custom broker assembly

   a. Goto bin folder and copy SampleBroker.exe to "C:\" on broker node of your cluster
   b. Goto \\<headnode>\ServiceRegistration folder, find CCPEchoSvc.config and uncomment the following section to enable custom broker:
    <!--<customBroker executive="C:\SampleBroker.exe">
      <environmentVariables>
        <add name="myname1" value="myvalue1"/>
        <add name="myname2" value="myvalue2"/>
      </environmentVariables>
    </customBroker>-->


4. Run ccpEchoClient.exe to test the custom broker. It uses basic v2 client application through CCPEchoSvc. 

