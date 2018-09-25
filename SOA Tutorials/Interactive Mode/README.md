# HPC Pack 2016 SOA Tutorial: Interactive Mode

## Introduction

This sample builds an interactive (real-time mode) service-oriented architecture (SOA) service, and a client application that interacts with the service, to run on a Windows high performance computing (HPC) cluster created by using Microsoft HPC Pack 2016. PrimeFactorizationService finds the prime factors of an integer and is an example of a more complex HPC algorithm. To reduce the computation time, only small number should be passed to the service. The sample uses a client that can reuse an existing session, reducing the startup time for subsequent sessions.

## Building the Sample

Prerequisites:

- Client computer
  - Windows 8 or later
  - Visual Studio 2017 or later  
- Microsoft HPC cluster 
  - Head node running Windows Server 2016
  - [Microsoft HPC Pack 2016 Update 1](https://www.microsoft.com/en-us/download/details.aspx?id=56360) or later

## Steps

1. Open RealTimeMode.sln with visual studio.
2. Open Program.cs and change headnode setting in both RequestSender and ResponseReceiver.
3. Build project Service and deploy the service
   a. Open BatchMode\PrimeFactorizationService\bin
     b. Copy PrimeFactorizationService.dll to a share folder which can be accessed by all compute nodes.
     c. Open PrimeFactorizationService.config and change assembly path to your share folder
     d. Copy PrimeFactorizationService.config to %CCP_HOME%ServiceRegistration/ on head node
4. Set RequestSender as startup project and run it.
5. Set ResponseReceiver as startup project and run it.