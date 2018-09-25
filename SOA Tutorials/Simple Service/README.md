# HPC Pack 2016 SOA Tutorial: Simple Service

## Introduction

This sample builds a simple service-oriented architecture (SOA) service (CalculatorService), and a client application that interacts with the service, to run on a Windows high performance computing (HPC) cluster created by using Microsoft HPC Pack 2016. CalculatorService performs a simple addition to demonstrate the SOA programming model.

With SOA, distinct computational functions are packaged as software modules called services. Unlike traditional HPC applications, an HPC SOA service exposes its functionality through a well-defined service interface which allows any application or process on the network to access the functionality of the service. Developers can implement service functionality by using any programming language, and can write original services or package existing dynamic-link libraries (DLLs) and IT investments as services.

## Building the Sample

Prerequisites:

- Client computer
  - Windows 8 or later
  - Visual Studio 2017 or later  
- Microsoft HPC cluster 
  - Head node running Windows Server 2016
  - [Microsoft HPC Pack 2016 Update 1](https://www.microsoft.com/en-us/download/details.aspx?id=56360) or later

## Steps

1. Open FirstSOAService.sln with visual studio.

2. Open Program.cs and change headnode setting in following code

     ```csharp
     SessionStartInfo info = new SessionStartInfo("head.contoso.com", "CalculatorService");
     ```

3. Build project Service and deploy the service
  a. Open CalculatorService\bin
  b. Copy CalculatorService.dll to a share folder which can be accessed by all compute nodes.
  c. Open CalculatorService.config and change assembly path to your share folder
  d. Copy CalculatorService.config to %CCP_HOME%ServiceRegistration/ on head node

4. Set Client as startup project and run it.