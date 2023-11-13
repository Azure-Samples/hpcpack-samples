# Introduction
This sample describes the process of modifying a job's progress information and message using HPC Pack 2016+ APIs.

# Description
The sample is set up to send jobs to the scheduler defined by the CCP_SCHEDULER environment variable, which is set up automatically when installing the Microsoft HPC Pack 2016+ server (Set the CCP_SCHEDULER if you are using Microsoft HPC Pack 2016+ client utilities to configure the scheduler).
It shows the general process of submitting a job to a cluster, running the job, modifying the process, and waiting for the job to finsih. A job is submitted with an "echo *" Parametric Sweep task which echos number 1-500, and use an event handler, wait for the job status to change, and print corresponding information.