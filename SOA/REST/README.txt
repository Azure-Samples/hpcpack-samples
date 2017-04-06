Introduction
============
	The sample code shows how to enable REST(WebHttpBinding) in HPC V4 SOA service. As custom broker and custom web service host can be configured in service config file, they can be built separately against .net 4.5 framework which incorporates WebHttp binding. SOA on HPC can thus provide support for REST.


Prerequisites
=============
	1) Visual Studio 2015 or later

How to setup the system
=======================
	1) On a cluster, install HPC Pack and make sure there is at lease one broker node and one compute node. By default, the head node also works as a broker node and a compute node. So one machine is enough.

	2) Open REST.sln, and build solution.

	3) Copy EchoService\MyEchoService\bin\Debug\WebHttpDemo.dll to all compute nodes at C:\Services\WebHttpDemo.dll

	4) Copy Client\ClusterWebHttp\bin\Debug\ClusterWebHttp.exe to your desktop. 

	5) Copy CustomWebServiceHost\CustomWebServiceHost\bin\Debug\CustomWebServiceHost.exe to all compute nodes at C:\REST\CustomWebServiceHost.exe.

	6) Copy CustomBroker\bin\Debug\SampleBroker.exe to all broker nodes at C:\REST\SampleBroker.exe.

	7) Copy WebHttpDemo.config to \\$headNodeMachineName$\HpcServiceRegistration\WebHttpDemo.config.

	8) Add firewall rule(inbound and outbound) to allow TCP port 8088 on all nodes


How to run the tests
====================
Before running "ClusterWebHttp.exe", make sure below steps (for deploying required files on Azure) have ran:
	- hpcpack create EchoService.zip EchoService.dll,EchoService.config
	- hpcpack upload EchoService.zip /account:[storage name] /key:[storage keys]
	- Logon hpc on Azure header node, and run �clusrun hpcsync?
	- Create a user to run jobs, like �user name= testuser? and �password=abc123?

Execute "ClusterWebHttp.exe headNodeName" on your desktop.
