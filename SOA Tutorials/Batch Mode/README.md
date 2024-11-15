# HPC Pack 2019 SOA Tutorial: Batch Mode

## Introduction

This sample builds a batch mode service-oriented architecture (SOA) service (Prime FactorizationService), and a client application that interacts with the service, to run on a Microsoft high performance computing (HPC) cluster created by using Microsoft HPC Pack 2016. PrimeFactorizationService finds the prime factors of an integer and is an example of a more complex HPC algorithm.  To reduce the computation time, only small number should be passed to the service. The sample uses a durable session client to persist the requests and the responses, enabling reliable, long-running calculations.

## Building the Sample

Prerequisites:

- Client computer
  - Windows 10 or later
  - Visual Studio 2017 or later  

- Microsoft HPC cluster 
  - Head node running Windows Server 2016 or later
  - [Microsoft HPC Pack 2019 Update 1](https://www.microsoft.com/en-us/download/details.aspx?id=56360) or later

## Steps

1. Open BatchMode.sln with visual studio.

2. Open Program.cs and change headnode setting in both RequestSender and ResponseReceiver.

3. Build project Service and deploy the service
    a. Open BatchMode\PrimeFactorizationService\bin
    b. Copy PrimeFactorizationService.dll to a share folder which can be accessed by all compute nodes.
    c. Open PrimeFactorizationService.config and change assembly path to your share folder
    d. Copy PrimeFactorizationService.config to %CCP_HOME%ServiceRegistration/ on head node

4. Set RequestSender as startup project and run it.

5. Set ResponseReceiver as startup project and run it.