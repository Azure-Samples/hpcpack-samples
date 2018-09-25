# HPC Pack 2016 SOA Tutorial: Common Data

## Introduction

This sample builds a sample service-oriented architecture (SOA) service that makes use of common data, and a client application that interacts with the service, to run on a Microsoft high performance computing (HPC) cluster created by using Microsoft HPC Pack 2016. PrimeFactorizationService finds the prime factors of an integer and is an example of a more complex HPC algorithm.  To accelerate the algorithm, the sample employs a prime number table that is stored as common data that is accessible to the service on all of the compute nodes.

## Building the Sample

Prerequisites:

- Client computer
  - Windows 8 or later
  - Visual Studio 2017 or later  
- Microsoft HPC cluster 
  - Head node running Windows Server 2016
  - [Microsoft HPC Pack 2016 Update 1](https://www.microsoft.com/en-us/download/details.aspx?id=56360) or later

## Steps

1. Open CommonDataService.sln with visual studio.
2. Open Program.cs and change headnode setting in Client and DataManager

3. Build project Service and deploy the service
  a. Open PrimeFactorizationService\bin
  b. Copy CommonData.PrimeFactorization.dll to a share folder which can be accessed by all compute nodes.
  c. Open CommonData.PrimeFactorization.config and change assembly path to your share folder
  d. Copy CommonData.PrimeFactorization.config to %CCP_HOME%ServiceRegistration/ on head node

4. Set DataManager as startup project and run it. 

5. Set Client as startup project and run it.