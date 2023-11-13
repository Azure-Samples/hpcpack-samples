# Introduction
WCF Broker sample code: hello world R2 .Net version

# Build
- Clone the repository to the local disk.
- Open "HellowWorldR2.sln" and compile the "Release" version of the code. The result is that EchoService.dll gets created under HelloWorldR2\EchoService\bin.

# Deploy
## In HPC cluster (preferred)
- Copy EchoService.dll (HelloWorldR2\EchoService\bin) to every compute node under the folder C:\ServicesR2.
- HelloWorldR2\EchoService\EchoService.config "assembly" entry to make it point to the full DLL path (e.g. C:\ServicesR2\EchoService.dll).
- Copy HelloWorldR2\EchoService\EchoService.config to the headnode’s %CCP_HOME%\ServiceRegistration.

## In Azure
- Copy EchoService.dll (HelloWorldR2\EchoService\bin) and EchoService.config (HelloWorldR2\EchoService\EchoService.config) to the folder C:\ServicesR2.
- Use hpcpack to create the service package: c:\ServiceR2\hpcpack create EchoService.zip EchoService.config,EchoService.dll.
- Upload the hpcpack to upload the service package: C:\ServiceR2\hpcpack upload EchoService.zip /account:[service] /key:[storagekey]
- On the Azure Cluster, run clusrun to deploy the service: clusrun hpcsync [service]  [storagekey] ^%ccp_package_root^%

# Source code
There are 9 client samples in HelloWorldR2.sln. Run the sample exes in admin role.
- HelloWorldR2 demos how to use the new Session API provided by HPC Server R2 to send requests and get responses on a HPC Server R2 cluster.
- HelloWorldR2Linq demos how to use Linq for querying responses.
- HelloWorldR2ErrorHandle demos how to write a reliable client by dealing with exceptions and retrying.
- HelloWorldR2Callback is the same as HelloWorldR2 besides it also demos how to get result by registering a callback.
- HelloWorldR2MultiBatch is the same as HelloWorldR2 besides it also demos how to share a session between multiple clients (or client applications) by specifying a unique client id when creating and attaching clients.
- FireNRecollect demos how to send requests in one process and coming back to retrieve the result using AttachSession in another process.
- HelleWorldR2CancelReuqests demos how to cancel the processing requests in a broker client by close the client with purging.
- HelloWorldR2OnExit/EchoService demos how to register the OnExiting handler for the service host to do cleanup work when it is gracefully closed.
- HelloWorldR2ServiceVersioning shows how to use services of different versions which are specified by the service config files on the cluster side.
- HellowWorldR2SessionPool demos how to use session pool to shorten the time for creating sessions
- InprocessBroker demos how to create an interactive session with inprocess broker instead of a dedicated broker node.
- HellowWorldR2ReliableBrokerClient demos how to use reliable broker client for durable sessions to handle the network failures when sending/flushing/ending requests or getting responses.

# How to run in non-domain joined HPC cluster
- In EchoService.config, find binding name="Microsoft.Hpc.SecureNetTcpBrokerBinding", in security mode, change Transport to None. The same for binding name="Microsoft.Hpc.BackEndBinding".
- In sample code, remember to add "info.Secure = false;" and "NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);".
- HelloWorldR2ServiceVersioning and InprocessBroker are only abailable for domain-joined cluster.
